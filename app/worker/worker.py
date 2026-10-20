"""Worker de outbox: envía pedidos pendientes a la API interna con reintentos e idempotencia."""
import os
import time

import httpx
import psycopg

DATABASE_URL = os.environ["DATABASE_URL"]
INTERNAL_API_URL = os.environ["INTERNAL_API_URL"].rstrip("/")
INTERNAL_API_KEY = os.environ["INTERNAL_API_KEY"]
POLL_SECONDS = float(os.environ.get("POLL_SECONDS", "2"))
MAX_BACKOFF_SECONDS = 300


def payload(conn, pedido_id):
    p = conn.execute(
        """SELECT p.id_persona, u.id_email, p.sucursal_id, p.id_documento, p.id_uso_cfdi, p.id_tipo_venta,
                  p.id_transporte, p.id_porte, p.id_direccion_fiscal, p.id_direccion_entrega, p.observaciones
           FROM pedido p JOIN usuario u ON u.id = p.usuario_id WHERE p.id = %s""",
        (pedido_id,),
    ).fetchone()
    items = conn.execute(
        "SELECT id_producto, cantidad, unitario, descuento FROM pedido_item WHERE pedido_id = %s", (pedido_id,)
    ).fetchall()
    return {
        "orderId": str(pedido_id),
        "idPersona": p[0], "idEmail": p[1], "sucursal": p[2],
        "idDocumento": p[3], "idUsoCfdi": p[4], "idTipoVenta": p[5],
        "idTransporte": p[6], "idPorte": p[7], "fiscal": p[8], "entrega": p[9], "observaciones": p[10],
        "items": [
            {"idProducto": i, "cantidad": c, "unitario": float(u), "descuento": float(d)} for i, c, u, d in items
        ],
    }


def procesar_uno(conn, client):
    with conn.transaction():
        row = conn.execute(
            """SELECT id, pedido_id, intentos FROM outbox
               WHERE enviado_en IS NULL AND siguiente_intento <= now()
               ORDER BY id FOR UPDATE SKIP LOCKED LIMIT 1"""
        ).fetchone()
        if not row:
            return False
        outbox_id, pedido_id, intentos = row
        try:
            r = client.post(
                f"{INTERNAL_API_URL}/orders",
                json=payload(conn, pedido_id),
                headers={"X-Api-Key": INTERNAL_API_KEY, "Idempotency-Key": str(pedido_id)},
                timeout=20,
            )
            if r.status_code in (200, 201):
                data = r.json()
                conn.execute("UPDATE outbox SET enviado_en = now(), ultimo_error = NULL WHERE id = %s", (outbox_id,))
                conn.execute(
                    "UPDATE pedido SET estado = 'sincronizado', id_pedido_mr = %s WHERE id = %s",
                    (data["idPedidoMr"], pedido_id),
                )
                for s in data.get("surtido", []):
                    conn.execute(
                        "UPDATE pedido_item SET surtido = %s, colocado = %s WHERE pedido_id = %s AND id_producto = %s",
                        (s["surtido"], s["pendiente"] == 0, pedido_id, s["idProducto"]),
                    )
                print(f"pedido {pedido_id} sincronizado -> {data['idPedidoMr']}", flush=True)
                return True
            if r.status_code == 409:  # sin crédito / producto deshabilitado: no tiene caso reintentar solo
                conn.execute("UPDATE outbox SET enviado_en = now(), ultimo_error = %s WHERE id = %s", (r.text, outbox_id))
                conn.execute("UPDATE pedido SET estado = 'error' WHERE id = %s", (pedido_id,))
                print(f"pedido {pedido_id} rechazado: {r.text}", flush=True)
                return True
            raise RuntimeError(f"HTTP {r.status_code}: {r.text[:200]}")
        except Exception as e:  # red caída, 5xx, SQL Server lento: reintentar con backoff exponencial
            espera = min(2 ** (intentos + 1), MAX_BACKOFF_SECONDS)
            conn.execute(
                "UPDATE outbox SET intentos = intentos + 1, ultimo_error = %s, "
                "siguiente_intento = now() + make_interval(secs => %s) WHERE id = %s",
                (str(e)[:500], espera, outbox_id),
            )
            print(f"pedido {pedido_id} reintento en {espera}s: {e}", flush=True)
            return True


def main():
    with psycopg.connect(DATABASE_URL, autocommit=True) as conn, httpx.Client() as client:
        while True:
            if not procesar_uno(conn, client):
                time.sleep(POLL_SECONDS)


if __name__ == "__main__":
    main()
