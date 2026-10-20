//! API de la nube: tienda (auth, catálogo, carrito) y pedidos en Postgres + outbox. Nunca habla con SQL Server;
//! precios y crédito los pide a la API interna de MR.

use anyhow::{bail, Result};
use cloud_api::mr::{FakeMr, HttpMr};
use cloud_api::{router, Config, Mr};
use expo_common::{connect, init_tracing, migrate, shutdown_signal};
use std::sync::Arc;

fn env_parse<T: std::str::FromStr>(name: &str, default: T) -> T {
    std::env::var(name)
        .ok()
        .and_then(|v| v.parse().ok())
        .unwrap_or(default)
}

#[tokio::main]
async fn main() -> Result<()> {
    init_tracing();
    let pool = connect(10).await?;
    let seed_dev = std::env::var("SEED_DEV").is_ok_and(|v| v == "true");
    if std::env::var("RUN_MIGRATIONS").map_or(true, |v| v != "false") {
        migrate(&pool, seed_dev).await?;
    }

    // MR_MODE=http llama a la API interna; MR_MODE=fake simula precios y crédito (solo desarrollo).
    // Sin MR_MODE: http si hay INTERNAL_API_URL, fake solo con SEED_DEV=true. Nunca fake "por accidente" en producción.
    let url = std::env::var("INTERNAL_API_URL")
        .ok()
        .filter(|v| !v.is_empty());
    let mode = std::env::var("MR_MODE").unwrap_or_else(|_| {
        if url.is_some() {
            "http"
        } else if seed_dev {
            "fake"
        } else {
            ""
        }
        .into()
    });
    let mr: Arc<dyn Mr> = match mode.as_str() {
        "http" => {
            let Some(url) = url else {
                bail!("MR_MODE=http requiere INTERNAL_API_URL")
            };
            let key = expo_common::env_required("INTERNAL_API_KEY")?;
            Arc::new(HttpMr::new(&url, &key)?)
        }
        "fake" => {
            tracing::warn!("MR_MODE=fake: precios y crédito simulados, NO usar en producción");
            Arc::new(FakeMr { pool: pool.clone() })
        }
        _ => bail!("define INTERNAL_API_URL (+ INTERNAL_API_KEY) o MR_MODE=fake para desarrollo"),
    };

    let cfg = Config {
        price_ttl_secs: env_parse("PRICE_TTL_SECS", 900.0),
        session_hours: env_parse("SESSION_HOURS", 24),
        cors_origins: std::env::var("CORS_ORIGINS")
            .unwrap_or_default()
            .split(',')
            .map(|s| s.trim().to_string())
            .filter(|s| !s.is_empty())
            .collect(),
    };

    // Railway inyecta PORT; en local usamos 8000 (el nginx de `web` apunta ahí).
    let port = std::env::var("PORT").unwrap_or_else(|_| "8000".into());
    let listener = tokio::net::TcpListener::bind(format!("0.0.0.0:{port}")).await?;
    tracing::info!(%port, %mode, "cloud-api escuchando");
    axum::serve(listener, router(pool, mr, cfg))
        .with_graceful_shutdown(shutdown_signal())
        .await?;
    Ok(())
}
