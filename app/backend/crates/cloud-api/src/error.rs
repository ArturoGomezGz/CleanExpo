use axum::{http::StatusCode, response::IntoResponse, response::Response, Json};
use serde_json::json;

#[derive(Debug)]
pub enum ApiError {
    NotFound(&'static str),
    Unprocessable(String),
    Unauthorized(&'static str),
    /// Error de negocio con un código estable que el frontend traduce (`sin-credito`, `carrito-vacio`, ...).
    Coded(StatusCode, &'static str, String),
    /// La API interna de MR no respondió; el cliente puede reintentar.
    Upstream(String),
    Internal(sqlx::Error),
}

impl From<sqlx::Error> for ApiError {
    fn from(e: sqlx::Error) -> Self {
        ApiError::Internal(e)
    }
}

impl IntoResponse for ApiError {
    fn into_response(self) -> Response {
        let (status, code, msg) = match self {
            ApiError::NotFound(m) => (StatusCode::NOT_FOUND, "no-encontrado", m.to_string()),
            ApiError::Unprocessable(m) => (StatusCode::UNPROCESSABLE_ENTITY, "invalido", m),
            ApiError::Unauthorized(m) => (StatusCode::UNAUTHORIZED, "credenciales", m.to_string()),
            ApiError::Coded(s, c, m) => (s, c, m),
            ApiError::Upstream(m) => {
                tracing::warn!(error = %m, "falló la API interna de MR");
                (
                    StatusCode::SERVICE_UNAVAILABLE,
                    "mr-no-disponible",
                    "no se pudo consultar a Medicinas Rosario, intenta de nuevo".to_string(),
                )
            }
            ApiError::Internal(e) => {
                tracing::error!(error = %e, "error de base de datos");
                (
                    StatusCode::INTERNAL_SERVER_ERROR,
                    "interno",
                    "error interno".to_string(),
                )
            }
        };
        (status, Json(json!({ "detail": msg, "code": code }))).into_response()
    }
}
