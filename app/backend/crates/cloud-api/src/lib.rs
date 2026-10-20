mod error;
mod orders;

use axum::{routing::get, routing::post, Json, Router};
use serde_json::{json, Value};
use sqlx::PgPool;

pub use error::ApiError;

#[derive(Clone)]
pub struct AppState {
    pub pool: PgPool,
}

pub fn router(pool: PgPool) -> Router {
    Router::new()
        .route("/health", get(health))
        .route("/orders", post(orders::create))
        .route("/orders/{id}", get(orders::status))
        .with_state(AppState { pool })
}

async fn health(
    axum::extract::State(state): axum::extract::State<AppState>,
) -> Result<Json<Value>, ApiError> {
    sqlx::query("SELECT 1").execute(&state.pool).await?;
    Ok(Json(json!({ "status": "ok" })))
}
