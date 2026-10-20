use axum::{http::StatusCode, response::IntoResponse, response::Response, Json};
use serde_json::json;

#[derive(Debug)]
pub enum ApiError {
    NotFound(&'static str),
    Unprocessable(String),
    Internal(sqlx::Error),
}

impl From<sqlx::Error> for ApiError {
    fn from(e: sqlx::Error) -> Self {
        ApiError::Internal(e)
    }
}

impl IntoResponse for ApiError {
    fn into_response(self) -> Response {
        let (status, msg) = match self {
            ApiError::NotFound(m) => (StatusCode::NOT_FOUND, m.to_string()),
            ApiError::Unprocessable(m) => (StatusCode::UNPROCESSABLE_ENTITY, m),
            ApiError::Internal(e) => {
                tracing::error!(error = %e, "error de base de datos");
                (
                    StatusCode::INTERNAL_SERVER_ERROR,
                    "error interno".to_string(),
                )
            }
        };
        (status, Json(json!({ "detail": msg }))).into_response()
    }
}
