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
| `postgres` | Base de la nube (`db/init`: modelo núcleo, módulos opcionales, datos de desarrollo) |
| `cloud-api` | Recibe pedidos, calcula precios solo en servidor, guarda pedido + outbox |
| `worker` | Envía pedidos pendientes a la API interna con `Idempotency-Key` y reintentos con backoff |
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

## Estado
Hecho: esquema, flujo pedido → outbox → API interna idempotente, reintentos, tema por tokens.
Pendiente: catálogo/precios/crédito contra Indigo (`/catalog`, `/prices`, `/clients`), autenticación, carrito, módulos de monedas/promociones/ruleta, imágenes, surtido, CI, despliegue en Railway.
Nota: `docker-compose.yml` y `lb/nginx.conf` no se ejecutaron en el entorno donde se creó (sin Docker); los servicios Python y el esquema sí se probaron de forma nativa.
