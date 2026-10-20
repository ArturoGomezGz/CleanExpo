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
curl -XPOST localhost:8000/api/orders -H 'content-type: application/json' \
  -d '{"id_persona":1001,"usuario_id":1,"items":[{"id_producto":1,"cantidad":3}]}'
# el worker lo sincroniza; consulta el estado:
curl localhost:8000/api/orders/<id>
```

## Backend Rust (`backend/`)
Workspace de Cargo: `common` (pool, migraciones, logging), `cloud-api`, `worker`, `images`. Las consultas son `sqlx` en tiempo de ejecución, así que el respaldo son las pruebas de integración contra Postgres real (`#[sqlx::test]`), no el chequeo en compilación.
```
cd backend
export DATABASE_URL=postgres://usuario:clave@localhost:5432/postgres   # sqlx::test crea bases temporales
cargo test --workspace
```
Variables: `DATABASE_URL`, `PORT` (Railway la inyecta), `RUN_MIGRATIONS=false` para no migrar, `SEED_DEV=true` para datos ficticios; el worker además `INTERNAL_API_URL`, `INTERNAL_API_KEY`, `POLL_SECONDS`.
CI en `.github/workflows/backend.yml` (también corre cada mes). En Railway se despliega un servicio por binario con `BIN=cloud-api` o `BIN=worker` y root en `app/backend`.

## Estado
Hecho: esquema, flujo pedido → outbox → API interna idempotente, reintentos, tema por tokens.
Pendiente: catálogo/precios/crédito contra Indigo (`/catalog`, `/prices`, `/clients`), autenticación, carrito, módulos de monedas/promociones/ruleta, imágenes, surtido, CI, despliegue en Railway.
Nota: `docker-compose.yml`, los Dockerfiles y `lb/nginx.conf` no se han ejecutado todavía (sin daemon de Docker donde se creó); `cloud-api`, `worker` e `images` sí se probaron de forma nativa. `internal-api` sigue siendo la simulación en Python.
