use anyhow::Result;
use reqwest::StatusCode;
use rust_decimal::prelude::ToPrimitive;
use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};
use sqlx::PgPool;
use uuid::Uuid;

pub struct Config {
    pub internal_api_url: String,
    pub internal_api_key: String,
    pub max_backoff_secs: i64,
}

impl Default for Config {
    fn default() -> Self {
        Self {
            internal_api_url: String::new(),
            internal_api_key: String::new(),
            max_backoff_secs: 300,
        }
    }
}

#[derive(sqlx::FromRow)]
struct Header {
    id_persona: i32,
    id_email: i32,
    sucursal_id: i16,
    id_documento: Option<i16>,
    id_uso_cfdi: Option<i16>,
    id_tipo_venta: Option<i16>,
    id_transporte: Option<i32>,
    id_porte: Option<i16>,
    id_direccion_fiscal: Option<i32>,
    id_direccion_entrega: Option<i32>,
    observaciones: Option<String>,
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
struct ItemPayload {
    id_producto: i16,
    cantidad: i32,
    unitario: f64,
    descuento: f64,
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
struct OrderPayload {
    order_id: Uuid,
    id_persona: i32,
    id_email: i32,
    sucursal: i16,
    id_documento: Option<i16>,
    id_uso_cfdi: Option<i16>,
    id_tipo_venta: Option<i16>,
    id_transporte: Option<i32>,
    id_porte: Option<i16>,
    fiscal: Option<i32>,
    entrega: Option<i32>,
    observaciones: Option<String>,
    items: Vec<ItemPayload>,
}

#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
struct Accepted {
    id_pedido_mr: i32,
    #[serde(default)]
    surtido: Vec<Fulfilled>,
}

#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
struct Fulfilled {
    id_producto: i16,
    surtido: i32,
    pendiente: i32,
}

async fn build_payload(pool: &PgPool, pedido_id: Uuid) -> Result<OrderPayload> {
    let h: Header = sqlx::query_as(
        "SELECT p.id_persona, u.id_email, p.sucursal_id, p.id_documento, p.id_uso_cfdi, p.id_tipo_venta,
                p.id_transporte, p.id_porte, p.id_direccion_fiscal, p.id_direccion_entrega, p.observaciones
         FROM pedido p JOIN usuario u ON u.id = p.usuario_id WHERE p.id = $1",
    )
    .bind(pedido_id)
    .fetch_one(pool)
    .await?;
    let items: Vec<(i16, i32, Decimal, Decimal)> = sqlx::query_as(
        "SELECT id_producto, cantidad, unitario, descuento FROM pedido_item WHERE pedido_id = $1 ORDER BY id_producto",
    )
    .bind(pedido_id)
    .fetch_all(pool)
    .await?;
    Ok(OrderPayload {
        order_id: pedido_id,
        id_persona: h.id_persona,
        id_email: h.id_email,
        sucursal: h.sucursal_id,
        id_documento: h.id_documento,
        id_uso_cfdi: h.id_uso_cfdi,
        id_tipo_venta: h.id_tipo_venta,
        id_transporte: h.id_transporte,
        id_porte: h.id_porte,
        fiscal: h.id_direccion_fiscal,
        entrega: h.id_direccion_entrega,
        observaciones: h.observaciones,
        items: items
            .into_iter()
            .map(|(id_producto, cantidad, u, d)| ItemPayload {
                id_producto,
                cantidad,
                unitario: u.to_f64().unwrap_or_default(),
                descuento: d.to_f64().unwrap_or_default(),
            })
            .collect(),
    })
}

/// Toma un envío pendiente (con `FOR UPDATE SKIP LOCKED`, así varios workers no se pisan) y lo manda a la API interna.
/// Devuelve `false` si no había nada pendiente.
pub async fn process_one(pool: &PgPool, client: &reqwest::Client, cfg: &Config) -> Result<bool> {
    let mut tx = pool.begin().await?;
    let row: Option<(i64, Uuid, i32)> = sqlx::query_as(
        "SELECT id, pedido_id, intentos FROM outbox
         WHERE enviado_en IS NULL AND siguiente_intento <= now()
         ORDER BY id FOR UPDATE SKIP LOCKED LIMIT 1",
    )
    .fetch_optional(&mut *tx)
    .await?;
    let Some((outbox_id, pedido_id, intentos)) = row else {
        return Ok(false);
    };

    let payload = build_payload(pool, pedido_id).await?;
    let response = client
        .post(format!("{}/orders", cfg.internal_api_url))
        .header("X-Api-Key", &cfg.internal_api_key)
        .header("Idempotency-Key", pedido_id.to_string())
        .json(&payload)
        .send()
        .await;

    let failure = match response {
        Ok(r) if matches!(r.status(), StatusCode::OK | StatusCode::CREATED) => {
            match r.json::<Accepted>().await {
                Ok(data) => {
                    sqlx::query(
                        "UPDATE outbox SET enviado_en = now(), ultimo_error = NULL WHERE id = $1",
                    )
                    .bind(outbox_id)
                    .execute(&mut *tx)
                    .await?;
                    sqlx::query("UPDATE pedido SET estado = 'sincronizado', id_pedido_mr = $1 WHERE id = $2")
                        .bind(data.id_pedido_mr)
                        .bind(pedido_id)
                        .execute(&mut *tx)
                        .await?;
                    for s in data.surtido {
                        sqlx::query(
                            "UPDATE pedido_item SET surtido = $1, colocado = $2 WHERE pedido_id = $3 AND id_producto = $4",
                        )
                        .bind(s.surtido)
                        .bind(s.pendiente == 0)
                        .bind(pedido_id)
                        .bind(s.id_producto)
                        .execute(&mut *tx)
                        .await?;
                    }
                    tracing::info!(%pedido_id, id_pedido_mr = data.id_pedido_mr, "pedido sincronizado");
                    tx.commit().await?;
                    return Ok(true);
                }
                Err(e) => format!("respuesta inválida de la API interna: {e}"),
            }
        }
        // 409: sin crédito / producto deshabilitado. Reintentar no cambia nada, se marca error.
        Ok(r) if r.status() == StatusCode::CONFLICT => {
            let body = r.text().await.unwrap_or_default();
            sqlx::query("UPDATE outbox SET enviado_en = now(), ultimo_error = $1 WHERE id = $2")
                .bind(&body)
                .bind(outbox_id)
                .execute(&mut *tx)
                .await?;
            sqlx::query("UPDATE pedido SET estado = 'error' WHERE id = $1")
                .bind(pedido_id)
                .execute(&mut *tx)
                .await?;
            tracing::warn!(%pedido_id, %body, "pedido rechazado");
            tx.commit().await?;
            return Ok(true);
        }
        Ok(r) => {
            let status = r.status();
            let body = r.text().await.unwrap_or_default();
            format!(
                "HTTP {status}: {}",
                body.chars().take(200).collect::<String>()
            )
        }
        Err(e) => e.to_string(),
    };

    // Red caída, 5xx, SQL Server lento: reintentar con backoff exponencial.
    let wait = 2_i64
        .saturating_pow((intentos + 1).clamp(0, 30) as u32)
        .min(cfg.max_backoff_secs);
    sqlx::query(
        "UPDATE outbox SET intentos = intentos + 1, ultimo_error = $1,
         siguiente_intento = now() + make_interval(secs => $2) WHERE id = $3",
    )
    .bind(failure.chars().take(500).collect::<String>())
    .bind(wait as f64)
    .bind(outbox_id)
    .execute(&mut *tx)
    .await?;
    tx.commit().await?;
    tracing::warn!(%pedido_id, wait_secs = wait, error = %failure, "reintento programado");
    Ok(true)
}
