use async_trait::async_trait;
use axum::{body::Body, http::Request, http::StatusCode};
use cloud_api::mr::MrError;
use cloud_api::{Config, Mr};
use http_body_util::BodyExt;
use rust_decimal::Decimal;
use serde_json::{json, Value};
use sqlx::PgPool;
use std::str::FromStr;
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::{Arc, Mutex};
use tower::ServiceExt;

/// MR de mentira para las pruebas: precios fijos, crédito ajustable y caída simulable.
struct StubMr {
    credit: Mutex<Decimal>,
    down: AtomicBool,
    sin_precio: Mutex<Vec<i16>>,
}

impl StubMr {
    fn new(credit: &str) -> Arc<Self> {
        Arc::new(Self {
            credit: Mutex::new(Decimal::from_str(credit).unwrap()),
            down: AtomicBool::new(false),
            sin_precio: Mutex::new(vec![]),
        })
    }
}

#[async_trait]
impl Mr for StubMr {
    async fn prices(&self, _p: i32, productos: &[i16]) -> Result<Vec<(i16, Decimal)>, MrError> {
        if self.down.load(Ordering::SeqCst) {
            return Err(MrError("caído".into()));
        }
        Ok(productos
            .iter()
            .filter(|id| !self.sin_precio.lock().unwrap().contains(id))
            .filter_map(|id| match id {
                1 => Some((1, Decimal::from_str("12.50").unwrap())),
                2 => Some((2, Decimal::from_str("18.00").unwrap())),
                3 => Some((3, Decimal::from_str("80.00").unwrap())),
                4 => Some((4, Decimal::from_str("150.00").unwrap())),
                _ => None,
            })
            .collect())
    }
    async fn credit(&self, _p: i32) -> Result<Decimal, MrError> {
        if self.down.load(Ordering::SeqCst) {
            return Err(MrError("caído".into()));
        }
        Ok(*self.credit.lock().unwrap())
    }
}

struct Ctx {
    pool: PgPool,
    mr: Arc<StubMr>,
    token: Option<String>,
}

impl Ctx {
    async fn new(pool: PgPool, credit: &str) -> Self {
        sqlx::raw_sql(expo_common::SEED_DEV_SQL)
            .execute(&pool)
            .await
            .unwrap();
        Ctx {
            pool,
            mr: StubMr::new(credit),
            token: None,
        }
    }

    async fn call(&self, method: &str, uri: &str, body: Option<Value>) -> (StatusCode, Value) {
        let mut req = Request::builder()
            .method(method)
            .uri(uri)
            .header("content-type", "application/json");
        if let Some(t) = &self.token {
            req = req.header("authorization", format!("Bearer {t}"));
        }
        let req = req
            .body(body.map_or_else(Body::empty, |b| Body::from(b.to_string())))
            .unwrap();
        let cfg = Config {
            price_ttl_secs: 0.0, // siempre revalida contra el stub
            ..Config::default()
        };
        let app = cloud_api::router(self.pool.clone(), self.mr.clone(), cfg);
        let res = app.oneshot(req).await.unwrap();
        let status = res.status();
        let bytes = res.into_body().collect().await.unwrap().to_bytes();
        (
            status,
            serde_json::from_slice(&bytes).unwrap_or(Value::Null),
        )
    }

    async fn login(&mut self) {
        let (s, b) = self
            .call(
                "POST",
                "/auth/login",
                Some(json!({ "email": " Demo@Expo.test ", "password": "expo" })),
            )
            .await;
        assert_eq!(s, StatusCode::OK, "{b}");
        self.token = Some(b["token"].as_str().unwrap().to_string());
    }
}

fn datos() -> Value {
    json!({ "datos": { "idDocumento": 1, "usoCfdi": "G03", "direccionId": 2, "transporteId": 1, "observaciones": " entregar en la tarde " } })
}

#[sqlx::test(migrator = "expo_common::MIGRATOR")]
async fn health_ok(pool: PgPool) {
    let ctx = Ctx::new(pool, "1000").await;
    let (status, body) = ctx.call("GET", "/health", None).await;
    assert_eq!(status, StatusCode::OK);
    assert_eq!(body["status"], "ok");
}

#[sqlx::test(migrator = "expo_common::MIGRATOR")]
async fn login_sesion_y_logout(pool: PgPool) {
    let mut ctx = Ctx::new(pool, "5000").await;
    let bad = |email: &str, pw: &str| json!({ "email": email, "password": pw });
    let (s, b) = ctx
        .call("POST", "/auth/login", Some(bad("demo@expo.test", "mala")))
        .await;
    assert_eq!(
        (s, b["code"].as_str()),
        (StatusCode::UNAUTHORIZED, Some("credenciales"))
    );
    let (s, _) = ctx
        .call("POST", "/auth/login", Some(bad("nadie@expo.test", "expo")))
        .await;
    assert_eq!(s, StatusCode::UNAUTHORIZED);
    assert_eq!(
        ctx.call("GET", "/me", None).await.0,
        StatusCode::UNAUTHORIZED
    );
    assert_eq!(
        ctx.call("GET", "/stands", None).await.0,
        StatusCode::UNAUTHORIZED
    );

    ctx.login().await;
    let (s, me) = ctx.call("GET", "/me", None).await;
    assert_eq!(s, StatusCode::OK);
    assert_eq!(me["nombre"], "Farmacia Demo");
    assert_eq!(me["tipo"], "distribuidor");
    assert_eq!(me["sucursal"], "GDL");
    assert_eq!(me["credito"], 5000.0);
    assert_eq!(me["direcciones"].as_array().unwrap().len(), 2);
    assert_eq!(me["transportes"].as_array().unwrap().len(), 3);
    assert!(me.get("password_hash").is_none());

    // El token no se guarda en claro.
    let guardados: i64 = sqlx::query_scalar("SELECT count(*) FROM sesion WHERE token_hash = $1")
        .bind(ctx.token.clone().unwrap().into_bytes())
        .fetch_one(&ctx.pool)
        .await
        .unwrap();
    assert_eq!(guardados, 0);

    assert_eq!(
        ctx.call("POST", "/auth/logout", None).await.0,
        StatusCode::NO_CONTENT
    );
    assert_eq!(
        ctx.call("GET", "/me", None).await.0,
        StatusCode::UNAUTHORIZED
    );
}

#[sqlx::test(migrator = "expo_common::MIGRATOR")]
async fn sesion_vencida_no_entra(pool: PgPool) {
    let mut ctx = Ctx::new(pool, "5000").await;
    ctx.login().await;
    sqlx::query("UPDATE sesion SET expira_en = now() - interval '1 minute'")
        .execute(&ctx.pool)
        .await
        .unwrap();
    assert_eq!(
        ctx.call("GET", "/me", None).await.0,
        StatusCode::UNAUTHORIZED
    );
}

#[sqlx::test(migrator = "expo_common::MIGRATOR")]
async fn stands_catalogo_busqueda_y_promociones(pool: PgPool) {
    let mut ctx = Ctx::new(pool, "5000").await;
    ctx.login().await;

    let (_, stands) = ctx.call("GET", "/stands", None).await;
    assert_eq!(stands.as_array().unwrap().len(), 2);
    assert_eq!(stands[0]["nombre"], "Farma Norte");
    let (_, aurora) = ctx.call("GET", "/stands/9001", None).await;
    assert_eq!(aurora["anuncios"][0]["titulo"], "Compra 10, lleva 1");
    assert_eq!(
        ctx.call("GET", "/stands/1", None).await.0,
        StatusCode::NOT_FOUND
    );

    let (_, todos) = ctx.call("GET", "/products", None).await;
    assert_eq!(todos.as_array().unwrap().len(), 4);
    let (_, p) = ctx.call("GET", "/products/3", None).await;
    assert_eq!(p["standId"], 9002);
    assert_eq!(p["estado"], "nuevo");
    assert_eq!(p["precio"], 80.0); // precio del servidor
    assert_eq!(p["precioLista"], 86.4);

    let (_, por_stand) = ctx.call("GET", "/products?standId=9001", None).await;
    assert_eq!(por_stand.as_array().unwrap().len(), 2);
    let (_, liq) = ctx.call("GET", "/products?estado=liquidacion", None).await;
    assert_eq!(liq[0]["nombre"], "Protector solar FPS 50");
    // Sin acentos ni mayúsculas, y también por código.
    let (_, busq) = ctx.call("GET", "/products?texto=CAPSULAS", None).await;
    assert_eq!(busq[0]["id"], 3);
    let (_, busq) = ctx.call("GET", "/products?texto=mr00002", None).await;
    assert_eq!(busq[0]["id"], 2);
    assert_eq!(
        ctx.call("GET", "/products?estado=raro", None).await.0,
        StatusCode::UNPROCESSABLE_ENTITY
    );
    assert_eq!(
        ctx.call("GET", "/products/99", None).await.0,
        StatusCode::NOT_FOUND
    );

    // Un producto sin precio para el cliente no se ofrece.
    ctx.mr.sin_precio.lock().unwrap().push(4);
    let (_, todos) = ctx.call("GET", "/products", None).await;
    assert_eq!(todos.as_array().unwrap().len(), 3);

    let (_, promos) = ctx.call("GET", "/promotions", None).await;
    assert_eq!(promos[0]["standId"], 9001);
    assert_eq!(promos[0]["tipo"], "piezas");
    assert_eq!(promos[0]["umbral"], 10.0);
    assert_eq!(promos[0]["acumulable"], true);
    assert_eq!(promos[0]["premio"], "1 × caja de paracetamol 500 mg");
    let (_, otros) = ctx.call("GET", "/promotions?standId=9002", None).await;
    assert!(otros.as_array().unwrap().is_empty());
    sqlx::query("UPDATE setting SET valor = 'off' WHERE clave = 'expo.promociones'")
        .execute(&ctx.pool)
        .await
        .unwrap();
    let (_, apagadas) = ctx.call("GET", "/promotions", None).await;
    assert!(apagadas.as_array().unwrap().is_empty());
}

#[sqlx::test(migrator = "expo_common::MIGRATOR")]
async fn carrito_agrega_confirma_y_quita(pool: PgPool) {
    let mut ctx = Ctx::new(pool, "5000").await;
    ctx.login().await;
    assert_eq!(ctx.call("GET", "/cart", None).await.1, json!([]));

    let (_, c) = ctx
        .call("PUT", "/cart/items/1", Some(json!({ "cantidad": 3 })))
        .await;
    assert_eq!(
        c,
        json!([{ "productoId": 1, "cantidad": 3, "confirmado": true }])
    );
    let (_, c) = ctx
        .call("PUT", "/cart/items/3", Some(json!({ "cantidad": 2 })))
        .await;
    assert_eq!(c.as_array().unwrap().len(), 2);
    // Cambiar la cantidad conserva la confirmación.
    ctx.call(
        "PUT",
        "/cart/items/1/confirm",
        Some(json!({ "confirmado": false })),
    )
    .await;
    let (_, c) = ctx
        .call("PUT", "/cart/items/1", Some(json!({ "cantidad": 5 })))
        .await;
    assert_eq!(
        c[0],
        json!({ "productoId": 1, "cantidad": 5, "confirmado": false })
    );
    let (_, c) = ctx
        .call("PUT", "/cart/confirm", Some(json!({ "confirmado": false })))
        .await;
    assert!(c
        .as_array()
        .unwrap()
        .iter()
        .all(|l| l["confirmado"] == false));
    let (_, c) = ctx
        .call("PUT", "/cart/confirm", Some(json!({ "confirmado": true })))
        .await;
    assert!(c
        .as_array()
        .unwrap()
        .iter()
        .all(|l| l["confirmado"] == true));
    let (_, c) = ctx
        .call("PUT", "/cart/items/1", Some(json!({ "cantidad": 0 })))
        .await;
    assert_eq!(c.as_array().unwrap().len(), 1);

    assert_eq!(
        ctx.call("PUT", "/cart/items/99", Some(json!({ "cantidad": 1 })))
            .await
            .0,
        StatusCode::NOT_FOUND
    );
    assert_eq!(
        ctx.call(
            "PUT",
            "/cart/items/1",
            Some(json!({ "cantidad": 10000000 }))
        )
        .await
        .0,
        StatusCode::UNPROCESSABLE_ENTITY
    );
}

#[sqlx::test(migrator = "expo_common::MIGRATOR")]
async fn coloca_pedido_con_precio_credito_promo_y_outbox(pool: PgPool) {
    let mut ctx = Ctx::new(pool, "1000").await;
    ctx.login().await;
    ctx.call("PUT", "/cart/items/1", Some(json!({ "cantidad": 20 })))
        .await; // 20 × 12.50 = 250 (2 premios)
    ctx.call("PUT", "/cart/items/4", Some(json!({ "cantidad": 2 })))
        .await; // 2 × 150 + IVA = 348
    ctx.call("PUT", "/cart/items/3", Some(json!({ "cantidad": 1 })))
        .await;
    ctx.call(
        "PUT",
        "/cart/items/3/confirm",
        Some(json!({ "confirmado": false })),
    )
    .await;

    let (s, p) = ctx.call("POST", "/orders", Some(datos())).await;
    assert_eq!(s, StatusCode::CREATED, "{p}");
    assert_eq!(p["estado"], "recibido");
    assert_eq!(p["total"], 598.0);
    assert_eq!(p["folio"], "EXPO-1");
    assert_eq!(
        p["promociones"],
        json!(["2 × 1 × caja de paracetamol 500 mg"])
    );
    assert_eq!(
        p["datos"],
        json!({ "idDocumento": 1, "usoCfdi": "G03", "direccionId": 2, "transporteId": 1, "observaciones": "entregar en la tarde" })
    );
    assert_eq!(p["lineas"].as_array().unwrap().len(), 2);
    assert_eq!(p["lineas"][1]["nombre"], "Protector solar FPS 50");

    // Solo sale lo confirmado; lo no confirmado se queda en el carrito.
    let (_, c) = ctx.call("GET", "/cart", None).await;
    assert_eq!(
        c,
        json!([{ "productoId": 3, "cantidad": 1, "confirmado": false }])
    );
    let (pendientes, unitario): (i64, rust_decimal::Decimal) = sqlx::query_as(
        "SELECT (SELECT count(*) FROM outbox WHERE enviado_en IS NULL), (SELECT unitario FROM pedido_item WHERE id_producto = 1)",
    )
    .fetch_one(&ctx.pool)
    .await
    .unwrap();
    assert_eq!((pendientes, unitario.to_string().as_str()), (1, "12.5000"));

    // El pedido sin sincronizar ya consume crédito (MR todavía no lo sabe): 1000 - 598.
    let (_, cr) = ctx.call("GET", "/me/credit", None).await;
    assert_eq!(cr["disponible"], 402.0);

    // Listado y detalle.
    let (_, lista) = ctx.call("GET", "/orders", None).await;
    assert_eq!(lista.as_array().unwrap().len(), 1);
    let (s, det) = ctx
        .call(
            "GET",
            &format!("/orders/{}", p["id"].as_str().unwrap()),
            None,
        )
        .await;
    assert_eq!((s, &det["folio"]), (StatusCode::OK, &json!("EXPO-1")));

    // Cuando MR lo surte, el cliente ve el avance.
    sqlx::raw_sql("UPDATE pedido SET estado = 'parcial'; UPDATE pedido_item SET surtido = 5 WHERE id_producto = 1")
        .execute(&ctx.pool)
        .await
        .unwrap();
    let (_, det) = ctx
        .call(
            "GET",
            &format!("/orders/{}", p["id"].as_str().unwrap()),
            None,
        )
        .await;
    assert_eq!(det["estado"], "parcial");
    assert_eq!(det["lineas"][0]["surtido"], 5);
}

#[sqlx::test(migrator = "expo_common::MIGRATOR")]
async fn rechaza_pedidos_invalidos(pool: PgPool) {
    let mut ctx = Ctx::new(pool, "300").await;
    ctx.login().await;

    let (s, b) = ctx.call("POST", "/orders", Some(datos())).await;
    assert_eq!(
        (s, b["code"].as_str()),
        (StatusCode::UNPROCESSABLE_ENTITY, Some("carrito-vacio"))
    );

    ctx.call("PUT", "/cart/items/4", Some(json!({ "cantidad": 2 })))
        .await; // 348 > 300
    let (s, b) = ctx.call("POST", "/orders", Some(datos())).await;
    assert_eq!(
        (s, b["code"].as_str()),
        (StatusCode::CONFLICT, Some("sin-credito"))
    );

    *ctx.mr.credit.lock().unwrap() = Decimal::from(10_000);
    let con = |f: fn(&mut Value)| {
        let mut d = datos();
        f(&mut d["datos"]);
        d
    };
    let malos = [
        con(|d| d["idDocumento"] = json!(9)),
        con(|d| d["usoCfdi"] = json!("XXX")),
        con(|d| d["direccionId"] = json!(99)),
        con(|d| d["transporteId"] = json!(99)),
    ];
    for m in malos {
        assert_eq!(
            ctx.call("POST", "/orders", Some(m)).await.0,
            StatusCode::UNPROCESSABLE_ENTITY
        );
    }

    // Nota de remisión no necesita uso de CFDI válido.
    let nota = con(|d| {
        d["idDocumento"] = json!(2);
        d["usoCfdi"] = json!("");
    });
    // MR caído: el pedido no se coloca y el carrito queda intacto.
    ctx.mr.down.store(true, Ordering::SeqCst);
    let (s, b) = ctx.call("POST", "/orders", Some(nota.clone())).await;
    assert_eq!(
        (s, b["code"].as_str()),
        (StatusCode::SERVICE_UNAVAILABLE, Some("mr-no-disponible"))
    );
    ctx.mr.down.store(false, Ordering::SeqCst);
    let n: i64 = sqlx::query_scalar("SELECT count(*) FROM pedido")
        .fetch_one(&ctx.pool)
        .await
        .unwrap();
    assert_eq!(n, 0);
    assert_eq!(
        ctx.call("GET", "/cart", None)
            .await
            .1
            .as_array()
            .unwrap()
            .len(),
        1
    );
    assert_eq!(
        ctx.call("POST", "/orders", Some(nota)).await.0,
        StatusCode::CREATED
    );

    assert_eq!(
        ctx.call("GET", "/orders/00000000-0000-0000-0000-000000000000", None)
            .await
            .0,
        StatusCode::NOT_FOUND
    );
}

#[sqlx::test(migrator = "expo_common::MIGRATOR")]
async fn un_cliente_no_ve_pedidos_de_otro(pool: PgPool) {
    let mut ctx = Ctx::new(pool, "10000").await;
    ctx.login().await;
    ctx.call("PUT", "/cart/items/1", Some(json!({ "cantidad": 1 })))
        .await;
    let (_, p) = ctx.call("POST", "/orders", Some(datos())).await;
    let id = p["id"].as_str().unwrap().to_string();

    sqlx::query(
        "INSERT INTO cliente (id_persona, nombre, sucursal_id) VALUES (2002, 'Otra Farmacia', 1)",
    )
    .execute(&ctx.pool)
    .await
    .unwrap();
    let hash = cloud_api::hash_password("otra").unwrap();
    sqlx::query("INSERT INTO usuario (id_email, id_persona, email, password_hash, tipo_usuario, sucursal_id) VALUES (502, 2002, 'otra@expo.test', $1, 1, 1)")
        .bind(hash)
        .execute(&ctx.pool)
        .await
        .unwrap();
    let (_, b) = ctx
        .call(
            "POST",
            "/auth/login",
            Some(json!({ "email": "otra@expo.test", "password": "otra" })),
        )
        .await;
    ctx.token = Some(b["token"].as_str().unwrap().to_string());
    assert_eq!(
        ctx.call("GET", &format!("/orders/{id}"), None).await.0,
        StatusCode::NOT_FOUND
    );
    assert_eq!(ctx.call("GET", "/orders", None).await.1, json!([]));
    assert_eq!(ctx.call("GET", "/cart", None).await.1, json!([]));
}
