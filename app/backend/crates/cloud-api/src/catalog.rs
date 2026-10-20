use axum::extract::{Path, Query, State};
use axum::Json;
use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};
use std::collections::HashMap;

use crate::auth::AuthUser;
use crate::{ApiError, AppState};

// ---------- Stands ----------

#[derive(Serialize)]
pub struct Anuncio {
    pub titulo: String,
    pub texto: String,
}

#[derive(Serialize)]
pub struct Stand {
    pub id: i32,
    pub nombre: String,
    pub descripcion: String,
    pub color: String,
    pub anuncios: Vec<Anuncio>,
}

async fn stands(state: &AppState, id: Option<i32>) -> Result<Vec<Stand>, ApiError> {
    let rows: Vec<(i32, String, String, String)> = sqlx::query_as(
        "SELECT id_persona, nombre, coalesce(descripcion, ''), coalesce(color_hex, '#64748b')
         FROM proveedor WHERE ($1::int IS NULL OR id_persona = $1) ORDER BY nombre",
    )
    .bind(id)
    .fetch_all(&state.pool)
    .await?;
    let anuncios: Vec<(i32, String, String)> = sqlx::query_as(
        "SELECT proveedor_id, coalesce(titulo, ''), coalesce(texto, '') FROM anuncio
         WHERE ($1::int IS NULL OR proveedor_id = $1) ORDER BY orden, id",
    )
    .bind(id)
    .fetch_all(&state.pool)
    .await?;
    Ok(rows
        .into_iter()
        .map(|(id, nombre, descripcion, color)| Stand {
            anuncios: anuncios
                .iter()
                .filter(|a| a.0 == id)
                .map(|a| Anuncio {
                    titulo: a.1.clone(),
                    texto: a.2.clone(),
                })
                .collect(),
            id,
            nombre,
            descripcion,
            color,
        })
        .collect())
}

pub async fn list_stands(
    State(state): State<AppState>,
    _user: AuthUser,
) -> Result<Json<Vec<Stand>>, ApiError> {
    Ok(Json(stands(&state, None).await?))
}

pub async fn get_stand(
    State(state): State<AppState>,
    _user: AuthUser,
    Path(id): Path<i32>,
) -> Result<Json<Stand>, ApiError> {
    stands(&state, Some(id))
        .await?
        .pop()
        .map(Json)
        .ok_or(ApiError::NotFound("stand no existe"))
}

// ---------- Precios por cliente ----------

/// Precios vigentes del cliente. Lo viejo o faltante (según `PRICE_TTL_SECS`) se consulta a MR y se guarda en
/// `precio_cliente`. Con `forzar` (al colocar un pedido) todo se revalida y un fallo de MR es error; sin `forzar`,
/// si MR no responde se usa lo último guardado.
pub async fn precios(
    state: &AppState,
    id_persona: i32,
    ids: &[i16],
    forzar: bool,
) -> Result<HashMap<i16, Decimal>, ApiError> {
    if ids.is_empty() {
        return Ok(HashMap::new());
    }
    let vigentes: Vec<(i16,)> = if forzar {
        vec![]
    } else {
        sqlx::query_as(
            "SELECT id_producto FROM precio_cliente
             WHERE id_persona = $1 AND id_producto = ANY($2)
               AND actualizado_en > now() - make_interval(secs => $3)",
        )
        .bind(id_persona)
        .bind(ids)
        .bind(state.cfg.price_ttl_secs)
        .fetch_all(&state.pool)
        .await?
    };
    let vigentes: Vec<i16> = vigentes.into_iter().map(|v| v.0).collect();
    let viejos: Vec<i16> = ids
        .iter()
        .copied()
        .filter(|i| !vigentes.contains(i))
        .collect();

    if !viejos.is_empty() {
        match state.mr.prices(id_persona, &viejos).await {
            Ok(nuevos) => {
                let (pids, vals): (Vec<i16>, Vec<Decimal>) = nuevos.iter().cloned().unzip();
                sqlx::query(
                    "INSERT INTO precio_cliente (id_persona, id_producto, precio, actualizado_en)
                     SELECT $1, t.id, t.precio, now() FROM UNNEST($2::smallint[], $3::numeric[]) AS t(id, precio)
                     WHERE EXISTS (SELECT 1 FROM producto WHERE id_producto = t.id)
                     ON CONFLICT (id_persona, id_producto) DO UPDATE SET precio = EXCLUDED.precio, actualizado_en = now()",
                )
                .bind(id_persona)
                .bind(&pids)
                .bind(&vals)
                .execute(&state.pool)
                .await?;
                // Lo que MR ya no vende a este cliente deja de ofrecerse.
                let sin_precio: Vec<i16> =
                    viejos.into_iter().filter(|i| !pids.contains(i)).collect();
                sqlx::query(
                    "DELETE FROM precio_cliente WHERE id_persona = $1 AND id_producto = ANY($2)",
                )
                .bind(id_persona)
                .bind(&sin_precio)
                .execute(&state.pool)
                .await?;
            }
            Err(e) if forzar => return Err(ApiError::Upstream(e.to_string())),
            Err(e) => tracing::warn!(error = %e, "MR no respondió; se usan los precios guardados"),
        }
    }
    let rows: Vec<(i16, Decimal)> = sqlx::query_as(
        "SELECT id_producto, precio FROM precio_cliente WHERE id_persona = $1 AND id_producto = ANY($2)",
    )
    .bind(id_persona)
    .bind(ids)
    .fetch_all(&state.pool)
    .await?;
    Ok(rows.into_iter().collect())
}

// ---------- Productos ----------

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct Producto {
    pub id: i16,
    pub stand_id: i32,
    pub nombre: String,
    pub presentacion: String,
    pub codigo: String,
    pub iva: bool,
    pub precio: Decimal,
    pub precio_lista: Decimal,
    pub estado: &'static str,
}

#[derive(Deserialize, Default)]
#[serde(rename_all = "camelCase")]
pub struct Filtro {
    pub stand_id: Option<i32>,
    pub texto: Option<String>,
    pub estado: Option<String>,
}

/// Minúsculas y sin acentos, igual en Rust y en SQL (`translate`), para que "analgésico" encuentre "analgesico".
fn normaliza(s: &str) -> String {
    s.chars()
        .map(|c| match c {
            'á' | 'Á' => 'a',
            'é' | 'É' => 'e',
            'í' | 'Í' => 'i',
            'ó' | 'Ó' => 'o',
            'ú' | 'Ú' | 'ü' | 'Ü' => 'u',
            'ñ' | 'Ñ' => 'n',
            c => c.to_ascii_lowercase(),
        })
        .collect()
}

fn estado_de(status: i16) -> &'static str {
    match status {
        3 => "nuevo",
        4 => "liquidacion",
        _ => "activo",
    }
}

type Fila = (i16, i32, String, String, String, bool, Option<Decimal>, i16);

pub async fn productos(
    state: &AppState,
    user: &AuthUser,
    filtro: &Filtro,
    id: Option<i16>,
) -> Result<Vec<Producto>, ApiError> {
    let status: Option<i16> = match filtro.estado.as_deref() {
        None => None,
        Some("activo") => Some(0),
        Some("nuevo") => Some(3),
        Some("liquidacion") => Some(4),
        Some(_) => return Err(ApiError::Unprocessable("estado inválido".into())),
    };
    let texto = filtro
        .texto
        .as_deref()
        .map(|t| normaliza(t.trim()))
        .filter(|t| !t.is_empty());
    let filas: Vec<Fila> = sqlx::query_as(
        "SELECT p.id_producto, l.proveedor_id, p.nombre, coalesce(p.presentacion, ''), coalesce(p.codigo, ''),
                p.iva, p.precio_lista, p.status_indigo
         FROM producto p JOIN laboratorio l ON l.id_laboratorio = p.id_laboratorio
         WHERE p.habilitar AND p.status_indigo IN (0, 3, 4) AND l.proveedor_id IS NOT NULL
           AND ($1::int IS NULL OR l.proveedor_id = $1)
           AND ($2::smallint IS NULL OR p.status_indigo = $2)
           AND ($3::smallint IS NULL OR p.id_producto = $3)
           AND ($4::text IS NULL OR strpos(
                 lower(translate(p.nombre || ' ' || coalesce(p.presentacion, '') || ' ' || coalesce(p.codigo, ''),
                                 'áéíóúüñÁÉÍÓÚÜÑ', 'aeiouunaeiouun')), $4) > 0)
         ORDER BY l.proveedor_id, p.nombre, p.id_producto",
    )
    .bind(filtro.stand_id)
    .bind(status)
    .bind(id)
    .bind(texto.as_deref())
    .fetch_all(&state.pool)
    .await?;
    if let Some(t) = &texto {
        sqlx::query("INSERT INTO log_busqueda (usuario_id, termino) VALUES ($1, $2)")
            .bind(user.usuario_id)
            .bind(t)
            .execute(&state.pool)
            .await?;
    }
    let ids: Vec<i16> = filas.iter().map(|f| f.0).collect();
    let precios = precios(state, user.id_persona, &ids, false).await?;
    Ok(filas
        .into_iter()
        .filter_map(
            |(id, stand_id, nombre, presentacion, codigo, iva, lista, status)| {
                let precio = *precios.get(&id)?; // sin precio para este cliente = no se vende
                Some(Producto {
                    id,
                    stand_id,
                    nombre,
                    presentacion,
                    codigo,
                    iva,
                    precio,
                    precio_lista: lista.map_or(precio, |l| l.max(precio)),
                    estado: estado_de(status),
                })
            },
        )
        .collect())
}

pub async fn list_productos(
    State(state): State<AppState>,
    user: AuthUser,
    Query(filtro): Query<Filtro>,
) -> Result<Json<Vec<Producto>>, ApiError> {
    Ok(Json(productos(&state, &user, &filtro, None).await?))
}

pub async fn get_producto(
    State(state): State<AppState>,
    user: AuthUser,
    Path(id): Path<i16>,
) -> Result<Json<Producto>, ApiError> {
    productos(&state, &user, &Filtro::default(), Some(id))
        .await?
        .pop()
        .map(Json)
        .ok_or(ApiError::NotFound("producto no existe"))
}

// ---------- Promociones ----------

#[derive(Serialize, Clone)]
#[serde(rename_all = "camelCase")]
pub struct Promocion {
    pub id: i32,
    pub stand_id: i32,
    pub titulo: String,
    pub tipo: &'static str,
    pub umbral: Decimal,
    pub premio: String,
    pub acumulable: bool,
}

type FilaPromo = (
    i32,
    i32,
    String,
    Option<i32>,
    Option<Decimal>,
    i16,
    Option<String>,
);

/// Promociones activas para el cliente. Solo las de stand completo (`id_producto` nulo): las de un producto
/// concreto no caben en el contrato del frontend. Apagadas si `expo.promociones` no está en `on`.
pub async fn promociones(
    state: &AppState,
    id_persona: i32,
    stand_id: Option<i32>,
) -> Result<Vec<Promocion>, ApiError> {
    let on: Option<String> =
        sqlx::query_scalar("SELECT valor FROM setting WHERE clave = 'expo.promociones'")
            .fetch_optional(&state.pool)
            .await?;
    if on.as_deref() != Some("on") {
        return Ok(vec![]);
    }
    let rows: Vec<FilaPromo> = sqlx::query_as(
        "SELECT pr.id, pr.proveedor_id, coalesce(pr.titulo, ''), pr.piezas, pr.monto, pr.id_tipo_promocion,
                (SELECT string_agg(pa.cantidad || ' × ' || a.nombre, ', ' ORDER BY a.nombre)
                 FROM promocion_articulo pa JOIN articulo a ON a.id = pa.articulo_id WHERE pa.promocion_id = pr.id)
         FROM promocion pr
         WHERE pr.activa AND pr.id_producto IS NULL AND (pr.piezas IS NOT NULL OR pr.monto IS NOT NULL)
           AND (pr.cliente_id IS NULL OR pr.cliente_id = $1)
           AND ($2::int IS NULL OR pr.proveedor_id = $2)
         ORDER BY pr.proveedor_id, pr.id",
    )
    .bind(id_persona)
    .bind(stand_id)
    .fetch_all(&state.pool)
    .await?;
    Ok(rows
        .into_iter()
        .map(|(id, stand_id, titulo, piezas, monto, tipo, premio)| {
            let (tipo_umbral, umbral) = match piezas {
                Some(p) => ("piezas", Decimal::from(p)),
                None => ("monto", monto.unwrap_or_default()),
            };
            let premio = premio.unwrap_or_else(|| titulo.clone());
            Promocion {
                id,
                stand_id,
                titulo: if titulo.is_empty() {
                    premio.clone()
                } else {
                    titulo
                },
                tipo: tipo_umbral,
                umbral,
                premio,
                acumulable: tipo == 2,
            }
        })
        .collect())
}

#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct PromoQuery {
    pub stand_id: Option<i32>,
}

pub async fn list_promociones(
    State(state): State<AppState>,
    user: AuthUser,
    Query(q): Query<PromoQuery>,
) -> Result<Json<Vec<Promocion>>, ApiError> {
    Ok(Json(
        promociones(&state, user.id_persona, q.stand_id).await?,
    ))
}
