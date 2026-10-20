"""API de la nube: recibe pedidos y los deja en Postgres + outbox (nunca habla con SQL Server)."""
import os
from uuid import UUID

import psycopg
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field

DATABASE_URL = os.environ["DATABASE_URL"]
app = FastAPI(title="Clean Expo — API nube")


def db():
    return psycopg.connect(DATABASE_URL)


class Item(BaseModel):
    id_producto: int
    cantidad: int = Field(gt=0)


class NuevoPedido(BaseModel):
    id_persona: int
    usuario_id: int
    items: list[Item] = Field(min_length=1)


@app.get("/health")
def health():
    with db() as conn:
        conn.execute("SELECT 1")
    return {"status": "ok"}


@app.post("/orders", status_code=202)
def crear_pedido(body: NuevoPedido):
    """Guarda el pedido como 'recibido' y encola el envío. El precio sale SIEMPRE de precio_cliente (servidor)."""
    with db() as conn, conn.transaction():
        cli = conn.execute(
            "SELECT sucursal_id FROM cliente WHERE id_persona = %s", (body.id_persona,)
        ).fetchone()
        if not cli:
            raise HTTPException(404, "cliente no existe")
        precios = dict(
            conn.execute(
                "SELECT id_producto, precio FROM precio_cliente WHERE id_persona = %s AND id_producto = ANY(%s)",
                (body.id_persona, [i.id_producto for i in body.items]),
            ).fetchall()
        )
        faltan = [i.id_producto for i in body.items if i.id_producto not in precios]
        if faltan:
            raise HTTPException(422, f"sin precio para productos {faltan}")
        pedido_id = conn.execute(
            "INSERT INTO pedido (id_persona, usuario_id, sucursal_id) VALUES (%s, %s, %s) RETURNING id",
            (body.id_persona, body.usuario_id, cli[0]),
        ).fetchone()[0]
        for i in body.items:
            conn.execute(
                "INSERT INTO pedido_item (pedido_id, id_producto, unitario, descuento, cantidad) VALUES (%s,%s,%s,0,%s)",
                (pedido_id, i.id_producto, precios[i.id_producto], i.cantidad),
            )
        conn.execute("INSERT INTO outbox (pedido_id) VALUES (%s)", (pedido_id,))
    return {"id": str(pedido_id), "estado": "recibido"}


@app.get("/orders/{pedido_id}")
def estado_pedido(pedido_id: UUID):
    with db() as conn:
        row = conn.execute(
            "SELECT estado, id_pedido_mr FROM pedido WHERE id = %s", (pedido_id,)
        ).fetchone()
        if not row:
            raise HTTPException(404, "pedido no existe")
        items = conn.execute(
            "SELECT id_producto, cantidad, surtido FROM pedido_item WHERE pedido_id = %s", (pedido_id,)
        ).fetchall()
    return {
        "id": str(pedido_id),
        "estado": row[0],
        "id_pedido_mr": row[1],
        "items": [{"id_producto": p, "cantidad": c, "surtido": s} for p, c, s in items],
    }
