use axum::extract::{Path, State};
use axum::http::StatusCode;
use axum::Json;
use chrono::{DateTime, Utc};
use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};
use serde_json::json;
use std::collections::HashMap;
use uuid::Uuid;

use crate::auth::AuthUser;
use crate::money::totales;
use crate::{catalog, ApiError, AppState};

/// Crédito del cliente: lo que MR reconoce menos los pedidos nuestros que MR todavía no recibió.
pub async fn credito_disponible(state: &AppState, id_persona: i32) -> Result<Decimal, ApiError> {
    let mr = state
        .mr
        .credit(id_persona)
        .await
        .map_err(|e| ApiError::Upstream(e.to_string()))?;
    let pendientes: Vec<(Uuid, i32, Decimal, bool)> = sqlx::query_as(
        "SELECT p.id, i.cantidad, i.unitario, pr.iva
         FROM pedido p JOIN pedido_item i ON i.pedido_id = p.id JOIN producto pr ON pr.id_producto = i.id_producto
         WHERE p.id_persona = $1 AND p.estado IN ('recibido', 'enviando')",
    )
    .bind(id_persona)
    .fetch_all(&state.pool)
    .await?;
    let mut por_pedido: HashMap<Uuid, Vec<(i32, Decimal, bool)>> = HashMap::new();
    for (id, c, u, iva) in pendientes {
        por_pedido.entry(id).or_default().push((c, u, iva));
    }
    let pendiente: Decimal = por_pedido.into_values().map(|l| totales(l).total).sum();
    Ok(mr - pendiente)
}

pub async fn credito(
    State(state): State<AppState>,
    user: AuthUser,
) -> Result<Json<serde_json::Value>, ApiError> {
    let disponible = credito_disponible(&state, user.id_persona).await?;
    Ok(Json(json!({ "disponible": disponible })))
}

// ---------- Colocar pedido ----------

#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct NewOrder {
    pub datos: Datos,
}

#[derive(Deserialize, Serialize, Clone)]
#[serde(rename_all = "camelCase")]
pub struct Datos {
    pub id_documento: i16, // 1 factura, 2 nota
    pub uso_cfdi: String,
    pub direccion_id: i32,
    pub transporte_id: i32,
    #[serde(default)]
    pub observaciones: String,
}

#[derive(sqlx::FromRow)]
struct LineaCarrito {
    id_producto: i16,
    cantidad: i32,
    nombre: String,
    iva: bool,
    stand_id: Option<i32>,
}

/// Convierte el carrito confirmado en pedido. Precio y crédito se validan aquí contra MR: nada viene del navegador.
pub async fn create(
    State(state): State<AppState>,
    user: AuthUser,
    Json(body): Json<NewOrder>,
) -> Result<(StatusCode, Json<Pedido>), ApiError> {
    let datos = body.datos;
    if !matches!(datos.id_documento, 1 | 2) {
        return Err(ApiError::Unprocessable("idDocumento inválido".into()));
    }
    let uso_cfdi: Option<i16> = sqlx::query_scalar("SELECT id FROM uso_cfdi WHERE clave = $1")
        .bind(datos.uso_cfdi.trim())
        .fetch_optional(&state.pool)
        .await?;
    if datos.id_documento == 1 && uso_cfdi.is_none() {
        return Err(ApiError::Unprocessable("uso de CFDI inválido".into()));
    }
    let entrega_ok: bool = sqlx::query_scalar(
        "SELECT EXISTS (SELECT 1 FROM direccion WHERE id = $1 AND id_persona = $2)",
    )
    .bind(datos.direccion_id)
    .bind(user.id_persona)
    .fetch_one(&state.pool)
    .await?;
    if !entrega_ok {
        return Err(ApiError::Unprocessable(
            "dirección de entrega inválida".into(),
        ));
    }
    let transporte_ok: bool =
        sqlx::query_scalar("SELECT EXISTS (SELECT 1 FROM transporte WHERE id = $1)")
            .bind(datos.transporte_id)
            .fetch_one(&state.pool)
            .await?;
    if !transporte_ok {
        return Err(ApiError::Unprocessable("transporte inválido".into()));
    }

    let lineas: Vec<LineaCarrito> = sqlx::query_as(
        "SELECT i.id_producto, i.cantidad, p.nombre, p.iva, l.proveedor_id AS stand_id
         FROM carrito c JOIN carrito_item i ON i.carrito_id = c.id
         JOIN producto p ON p.id_producto = i.id_producto JOIN laboratorio l ON l.id_laboratorio = p.id_laboratorio
         WHERE c.id_persona = $1 AND c.usuario_id = $2 AND i.confirmado ORDER BY i.id_producto",
    )
    .bind(user.id_persona)
    .bind(user.usuario_id)
    .fetch_all(&state.pool)
    .await?;
    if lineas.is_empty() {
        return Err(ApiError::Coded(
            StatusCode::UNPROCESSABLE_ENTITY,
            "carrito-vacio",
            "No hay productos confirmados en el carrito".into(),
        ));
    }

    // Un pedido a la vez por cliente: así dos pedidos simultáneos no gastan el mismo crédito.
    let mut tx = state.pool.begin().await?;
    sqlx::query("SELECT pg_advisory_xact_lock($1)")
        .bind(i64::from(user.id_persona))
        .execute(&mut *tx)
        .await?;

    let ids: Vec<i16> = lineas.iter().map(|l| l.id_producto).collect();
    let precios = catalog::precios(&state, user.id_persona, &ids, true).await?;
    if let Some(l) = lineas
        .iter()
        .find(|l| !precios.contains_key(&l.id_producto))
    {
        return Err(ApiError::Unprocessable(format!(
            "{} ya no está disponible para ti",
            l.nombre
        )));
    }
    let total = totales(
        lineas
            .iter()
            .map(|l| (l.cantidad, precios[&l.id_producto], l.iva)),
    )
    .total;
    let disponible = credito_disponible(&state, user.id_persona).await?;
    if total > disponible {
        return Err(ApiError::Coded(
            StatusCode::CONFLICT,
            "sin-credito",
            "Crédito insuficiente para este pedido".into(),
        ));
    }

    // Promociones ganadas: misma regla que cart-math.ts (no acumulable = una vez; acumulable = una por múltiplo del umbral).
    let mut por_stand: HashMap<i32, (i64, Decimal)> = HashMap::new();
    for l in &lineas {
        if let Some(s) = l.stand_id {
            let acc = por_stand.entry(s).or_default();
            acc.0 += i64::from(l.cantidad);
            acc.1 += Decimal::from(l.cantidad) * precios[&l.id_producto];
        }
    }
    let mut premios = Vec::new();
    for promo in catalog::promociones(&state, user.id_persona, None).await? {
        let Some((piezas, monto)) = por_stand.get(&promo.stand_id) else {
            continue;
        };
        let base = if promo.tipo == "piezas" {
            Decimal::from(*piezas)
        } else {
            *monto
        };
        if promo.umbral <= Decimal::ZERO {
            continue;
        }
        let veces = if promo.acumulable {
            (base / promo.umbral).floor()
        } else if base >= promo.umbral {
            Decimal::ONE
        } else {
            Decimal::ZERO
        };
        if veces > Decimal::ZERO {
            premios.push(if veces > Decimal::ONE {
                format!("{veces} × {}", promo.premio)
            } else {
                promo.premio
            });
        }
    }

    let (sucursal, fiscal, tipo_venta, porte): (i16, Option<i32>, Option<i16>, Option<i16>) =
        sqlx::query_as(
            "SELECT sucursal_id, id_direccion_fiscal, id_tipo_venta, id_porte FROM cliente WHERE id_persona = $1",
        )
        .bind(user.id_persona)
        .fetch_optional(&mut *tx)
        .await?
        .ok_or(ApiError::NotFound("cliente no existe"))?;

    let pedido_id: Uuid = sqlx::query_scalar(
        "INSERT INTO pedido (id_persona, usuario_id, sucursal_id, id_documento, id_uso_cfdi, id_tipo_venta,
                             id_transporte, id_porte, id_direccion_fiscal, id_direccion_entrega, observaciones, promociones)
         VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12) RETURNING id",
    )
    .bind(user.id_persona)
    .bind(user.usuario_id)
    .bind(sucursal)
    .bind(datos.id_documento)
    .bind(uso_cfdi)
    .bind(tipo_venta)
    .bind(datos.transporte_id)
    .bind(porte)
    .bind(fiscal)
    .bind(datos.direccion_id)
    .bind(datos.observaciones.trim())
    .bind(json!(premios))
    .fetch_one(&mut *tx)
    .await?;
    for l in &lineas {
        sqlx::query(
            "INSERT INTO pedido_item (pedido_id, id_producto, unitario, descuento, cantidad) VALUES ($1, $2, $3, 0, $4)",
        )
        .bind(pedido_id)
        .bind(l.id_producto)
        .bind(precios[&l.id_producto])
        .bind(l.cantidad)
        .execute(&mut *tx)
        .await?;
    }
    sqlx::query("INSERT INTO outbox (pedido_id) VALUES ($1)")
        .bind(pedido_id)
        .execute(&mut *tx)
        .await?;
    sqlx::query(
        "DELETE FROM carrito_item WHERE confirmado AND carrito_id IN
         (SELECT id FROM carrito WHERE id_persona = $1 AND usuario_id = $2)",
    )
    .bind(user.id_persona)
    .bind(user.usuario_id)
    .execute(&mut *tx)
    .await?;
    tx.commit().await?;

    let pedido = pedidos(&state, user.id_persona, Some(pedido_id))
        .await?
        .pop()
        .ok_or(ApiError::NotFound("pedido no existe"))?;
    Ok((StatusCode::CREATED, Json(pedido)))
}

// ---------- Consulta ----------

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct LineaPedido {
    pub producto_id: i16,
    pub nombre: String,
    pub cantidad: i32,
    pub unitario: Decimal,
    pub iva: bool,
    pub surtido: i32,
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct Pedido {
    pub id: Uuid,
    pub folio: String,
    pub creado_en: DateTime<Utc>,
    pub estado: &'static str,
    pub datos: Datos,
    pub lineas: Vec<LineaPedido>,
    pub promociones: Vec<String>,
    pub total: Decimal,
}

#[derive(sqlx::FromRow)]
struct Cabecera {
    id: Uuid,
    folio: i64,
    creado_en: DateTime<Utc>,
    estado: String,
    id_documento: Option<i16>,
    uso_cfdi: Option<String>,
    id_direccion_entrega: Option<i32>,
    id_transporte: Option<i32>,
    observaciones: Option<String>,
    promociones: serde_json::Value,
}

async fn pedidos(
    state: &AppState,
    id_persona: i32,
    solo: Option<Uuid>,
) -> Result<Vec<Pedido>, ApiError> {
    let cabeceras: Vec<Cabecera> = sqlx::query_as(
        "SELECT p.id, p.folio, p.creado_en, p.estado::text AS estado, p.id_documento, u.clave AS uso_cfdi,
                p.id_direccion_entrega, p.id_transporte, p.observaciones, p.promociones
         FROM pedido p LEFT JOIN uso_cfdi u ON u.id = p.id_uso_cfdi
         WHERE p.id_persona = $1 AND ($2::uuid IS NULL OR p.id = $2)
         ORDER BY p.creado_en DESC, p.folio DESC LIMIT 200",
    )
    .bind(id_persona)
    .bind(solo)
    .fetch_all(&state.pool)
    .await?;
    let ids: Vec<Uuid> = cabeceras.iter().map(|c| c.id).collect();
    let items: Vec<(Uuid, i16, String, i32, Decimal, bool, i32)> = sqlx::query_as(
        "SELECT i.pedido_id, i.id_producto, p.nombre, i.cantidad, i.unitario, p.iva, i.surtido
         FROM pedido_item i JOIN producto p ON p.id_producto = i.id_producto
         WHERE i.pedido_id = ANY($1) ORDER BY i.id_producto",
    )
    .bind(&ids)
    .fetch_all(&state.pool)
    .await?;
    Ok(cabeceras
        .into_iter()
        .map(|c| {
            let lineas: Vec<LineaPedido> = items
                .iter()
                .filter(|i| i.0 == c.id)
                .map(|i| LineaPedido {
                    producto_id: i.1,
                    nombre: i.2.clone(),
                    cantidad: i.3,
                    unitario: i.4,
                    iva: i.5,
                    surtido: i.6,
                })
                .collect();
            let total = totales(lineas.iter().map(|l| (l.cantidad, l.unitario, l.iva))).total;
            Pedido {
                id: c.id,
                folio: format!("EXPO-{}", c.folio),
                creado_en: c.creado_en,
                // `enviando` es interno del worker; para el cliente sigue siendo "recibido".
                estado: match c.estado.as_str() {
                    "sincronizado" => "sincronizado",
                    "parcial" => "parcial",
                    "surtido" => "surtido",
                    "error" => "error",
                    _ => "recibido",
                },
                datos: Datos {
                    id_documento: c.id_documento.unwrap_or(1),
                    uso_cfdi: c.uso_cfdi.unwrap_or_default(),
                    direccion_id: c.id_direccion_entrega.unwrap_or_default(),
                    transporte_id: c.id_transporte.unwrap_or_default(),
                    observaciones: c.observaciones.unwrap_or_default(),
                },
                lineas,
                promociones: serde_json::from_value(c.promociones).unwrap_or_default(),
                total,
            }
        })
        .collect())
}

pub async fn list(
    State(state): State<AppState>,
    user: AuthUser,
) -> Result<Json<Vec<Pedido>>, ApiError> {
    Ok(Json(pedidos(&state, user.id_persona, None).await?))
}

pub async fn get(
    State(state): State<AppState>,
    user: AuthUser,
    Path(id): Path<Uuid>,
) -> Result<Json<Pedido>, ApiError> {
    pedidos(&state, user.id_persona, Some(id))
        .await?
        .pop()
        .map(Json)
        .ok_or(ApiError::NotFound("pedido no existe"))
}
