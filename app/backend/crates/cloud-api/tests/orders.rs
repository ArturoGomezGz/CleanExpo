use axum::{body::Body, http::Request, http::StatusCode};
use http_body_util::BodyExt;
use serde_json::{json, Value};
use sqlx::PgPool;
use tower::ServiceExt;

async fn call(pool: &PgPool, method: &str, uri: &str, body: Option<Value>) -> (StatusCode, Value) {
    let req = Request::builder()
        .method(method)
        .uri(uri)
        .header("content-type", "application/json")
        .body(body.map_or_else(Body::empty, |b| Body::from(b.to_string())))
        .unwrap();
    let res = cloud_api::router(pool.clone()).oneshot(req).await.unwrap();
    let status = res.status();
    let bytes = res.into_body().collect().await.unwrap().to_bytes();
    (
        status,
        serde_json::from_slice(&bytes).unwrap_or(Value::Null),
    )
}

async fn seed(pool: &PgPool) {
    sqlx::raw_sql(expo_common::SEED_DEV_SQL)
        .execute(pool)
        .await
        .unwrap();
}

#[sqlx::test(migrator = "expo_common::MIGRATOR")]
async fn health_ok(pool: PgPool) {
    let (status, body) = call(&pool, "GET", "/health", None).await;
    assert_eq!(status, StatusCode::OK);
    assert_eq!(body["status"], "ok");
}

#[sqlx::test(migrator = "expo_common::MIGRATOR")]
async fn crea_pedido_con_precio_del_servidor_y_outbox(pool: PgPool) {
    seed(&pool).await;
    let (status, created) = call(
        &pool,
        "POST",
        "/orders",
        Some(json!({ "id_persona": 1001, "usuario_id": 1, "items": [{ "id_producto": 1, "cantidad": 3 }] })),
    )
    .await;
    assert_eq!(status, StatusCode::ACCEPTED);
    assert_eq!(created["estado"], "recibido");

    let (unitario, pendientes): (rust_decimal::Decimal, i64) = sqlx::query_as(
        "SELECT (SELECT unitario FROM pedido_item LIMIT 1), (SELECT count(*) FROM outbox WHERE enviado_en IS NULL)",
    )
    .fetch_one(&pool)
    .await
    .unwrap();
    assert_eq!(unitario.to_string(), "12.5000");
    assert_eq!(pendientes, 1);

    let (status, st) = call(
        &pool,
        "GET",
        &format!("/orders/{}", created["id"].as_str().unwrap()),
        None,
    )
    .await;
    assert_eq!(status, StatusCode::OK);
    assert_eq!(st["estado"], "recibido");
    assert_eq!(st["items"][0]["cantidad"], 3);
}

#[sqlx::test(migrator = "expo_common::MIGRATOR")]
async fn rechaza_pedidos_invalidos(pool: PgPool) {
    seed(&pool).await;
    let post = |b: Value| call(&pool, "POST", "/orders", Some(b));
    assert_eq!(post(json!({ "id_persona": 999, "usuario_id": 1, "items": [{ "id_producto": 1, "cantidad": 1 }] })).await.0, StatusCode::NOT_FOUND);
    assert_eq!(post(json!({ "id_persona": 1001, "usuario_id": 1, "items": [{ "id_producto": 77, "cantidad": 1 }] })).await.0, StatusCode::UNPROCESSABLE_ENTITY);
    assert_eq!(post(json!({ "id_persona": 1001, "usuario_id": 1, "items": [{ "id_producto": 1, "cantidad": 0 }] })).await.0, StatusCode::UNPROCESSABLE_ENTITY);
    assert_eq!(
        post(json!({ "id_persona": 1001, "usuario_id": 1, "items": [] }))
            .await
            .0,
        StatusCode::UNPROCESSABLE_ENTITY
    );
    let n: i64 = sqlx::query_scalar("SELECT count(*) FROM pedido")
        .fetch_one(&pool)
        .await
        .unwrap();
    assert_eq!(n, 0);
    let (status, _) = call(
        &pool,
        "GET",
        "/orders/00000000-0000-0000-0000-000000000000",
        None,
    )
    .await;
    assert_eq!(status, StatusCode::NOT_FOUND);
}
