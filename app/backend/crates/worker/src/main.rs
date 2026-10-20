//! Worker de outbox: envía pedidos pendientes a la API interna con reintentos e idempotencia.

use anyhow::Result;
use expo_common::{connect, env_required, init_tracing, shutdown_signal};
use std::time::Duration;
use worker::{process_one, Config};

#[tokio::main]
async fn main() -> Result<()> {
    init_tracing();
    let pool = connect(3).await?;
    let config = Config {
        internal_api_url: env_required("INTERNAL_API_URL")?
            .trim_end_matches('/')
            .to_string(),
        internal_api_key: env_required("INTERNAL_API_KEY")?,
        ..Config::default()
    };
    let poll = Duration::from_secs_f64(
        std::env::var("POLL_SECONDS")
            .ok()
            .and_then(|v| v.parse().ok())
            .unwrap_or(2.0),
    );
    let client = reqwest::Client::builder()
        .timeout(Duration::from_secs(20))
        .build()?;

    let run = async {
        loop {
            match process_one(&pool, &client, &config).await {
                Ok(true) => {}
                Ok(false) => tokio::time::sleep(poll).await,
                Err(e) => {
                    tracing::error!(error = %e, "fallo del ciclo del worker");
                    tokio::time::sleep(poll).await;
                }
            }
        }
    };
    tokio::select! { _ = run => {}, _ = shutdown_signal() => tracing::info!("apagando worker") }
    Ok(())
}
