use argon2::password_hash::{
    rand_core::OsRng, PasswordHash, PasswordHasher, PasswordVerifier, SaltString,
};
use argon2::Argon2;
use axum::extract::{FromRequestParts, State};
use axum::http::{header, request::Parts, StatusCode};
use axum::Json;
use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};
use sha2::{Digest, Sha256};
use std::sync::OnceLock;
use uuid::Uuid;

use crate::{ApiError, AppState};

/// Hash de una contraseña aleatoria: se verifica aunque el correo no exista, para no revelar qué correos existen por el tiempo de respuesta.
fn hash_falso() -> &'static str {
    static HASH: OnceLock<String> = OnceLock::new();
    HASH.get_or_init(|| {
        hash_password(&nuevo_token()).expect("no se pudo generar el hash de relleno")
    })
}

pub fn hash_password(password: &str) -> Result<String, argon2::password_hash::Error> {
    let salt = SaltString::generate(&mut OsRng);
    Ok(Argon2::default()
        .hash_password(password.as_bytes(), &salt)?
        .to_string())
}

fn hash_token(token: &str) -> Vec<u8> {
    Sha256::digest(token.as_bytes()).to_vec()
}

/// Token opaco de 244 bits (dos UUID v4 del CSPRNG del sistema).
fn nuevo_token() -> String {
    format!("{}{}", Uuid::new_v4().simple(), Uuid::new_v4().simple())
}

/// Usuario autenticado por `Authorization: Bearer <token>`.
pub struct AuthUser {
    pub usuario_id: i64,
    pub id_persona: i32,
    pub id_email: i32,
}

impl FromRequestParts<AppState> for AuthUser {
    type Rejection = ApiError;

    async fn from_request_parts(parts: &mut Parts, state: &AppState) -> Result<Self, ApiError> {
        let token = parts
            .headers
            .get(header::AUTHORIZATION)
            .and_then(|v| v.to_str().ok())
            .and_then(|v| v.strip_prefix("Bearer "))
            .ok_or(ApiError::Unauthorized("falta iniciar sesión"))?;
        let row: Option<(i64, i32, i32)> = sqlx::query_as(
            "SELECT u.id, u.id_persona, u.id_email FROM sesion s JOIN usuario u ON u.id = s.usuario_id
             WHERE s.token_hash = $1 AND s.expira_en > now()",
        )
        .bind(hash_token(token))
        .fetch_optional(&state.pool)
        .await?;
        let (usuario_id, id_persona, id_email) =
            row.ok_or(ApiError::Unauthorized("la sesión expiró"))?;
        Ok(AuthUser {
            usuario_id,
            id_persona,
            id_email,
        })
    }
}

#[derive(Deserialize)]
pub struct Login {
    pub email: String,
    pub password: String,
}

#[derive(Serialize)]
pub struct Direccion {
    pub id: i32,
    pub etiqueta: String,
    pub texto: String,
}

#[derive(Serialize)]
pub struct Transporte {
    pub id: i32,
    pub nombre: String,
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct Cliente {
    pub id_persona: i32,
    pub id_email: i32,
    pub nombre: String,
    pub email: String,
    pub tipo: &'static str,
    pub sucursal: String,
    pub credito: Decimal,
    pub direcciones: Vec<Direccion>,
    pub transportes: Vec<Transporte>,
}

#[derive(Serialize)]
pub struct LoginOk {
    pub token: String,
    pub cliente: Cliente,
}

pub async fn cliente(state: &AppState, user: &AuthUser) -> Result<Cliente, ApiError> {
    let (nombre, email, tipo, sucursal): (String, String, i16, String) = sqlx::query_as(
        "SELECT c.nombre, u.email, c.tipo_cliente, s.nombre
         FROM usuario u JOIN cliente c ON c.id_persona = u.id_persona JOIN sucursal s ON s.id = c.sucursal_id
         WHERE u.id = $1",
    )
    .bind(user.usuario_id)
    .fetch_optional(&state.pool)
    .await?
    .ok_or(ApiError::NotFound("cliente no existe"))?;
    let direcciones = sqlx::query_as::<_, (i32, String, String)>(
        "SELECT id, etiqueta, texto FROM direccion WHERE id_persona = $1 ORDER BY id",
    )
    .bind(user.id_persona)
    .fetch_all(&state.pool)
    .await?
    .into_iter()
    .map(|(id, etiqueta, texto)| Direccion {
        id,
        etiqueta,
        texto,
    })
    .collect();
    let transportes =
        sqlx::query_as::<_, (i32, String)>("SELECT id, nombre FROM transporte ORDER BY id")
            .fetch_all(&state.pool)
            .await?
            .into_iter()
            .map(|(id, nombre)| Transporte { id, nombre })
            .collect();
    // El crédito es informativo aquí: si MR no responde, el login no se cae (el pedido sí lo vuelve a validar).
    let credito = crate::orders::credito_disponible(state, user.id_persona)
        .await
        .unwrap_or_default();
    Ok(Cliente {
        id_persona: user.id_persona,
        id_email: user.id_email,
        nombre,
        email,
        tipo: if tipo == 2 {
            "distribuidor"
        } else {
            "mayorista"
        },
        sucursal,
        credito,
        direcciones,
        transportes,
    })
}

pub async fn login(
    State(state): State<AppState>,
    Json(body): Json<Login>,
) -> Result<Json<LoginOk>, ApiError> {
    let email = body.email.trim().to_lowercase();
    let row: Option<(i64, i32, i32, Option<String>)> = sqlx::query_as(
        "SELECT id, id_persona, id_email, password_hash FROM usuario WHERE lower(email) = $1",
    )
    .bind(&email)
    .fetch_optional(&state.pool)
    .await?;
    let hash = row
        .as_ref()
        .and_then(|r| r.3.as_deref())
        .unwrap_or_else(|| hash_falso())
        .to_string();
    let password = body.password;
    // argon2 es CPU-intensivo: fuera del runtime async.
    let ok = tokio::task::spawn_blocking(move || {
        PasswordHash::new(&hash)
            .map(|h| {
                Argon2::default()
                    .verify_password(password.as_bytes(), &h)
                    .is_ok()
            })
            .unwrap_or(false)
    })
    .await
    .unwrap_or(false);
    let (usuario_id, id_persona, id_email) = match row {
        Some((u, p, e, Some(_))) if ok => (u, p, e),
        _ => return Err(ApiError::Unauthorized("Correo o contraseña incorrectos")),
    };

    let token = nuevo_token();
    sqlx::query(
        "INSERT INTO sesion (token_hash, usuario_id, expira_en) VALUES ($1, $2, now() + make_interval(hours => $3))",
    )
    .bind(hash_token(&token))
    .bind(usuario_id)
    .bind(state.cfg.session_hours)
    .execute(&state.pool)
    .await?;
    // Limpieza oportunista de sesiones vencidas.
    sqlx::query("DELETE FROM sesion WHERE expira_en < now()")
        .execute(&state.pool)
        .await?;

    let user = AuthUser {
        usuario_id,
        id_persona,
        id_email,
    };
    let cliente = cliente(&state, &user).await?;
    Ok(Json(LoginOk { token, cliente }))
}

pub async fn logout(
    State(state): State<AppState>,
    headers: axum::http::HeaderMap,
) -> Result<StatusCode, ApiError> {
    if let Some(token) = headers
        .get(header::AUTHORIZATION)
        .and_then(|v| v.to_str().ok())
        .and_then(|v| v.strip_prefix("Bearer "))
    {
        sqlx::query("DELETE FROM sesion WHERE token_hash = $1")
            .bind(hash_token(token))
            .execute(&state.pool)
            .await?;
    }
    Ok(StatusCode::NO_CONTENT)
}

pub async fn me(State(state): State<AppState>, user: AuthUser) -> Result<Json<Cliente>, ApiError> {
    Ok(Json(cliente(&state, &user).await?))
}
