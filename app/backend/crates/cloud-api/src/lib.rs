mod auth;
mod cart;
mod catalog;
mod error;
mod money;
pub mod mr;
mod orders;

use axum::http::{header, HeaderValue, Method};
use axum::routing::{get, post, put};
use axum::{Json, Router};
use serde_json::{json, Value};
use sqlx::PgPool;
use std::sync::Arc;
use tower_http::cors::{AllowOrigin, CorsLayer};

pub use auth::hash_password;
pub use error::ApiError;
pub use mr::Mr;

#[derive(Clone)]
pub struct Config {
    /// Cuánto vale un precio guardado de `precio_cliente` antes de volver a pedirlo a MR.
    pub price_ttl_secs: f64,
    pub session_hours: i32,
    /// Orígenes del frontend permitidos (CORS). Vacío = solo mismo origen (nginx/proxy).
    pub cors_origins: Vec<String>,
}

impl Default for Config {
    fn default() -> Self {
        Self {
            price_ttl_secs: 900.0,
            session_hours: 24,
            cors_origins: vec![],
        }
    }
}

#[derive(Clone)]
pub struct AppState {
    pub pool: PgPool,
    pub mr: Arc<dyn Mr>,
    pub cfg: Config,
}

pub fn router(pool: PgPool, mr: Arc<dyn Mr>, cfg: Config) -> Router {
    let cors = (!cfg.cors_origins.is_empty()).then(|| {
        CorsLayer::new()
            .allow_origin(AllowOrigin::list(
                cfg.cors_origins
                    .iter()
                    .filter_map(|o| HeaderValue::from_str(o).ok()),
            ))
            .allow_methods([Method::GET, Method::POST, Method::PUT, Method::DELETE])
            .allow_headers([header::AUTHORIZATION, header::CONTENT_TYPE])
    });
    let app = Router::new()
        .route("/health", get(health))
        .route("/auth/login", post(auth::login))
        .route("/auth/logout", post(auth::logout))
        .route("/me", get(auth::me))
        .route("/me/credit", get(orders::credito))
        .route("/stands", get(catalog::list_stands))
        .route("/stands/{id}", get(catalog::get_stand))
        .route("/products", get(catalog::list_productos))
        .route("/products/{id}", get(catalog::get_producto))
        .route("/promotions", get(catalog::list_promociones))
        .route("/cart", get(cart::get))
        .route("/cart/confirm", put(cart::confirmar_todas))
        .route("/cart/items/{id}", put(cart::set_linea))
        .route("/cart/items/{id}/confirm", put(cart::confirmar_linea))
        .route("/orders", get(orders::list).post(orders::create))
        .route("/orders/{id}", get(orders::get))
        .with_state(AppState { pool, mr, cfg });
    match cors {
        Some(c) => app.layer(c),
        None => app,
    }
}

async fn health(
    axum::extract::State(state): axum::extract::State<AppState>,
) -> Result<Json<Value>, ApiError> {
    sqlx::query("SELECT 1").execute(&state.pool).await?;
    Ok(Json(json!({ "status": "ok" })))
}
