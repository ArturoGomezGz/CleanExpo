use axum::extract::{Path, State};
use axum::Json;
use serde::{Deserialize, Serialize};

use crate::auth::AuthUser;
use crate::{catalog, ApiError, AppState};

/// Tope por línea: evita desbordes y cantidades absurdas por error de captura.
const MAX_CANTIDAD: i64 = 100_000;

#[derive(Serialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct Linea {
    pub producto_id: i16,
    pub cantidad: i32,
    pub confirmado: bool,
}

#[derive(Deserialize)]
pub struct SetCantidad {
    pub cantidad: i64,
}

#[derive(Deserialize)]
pub struct SetConfirmado {
    pub confirmado: bool,
}

async fn lineas(state: &AppState, user: &AuthUser) -> Result<Vec<Linea>, ApiError> {
    Ok(sqlx::query_as(
        "SELECT i.id_producto AS producto_id, i.cantidad, i.confirmado
         FROM carrito c JOIN carrito_item i ON i.carrito_id = c.id
         WHERE c.id_persona = $1 AND c.usuario_id = $2 ORDER BY i.id_producto",
    )
    .bind(user.id_persona)
    .bind(user.usuario_id)
    .fetch_all(&state.pool)
    .await?)
}

pub async fn get(
    State(state): State<AppState>,
    user: AuthUser,
) -> Result<Json<Vec<Linea>>, ApiError> {
    Ok(Json(lineas(&state, &user).await?))
}

pub async fn set_linea(
    State(state): State<AppState>,
    user: AuthUser,
    Path(producto_id): Path<i16>,
    Json(body): Json<SetCantidad>,
) -> Result<Json<Vec<Linea>>, ApiError> {
    if body.cantidad <= 0 {
        sqlx::query(
            "DELETE FROM carrito_item WHERE id_producto = $3 AND carrito_id IN
             (SELECT id FROM carrito WHERE id_persona = $1 AND usuario_id = $2)",
        )
        .bind(user.id_persona)
        .bind(user.usuario_id)
        .bind(producto_id)
        .execute(&state.pool)
        .await?;
        return get(State(state), user).await;
    }
    if body.cantidad > MAX_CANTIDAD {
        return Err(ApiError::Unprocessable(format!(
            "cantidad máxima por producto: {MAX_CANTIDAD}"
        )));
    }
    let vendible: bool = sqlx::query_scalar(
        "SELECT EXISTS (SELECT 1 FROM producto WHERE id_producto = $1 AND habilitar AND status_indigo IN (0, 3, 4))",
    )
    .bind(producto_id)
    .fetch_one(&state.pool)
    .await?;
    if !vendible {
        return Err(ApiError::NotFound("producto no existe"));
    }
    // El precio guardado en la línea es informativo; el que cobra se vuelve a pedir a MR al colocar el pedido.
    let precio = catalog::precios(&state, user.id_persona, &[producto_id], false)
        .await?
        .remove(&producto_id)
        .ok_or_else(|| ApiError::Unprocessable("producto sin precio para este cliente".into()))?;
    let carrito_id: i64 = sqlx::query_scalar(
        "INSERT INTO carrito (id_persona, usuario_id) VALUES ($1, $2)
         ON CONFLICT (id_persona, usuario_id) DO UPDATE SET id = carrito.id RETURNING id",
    )
    .bind(user.id_persona)
    .bind(user.usuario_id)
    .fetch_one(&state.pool)
    .await?;
    sqlx::query(
        "INSERT INTO carrito_item (carrito_id, id_producto, unitario, cantidad) VALUES ($1, $2, $3, $4)
         ON CONFLICT (carrito_id, id_producto)
         DO UPDATE SET cantidad = EXCLUDED.cantidad, unitario = EXCLUDED.unitario, actualizado_en = now()",
    )
    .bind(carrito_id)
    .bind(producto_id)
    .bind(precio)
    .bind(body.cantidad as i32)
    .execute(&state.pool)
    .await?;
    get(State(state), user).await
}

pub async fn confirmar_linea(
    State(state): State<AppState>,
    user: AuthUser,
    Path(producto_id): Path<i16>,
    Json(body): Json<SetConfirmado>,
) -> Result<Json<Vec<Linea>>, ApiError> {
    sqlx::query(
        "UPDATE carrito_item SET confirmado = $4, actualizado_en = now()
         WHERE id_producto = $3 AND carrito_id IN (SELECT id FROM carrito WHERE id_persona = $1 AND usuario_id = $2)",
    )
    .bind(user.id_persona)
    .bind(user.usuario_id)
    .bind(producto_id)
    .bind(body.confirmado)
    .execute(&state.pool)
    .await?;
    get(State(state), user).await
}

pub async fn confirmar_todas(
    State(state): State<AppState>,
    user: AuthUser,
    Json(body): Json<SetConfirmado>,
) -> Result<Json<Vec<Linea>>, ApiError> {
    sqlx::query(
        "UPDATE carrito_item SET confirmado = $3, actualizado_en = now()
         WHERE carrito_id IN (SELECT id FROM carrito WHERE id_persona = $1 AND usuario_id = $2)",
    )
    .bind(user.id_persona)
    .bind(user.usuario_id)
    .bind(body.confirmado)
    .execute(&state.pool)
    .await?;
    get(State(state), user).await
}
