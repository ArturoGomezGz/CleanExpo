//! Piezas compartidas por los servicios de la nube: logging, pool de Postgres y migraciones.

use anyhow::{Context, Result};
use sqlx::postgres::{PgPool, PgPoolOptions};
use std::time::Duration;
use tracing_subscriber::EnvFilter;

/// Migraciones del esquema de la nube. Las aplica `cloud-api` al arrancar; el resto de servicios solo leen/escriben.
pub static MIGRATOR: sqlx::migrate::Migrator = sqlx::migrate!("../../migrations");

/// Datos ficticios de desarrollo; solo se cargan con `SEED_DEV=true`.
pub const SEED_DEV_SQL: &str = include_str!("../../../seed-dev.sql");

pub fn init_tracing() {
    let filter = EnvFilter::try_from_default_env().unwrap_or_else(|_| EnvFilter::new("info"));
    let json = std::env::var("LOG_FORMAT").is_ok_and(|v| v == "json");
    let builder = tracing_subscriber::fmt().with_env_filter(filter);
    if json {
        builder.json().init();
    } else {
        builder.init();
    }
}

pub fn env_required(name: &str) -> Result<String> {
    std::env::var(name).with_context(|| format!("falta la variable de entorno {name}"))
}

pub async fn connect(max_connections: u32) -> Result<PgPool> {
    let url = env_required("DATABASE_URL")?;
    PgPoolOptions::new()
        .max_connections(max_connections)
        .acquire_timeout(Duration::from_secs(5))
        .connect(&url)
        .await
        .context("no se pudo conectar a Postgres")
}

pub async fn migrate(pool: &PgPool, seed_dev: bool) -> Result<()> {
    MIGRATOR
        .run(pool)
        .await
        .context("fallaron las migraciones")?;
    if seed_dev {
        let ya_hay_datos: bool = sqlx::query_scalar("SELECT EXISTS (SELECT 1 FROM sucursal)")
            .fetch_one(pool)
            .await?;
        if !ya_hay_datos {
            sqlx::raw_sql(SEED_DEV_SQL)
                .execute(pool)
                .await
                .context("falló el seed de desarrollo")?;
            tracing::info!("seed de desarrollo cargado");
        }
    }
    Ok(())
}

/// Espera SIGINT/SIGTERM para apagado ordenado (Railway envía SIGTERM al redeployar).
pub async fn shutdown_signal() {
    let ctrl_c = async {
        let _ = tokio::signal::ctrl_c().await;
    };
    #[cfg(unix)]
    let term = async {
        if let Ok(mut s) = tokio::signal::unix::signal(tokio::signal::unix::SignalKind::terminate())
        {
            s.recv().await;
        }
    };
    #[cfg(not(unix))]
    let term = std::future::pending::<()>();
    tokio::select! { _ = ctrl_c => {}, _ = term => {} }
}
