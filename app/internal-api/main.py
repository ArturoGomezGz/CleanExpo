"""API interna (zona MR). En desarrollo el 'puente ExpoMR' se simula con una base Postgres llamada bridge.
En producción esta capa escribe en ExpoMR.mPedidos/dPedidos (SQL Server) y ejecuta Indigo.dbo.FacturarExpo
de la sucursal del cliente. Es stateless: la idempotencia vive en la base, así que sirve con N réplicas."""
import os
import socket
from contextlib import asynccontextmanager
from uuid import UUID

import psycopg
from fastapi import Depends, FastAPI, Header, HTTPException
from pydantic import BaseModel, Field

BRIDGE_DATABASE_URL = os.environ["BRIDGE_DATABASE_URL"]
API_KEY = os.environ["INTERNAL_API_KEY"]
REPLICA = socket.gethostname()


def db():
    return psycopg.connect(BRIDGE_DATABASE_URL)


@asynccontextmanager
async def lifespan(_: FastAPI):
    with db() as conn:
        conn.execute("SELECT pg_advisory_xact_lock(42)")  # evita carrera entre réplicas al crear la tabla
        conn.execute(
            """CREATE TABLE IF NOT EXISTS bridge_pedido (
                 id_pedido_mr serial PRIMARY KEY,
                 order_id uuid NOT NULL UNIQUE,
                 sucursal smallint NOT NULL,
                 payload jsonb NOT NULL,
                 creado_en timestamptz NOT NULL DEFAULT now())"""
        )
    yield


app = FastAPI(title="Clean Expo — API interna", lifespan=lifespan)


def require_key(x_api_key: str = Header(default="")):
    if x_api_key != API_KEY:
        raise HTTPException(401, "api key inválida")


class Item(BaseModel):
    idProducto: int
    cantidad: int = Field(gt=0)
    unitario: float
    descuento: float = 0


class Pedido(BaseModel):
    orderId: UUID
    idPersona: int
    idEmail: int
    sucursal: int = Field(ge=1, le=3)
    items: list[Item] = Field(min_length=1)
    model_config = {"extra": "allow"}


def facturar(pedido: Pedido):
    """Simulación de FacturarExpo: surte todo. El real factura solo lo que alcanza en inventario."""
    return {
        "facturas": [{"sucursal": pedido.sucursal, "renglones": len(pedido.items)}],
        "surtido": [{"idProducto": i.idProducto, "surtido": i.cantidad, "pendiente": 0} for i in pedido.items],
    }


@app.get("/health")
def health():
    with db() as conn:
        conn.execute("SELECT 1")
    return {"status": "ok", "replica": REPLICA}


@app.post("/orders", dependencies=[Depends(require_key)])
def colocar_pedido(pedido: Pedido, idempotency_key: UUID = Header(...)):
    if idempotency_key != pedido.orderId:
        raise HTTPException(400, "Idempotency-Key debe ser igual a orderId")
    with db() as conn, conn.transaction():
        nuevo = conn.execute(
            """INSERT INTO bridge_pedido (order_id, sucursal, payload) VALUES (%s, %s, %s::jsonb)
               ON CONFLICT (order_id) DO NOTHING RETURNING id_pedido_mr""",
            (pedido.orderId, pedido.sucursal, pedido.model_dump_json()),
        ).fetchone()
        if nuevo:
            return _respuesta(nuevo[0], pedido, 201)
        existente = conn.execute(
            "SELECT id_pedido_mr, payload FROM bridge_pedido WHERE order_id = %s", (pedido.orderId,)
        ).fetchone()
        # Reintento: se responde con lo ya guardado, no con lo que mande esta segunda petición.
        return _respuesta(existente[0], Pedido.model_validate(existente[1]), 200)


def _respuesta(id_pedido_mr: int, pedido: Pedido, status: int):
    from fastapi.responses import JSONResponse

    return JSONResponse(
        {"idPedidoMr": id_pedido_mr, "servidoPor": REPLICA, **facturar(pedido)}, status_code=status
    )


@app.get("/orders/{order_id}/status", dependencies=[Depends(require_key)])
def estado(order_id: UUID):
    with db() as conn:
        row = conn.execute(
            "SELECT id_pedido_mr, payload FROM bridge_pedido WHERE order_id = %s", (order_id,)
        ).fetchone()
    if not row:
        raise HTTPException(404, "pedido no existe")
    pedido = Pedido.model_validate(row[1])
    return {"idPedidoMr": row[0], **facturar(pedido)}
