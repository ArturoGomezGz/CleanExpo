use axum::{extract::State, http::HeaderMap, http::StatusCode, routing::post, Json, Router};
use serde_json::{json, Value};
use sqlx::PgPool;
use std::sync::{Arc, Mutex};
use worker::{process_one, Config};

#[derive(Clone)]
struct Mock {
    status: StatusCode,
    seen: Arc<Mutex<Vec<(String, Value)>>>,
}

async fn handler(
    State(m): State<Mock>,
    headers: HeaderMap,
    Json(body): Json<Value>,
) -> (StatusCode, Json<Value>) {
    let key = headers["idempotency-key"].to_str().unwrap().to_string();
    m.seen.lock().unwrap().push((key, body));
    let resp = match m.status {
        StatusCode::OK => {
            json!({ "idPedidoMr": 4242, "surtido": [{ "idProducto": 1, "surtido": 2, "pendiente": 1 }] })
        }
        _ => json!({ "detail": "x" }),
    };
    (m.status, Json(resp))
}

async fn start(status: StatusCode) -> (Config, Arc<Mutex<Vec<(String, Value)>>>) {
    let seen = Arc::new(Mutex::new(vec![]));
    let app = Router::new()
        .route("/orders", post(handler))
        .with_state(Mock {
            status,
            seen: seen.clone(),
        });
    let listener = tokio::net::TcpListener::bind("127.0.0.1:0").await.unwrap();
    let addr = listener.local_addr().unwrap();
    tokio::spawn(async move { axum::serve(listener, app).await.unwrap() });
    (
        Config {
            internal_api_url: format!("http://{addr}"),
            internal_api_key: "k".into(),
            ..Config::default()
        },
        seen,
    )
}

async fn pedido_en_outbox(pool: &PgPool) -> uuid::Uuid {
    sqlx::raw_sql(expo_common::SEED_DEV_SQL)
        .execute(pool)
        .await
        .unwrap();
    let id: uuid::Uuid = sqlx::query_scalar(
        "INSERT INTO pedido (id_persona, usuario_id, sucursal_id) VALUES (1001, 1, 1) RETURNING id",
    )
    .fetch_one(pool)
    .await
    .unwrap();
    sqlx::query("INSERT INTO pedido_item (pedido_id, id_producto, unitario, descuento, cantidad) VALUES ($1, 1, 12.50, 0, 3)")
        .bind(id).execute(pool).await.unwrap();
    sqlx::query("INSERT INTO outbox (pedido_id) VALUES ($1)")
        .bind(id)
        .execute(pool)
        .await
        .unwrap();
    id
}

async fn estado(pool: &PgPool, id: uuid::Uuid) -> String {
    sqlx::query_scalar("SELECT estado::text FROM pedido WHERE id = $1")
        .bind(id)
        .fetch_one(pool)
        .await
        .unwrap()
}

#[sqlx::test(migrator = "expo_common::MIGRATOR")]
async fn sincroniza_con_idempotency_key(pool: PgPool) {
    let id = pedido_en_outbox(&pool).await;
    let (cfg, seen) = start(StatusCode::OK).await;
    let client = reqwest::Client::new();
    assert!(process_one(&pool, &client, &cfg).await.unwrap());
    assert!(
        !process_one(&pool, &client, &cfg).await.unwrap(),
        "ya no queda nada pendiente"
    );

    assert_eq!(estado(&pool, id).await, "sincronizado");
    let (mr, surtido, colocado): (Option<i32>, i32, bool) = sqlx::query_as(
        "SELECT (SELECT id_pedido_mr FROM pedido WHERE id = $1), surtido, colocado FROM pedido_item WHERE pedido_id = $1",
    ).bind(id).fetch_one(&pool).await.unwrap();
    assert_eq!((mr, surtido, colocado), (Some(4242), 2, false));
    let seen = seen.lock().unwrap();
    assert_eq!(seen.len(), 1);
    assert_eq!(seen[0].0, id.to_string());
    assert_eq!(seen[0].1["items"][0]["unitario"], 12.5);
    assert_eq!(seen[0].1["idEmail"], 501);
}

#[sqlx::test(migrator = "expo_common::MIGRATOR")]
async fn reintenta_con_backoff_si_falla(pool: PgPool) {
    let id = pedido_en_outbox(&pool).await;
    let (cfg, _) = start(StatusCode::BAD_GATEWAY).await;
    assert!(process_one(&pool, &reqwest::Client::new(), &cfg)
        .await
        .unwrap());
    assert_eq!(estado(&pool, id).await, "recibido");
    let (intentos, espera, error): (i32, f64, Option<String>) = sqlx::query_as(
        "SELECT intentos, EXTRACT(EPOCH FROM siguiente_intento - now())::float8, ultimo_error FROM outbox",
    ).fetch_one(&pool).await.unwrap();
    assert_eq!(intentos, 1);
    assert!(espera > 0.0 && espera <= 4.0, "backoff de 4s, fue {espera}");
    assert!(error.unwrap().contains("502"));
    assert!(
        !process_one(&pool, &reqwest::Client::new(), &cfg)
            .await
            .unwrap(),
        "aún no toca reintentar"
    );
}

#[sqlx::test(migrator = "expo_common::MIGRATOR")]
async fn conflicto_marca_error_sin_reintentar(pool: PgPool) {
    let id = pedido_en_outbox(&pool).await;
    let (cfg, _) = start(StatusCode::CONFLICT).await;
    assert!(process_one(&pool, &reqwest::Client::new(), &cfg)
        .await
        .unwrap());
    assert_eq!(estado(&pool, id).await, "error");
    assert!(!process_one(&pool, &reqwest::Client::new(), &cfg)
        .await
        .unwrap());
}
