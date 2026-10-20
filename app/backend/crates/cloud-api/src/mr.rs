//! Lo que cloud-api necesita de Medicinas Rosario: precios por cliente y crédito.
//! Nunca se reimplementa la lógica del ERP (ver `internal-api.openapi.yaml`): en producción se llama a la API
//! interna; `FakeMr` existe solo para desarrollo y pruebas.

use async_trait::async_trait;
use rust_decimal::Decimal;
use serde::Deserialize;
use sqlx::PgPool;
use std::time::Duration;

#[derive(Debug)]
pub struct MrError(pub String);

impl std::fmt::Display for MrError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.write_str(&self.0)
    }
}

#[async_trait]
pub trait Mr: Send + Sync {
    /// Precio unitario (sin IVA) por producto para el cliente. Los productos sin precio no aparecen.
    async fn prices(
        &self,
        id_persona: i32,
        productos: &[i16],
    ) -> Result<Vec<(i16, Decimal)>, MrError>;
    /// Crédito que MR reconoce hoy al cliente (ya descontados sus pedidos sincronizados).
    async fn credit(&self, id_persona: i32) -> Result<Decimal, MrError>;
}

/// Máximo de productos por llamada a `POST /prices` (límite de mr-api).
const LOTE_PRECIOS: usize = 500;

pub struct HttpMr {
    base: String,
    key: String,
    client: reqwest::Client,
}

impl HttpMr {
    pub fn new(base: &str, key: &str) -> Result<Self, reqwest::Error> {
        Ok(Self {
            base: base.trim_end_matches('/').to_string(),
            key: key.to_string(),
            client: reqwest::Client::builder()
                .timeout(Duration::from_secs(15))
                .build()?,
        })
    }
}

#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
struct PricesResponse {
    precios: Vec<PriceRow>,
}
#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
struct PriceRow {
    id_producto: i16,
    precio: Decimal,
}
#[derive(Deserialize)]
struct CreditResponse {
    disponible: Decimal,
}

fn http_err(e: impl std::fmt::Display) -> MrError {
    MrError(e.to_string())
}

#[async_trait]
impl Mr for HttpMr {
    async fn prices(
        &self,
        id_persona: i32,
        productos: &[i16],
    ) -> Result<Vec<(i16, Decimal)>, MrError> {
        let mut out = Vec::with_capacity(productos.len());
        for lote in productos.chunks(LOTE_PRECIOS) {
            let r: PricesResponse = self
                .client
                .post(format!("{}/prices", self.base))
                .header("X-Api-Key", &self.key)
                .json(&serde_json::json!({ "idPersona": id_persona, "productos": lote }))
                .send()
                .await
                .and_then(|r| r.error_for_status())
                .map_err(http_err)?
                .json()
                .await
                .map_err(http_err)?;
            out.extend(r.precios.into_iter().map(|p| (p.id_producto, p.precio)));
        }
        Ok(out)
    }

    async fn credit(&self, id_persona: i32) -> Result<Decimal, MrError> {
        let r: CreditResponse = self
            .client
            .get(format!("{}/clients/{id_persona}/credit", self.base))
            .header("X-Api-Key", &self.key)
            .send()
            .await
            .and_then(|r| r.error_for_status())
            .map_err(http_err)?
            .json()
            .await
            .map_err(http_err)?;
        Ok(r.disponible)
    }
}

/// Simulador de desarrollo: lista = costo × 1.35 y el distribuidor recibe 12 % menos (igual que el mock del
/// frontend); el crédito es `cliente.credito_expo` menos lo ya sincronizado con MR.
pub struct FakeMr {
    pub pool: PgPool,
}

#[async_trait]
impl Mr for FakeMr {
    async fn prices(
        &self,
        id_persona: i32,
        productos: &[i16],
    ) -> Result<Vec<(i16, Decimal)>, MrError> {
        let rows: Vec<(i16, Decimal)> = sqlx::query_as(
            "SELECT p.id_producto,
                    round(coalesce(p.precio_lista, p.costo * 1.35)
                          * CASE WHEN c.tipo_cliente = 2 THEN 0.88 ELSE 1 END, 2)
             FROM producto p, cliente c
             WHERE c.id_persona = $1 AND p.id_producto = ANY($2)
               AND coalesce(p.precio_lista, p.costo) IS NOT NULL",
        )
        .bind(id_persona)
        .bind(productos)
        .fetch_all(&self.pool)
        .await
        .map_err(http_err)?;
        Ok(rows)
    }

    async fn credit(&self, id_persona: i32) -> Result<Decimal, MrError> {
        let c: Option<Decimal> = sqlx::query_scalar(
            "SELECT c.credito_expo - coalesce((
                 SELECT sum(round(i.cantidad * i.unitario, 2) + CASE WHEN pr.iva THEN round(i.cantidad * i.unitario * 0.16, 2) ELSE 0 END)
                 FROM pedido p JOIN pedido_item i ON i.pedido_id = p.id JOIN producto pr ON pr.id_producto = i.id_producto
                 WHERE p.id_persona = c.id_persona AND p.estado IN ('sincronizado','parcial','surtido')), 0)
             FROM cliente c WHERE c.id_persona = $1",
        )
        .bind(id_persona)
        .fetch_optional(&self.pool)
        .await
        .map_err(http_err)?;
        c.ok_or_else(|| MrError("cliente no existe".into()))
    }
}
