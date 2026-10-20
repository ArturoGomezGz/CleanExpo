//! API de la nube: recibe pedidos y los deja en Postgres + outbox. Nunca habla con SQL Server.

use anyhow::Result;
use cloud_api::router;
use expo_common::{connect, init_tracing, migrate, shutdown_signal};

#[tokio::main]
async fn main() -> Result<()> {
    init_tracing();
    let pool = connect(10).await?;
    if std::env::var("RUN_MIGRATIONS").map_or(true, |v| v != "false") {
        migrate(&pool, std::env::var("SEED_DEV").is_ok_and(|v| v == "true")).await?;
    }
    // Railway inyecta PORT; en local usamos 8000 (el nginx de `web` apunta ahí).
    let port = std::env::var("PORT").unwrap_or_else(|_| "8000".into());
    let listener = tokio::net::TcpListener::bind(format!("0.0.0.0:{port}")).await?;
    tracing::info!(%port, "cloud-api escuchando");
    axum::serve(listener, router(pool))
        .with_graceful_shutdown(shutdown_signal())
        .await?;
    Ok(())
}
