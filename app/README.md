# Clean Expo — plataforma (esqueleto)

Reemplazo contenerizado del sistema legacy ExpoMR (carpeta raíz del repo). Diseño y análisis en [`docs/`](docs/).

```
nube (Railway)                              red interna de MR
┌────────────────────────────────┐          ┌───────────────────────────────┐
│ web ─▶ cloud-api ─▶ postgres   │          │ lb ─▶ internal-api (N réplicas)│
│              worker (outbox) ──┼─ túnel ─▶│            └─▶ puente ExpoMR    │
└────────────────────────────────┘          │                 + FacturarExpo │
                                            └───────────────────────────────┘
```

| Servicio | Qué hace |
|---|---|
| `postgres` | Base de la nube. El esquema vive en `backend/migrations` (sqlx); `db/init` solo crea la base `bridge` de la simulación |
| `cloud-api` | Rust (axum + sqlx), en `backend/`. Migra el esquema al arrancar, recibe pedidos, calcula precios solo en servidor, guarda pedido + outbox |
| `worker` | Rust, en `backend/`. Envía pedidos pendientes a la API interna con `Idempotency-Key` y reintentos con backoff |
| `images` | Rust, en `backend/` (solo librería por ahora): valida límites y genera variantes WebP |
| `internal-api` | Zona MR, stateless. **Aquí es una simulación**: el puente se simula con la base `bridge` (Postgres). En producción escribe en ExpoMR y ejecuta `Indigo.dbo.FacturarExpo` por sucursal |
| `lb` | Nginx round-robin entre réplicas de `internal-api` (práctica de balanceo) |
| `web` | Placeholder con tema por *design tokens* (`web/themes/<año>/tokens.css`) |

## Correr en local
```
cp .env.example .env
docker compose up --build --scale internal-api=2
curl localhost:8000/api/health
# frontend en http://localhost:3000 (cuenta demo@expo.test / expo). Por API:
TOKEN=$(curl -s -XPOST localhost:8000/api/auth/login -H 'content-type: application/json' \
  -d '{"email":"demo@expo.test","password":"expo"}' | jq -r .token)
curl -H "authorization: Bearer $TOKEN" localhost:8000/api/products
curl -XPUT -H "authorization: Bearer $TOKEN" -H 'content-type: application/json' localhost:8000/api/cart/items/1 -d '{"cantidad":3}'
curl -XPOST -H "authorization: Bearer $TOKEN" -H 'content-type: application/json' localhost:8000/api/orders \
  -d '{"datos":{"idDocumento":1,"usoCfdi":"G03","direccionId":1,"transporteId":1,"observaciones":""}}'
# el worker lo sincroniza; consulta el estado con GET /api/orders
```

## Backend Rust (`backend/`)
Workspace de Cargo: `common` (pool, migraciones, logging), `cloud-api`, `worker`, `images`. Las consultas son `sqlx` en tiempo de ejecución, así que el respaldo son las pruebas de integración contra Postgres real (`#[sqlx::test]`), no el chequeo en compilación.
```
cd backend
export DATABASE_URL=postgres://usuario:clave@localhost:5432/postgres   # sqlx::test crea bases temporales
cargo test --workspace
```
Variables de `cloud-api`: `DATABASE_URL`, `PORT` (Railway la inyecta), `RUN_MIGRATIONS=false` para no migrar, `SEED_DEV=true` para datos ficticios, `INTERNAL_API_URL` + `INTERNAL_API_KEY` (precios y crédito vía la API interna de MR), `MR_MODE=fake` para simularlos en desarrollo (sin `INTERNAL_API_URL` y sin `SEED_DEV`/`MR_MODE=fake` no arranca, para no servir precios falsos por accidente), `PRICE_TTL_SECS` (caché de `precio_cliente`, 900), `SESSION_HOURS` (24), `CORS_ORIGINS` (lista separada por comas). El worker: `INTERNAL_API_URL`, `INTERNAL_API_KEY`, `POLL_SECONDS`.

API de `cloud-api` (JSON camelCase, todo salvo `/health` y `/auth/login` pide `Authorization: Bearer <token>`; errores `{detail, code}`):
`POST /auth/login` · `POST /auth/logout` · `GET /me` · `GET /me/credit` · `GET /stands[/{id}]` · `GET /products?standId&texto&estado` · `GET /products/{id}` · `GET /promotions?standId` · `GET /cart` · `PUT /cart/items/{id}` `{cantidad}` · `PUT /cart/items/{id}/confirm` `{confirmado}` · `PUT /cart/confirm` `{confirmado}` · `POST /orders` `{datos}` (convierte el carrito confirmado en pedido) · `GET /orders` · `GET /orders/{id}`.
El precio sale de `precio_cliente` (caché con TTL de `POST /prices` de mr-api) y se revalida contra MR al colocar el pedido; el crédito es `GET /clients/{id}/credit` menos los pedidos aún no enviados a MR.
CI en `.github/workflows/backend.yml` (también corre cada mes). En Railway se despliega un servicio por binario con `BIN=cloud-api` o `BIN=worker` y root en `app/backend`.

## Estado
Hecho: esquema, flujo pedido → outbox → API interna idempotente, reintentos, tema por tokens.
Hecho también: autenticación, stands, catálogo y búsqueda por cliente, promociones, carrito, crédito y pedidos en `cloud-api`; cliente HTTP de `ExpoApi` en el frontend (`VITE_API_MODE=http`).
Pendiente: importar el snapshot de Indigo (`/catalog`, `/clients`: hoy las tablas `producto`, `cliente`, `direccion`, `transporte`, `uso_cfdi` solo se llenan con el seed), alta de usuarios y contraseñas reales, límite de intentos de login, módulos de monedas y ruleta, promociones por producto o con tope, imágenes, surtido, despliegue en Railway.
Nota: `docker-compose.yml`, los Dockerfiles y `lb/nginx.conf` no se han ejecutado todavía (sin daemon de Docker donde se creó); `cloud-api`, `worker` e `images` sí se probaron de forma nativa. `internal-api` sigue siendo la simulación en Python.
