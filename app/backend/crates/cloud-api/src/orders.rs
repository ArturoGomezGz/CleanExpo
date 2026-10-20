use axum::{extract::Path, extract::State, http::StatusCode, Json};
use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};
use uuid::Uuid;

use crate::{ApiError, AppState};

#[derive(Deserialize)]
pub struct NewOrder {
    pub id_persona: i32,
    pub usuario_id: i64,
    pub items: Vec<NewItem>,
}

#[derive(Deserialize)]
pub struct NewItem {
    pub id_producto: i16,
    pub cantidad: i32,
}

#[derive(Serialize)]
pub struct Created {
    pub id: Uuid,
    pub estado: &'static str,
}

/// Guarda el pedido como `recibido` y encola el envío. El precio sale SIEMPRE de `precio_cliente` (servidor).
pub async fn create(
    State(state): State<AppState>,
    Json(body): Json<NewOrder>,
) -> Result<(StatusCode, Json<Created>), ApiError> {
    if body.items.is_empty() {
        return Err(ApiError::Unprocessable(
            "el pedido no tiene productos".into(),
        ));
    }
    if body.items.iter().any(|i| i.cantidad <= 0) {
        return Err(ApiError::Unprocessable(
            "cantidad debe ser mayor a 0".into(),
        ));
    }
    let mut ids: Vec<i16> = body.items.iter().map(|i| i.id_producto).collect();
    ids.sort_unstable();
    if ids.windows(2).any(|w| w[0] == w[1]) {
        return Err(ApiError::Unprocessable(
            "producto repetido en el pedido".into(),
        ));
    }

    let mut tx = state.pool.begin().await?;

    let sucursal: Option<i16> =
        sqlx::query_scalar("SELECT sucursal_id FROM cliente WHERE id_persona = $1")
            .bind(body.id_persona)
            .fetch_optional(&mut *tx)
            .await?;
    let sucursal = sucursal.ok_or(ApiError::NotFound("cliente no existe"))?;

    let precios: Vec<(i16, Decimal)> = sqlx::query_as(
        "SELECT id_producto, precio FROM precio_cliente WHERE id_persona = $1 AND id_producto = ANY($2)",
    )
    .bind(body.id_persona)
    .bind(&ids)
    .fetch_all(&mut *tx)
    .await?;
    let faltan: Vec<i16> = ids
        .iter()
        .copied()
        .filter(|id| !precios.iter().any(|(p, _)| p == id))
        .collect();
    if !faltan.is_empty() {
        return Err(ApiError::Unprocessable(format!(
            "sin precio para productos {faltan:?}"
        )));
    }

    let pedido_id: Uuid = sqlx::query_scalar(
        "INSERT INTO pedido (id_persona, usuario_id, sucursal_id) VALUES ($1, $2, $3) RETURNING id",
    )
    .bind(body.id_persona)
    .bind(body.usuario_id)
    .bind(sucursal)
    .fetch_one(&mut *tx)
    .await?;

    for item in &body.items {
        let precio = precios
            .iter()
            .find(|(p, _)| *p == item.id_producto)
            .map(|(_, v)| *v)
            .unwrap_or_default();
        sqlx::query(
            "INSERT INTO pedido_item (pedido_id, id_producto, unitario, descuento, cantidad) VALUES ($1, $2, $3, 0, $4)",
        )
        .bind(pedido_id)
        .bind(item.id_producto)
        .bind(precio)
        .bind(item.cantidad)
        .execute(&mut *tx)
        .await?;
    }
    sqlx::query("INSERT INTO outbox (pedido_id) VALUES ($1)")
        .bind(pedido_id)
        .execute(&mut *tx)
        .await?;
    tx.commit().await?;

    Ok((
        StatusCode::ACCEPTED,
        Json(Created {
            id: pedido_id,
            estado: "recibido",
        }),
    ))
}

#[derive(Serialize)]
pub struct Status {
    pub id: Uuid,
    pub estado: String,
    pub id_pedido_mr: Option<i32>,
    pub items: Vec<ItemStatus>,
}

#[derive(Serialize, sqlx::FromRow)]
pub struct ItemStatus {
    pub id_producto: i16,
    pub cantidad: i32,
    pub surtido: i32,
}

pub async fn status(
    State(state): State<AppState>,
    Path(id): Path<Uuid>,
) -> Result<Json<Status>, ApiError> {
    let row: Option<(String, Option<i32>)> =
        sqlx::query_as("SELECT estado::text, id_pedido_mr FROM pedido WHERE id = $1")
            .bind(id)
            .fetch_optional(&state.pool)
            .await?;
    let (estado, id_pedido_mr) = row.ok_or(ApiError::NotFound("pedido no existe"))?;
    let items = sqlx::query_as::<_, ItemStatus>(
        "SELECT id_producto, cantidad, surtido FROM pedido_item WHERE pedido_id = $1 ORDER BY id_producto",
    )
    .bind(id)
    .fetch_all(&state.pool)
    .await?;
    Ok(Json(Status {
        id,
        estado,
        id_pedido_mr,
        items,
    }))
}
