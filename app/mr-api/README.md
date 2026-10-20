# mr-api — API interna en la red de MR (.NET 8)

Accede a Indigo/ExpoMR (SQL Server) y expone los endpoints que consume la nube. Sin estado: se pueden correr varias réplicas detrás del balanceador. Contrato general en `../docs/internal-api.openapi.yaml`.

## Endpoints implementados (solo lectura)
| Ruta | Fuente real |
|---|---|
| `GET /health`, `GET /health/ready` | ping a ExpoMR y a cada Indigo (`ready` da 503 si alguno falla) |
| `POST /prices` `{idPersona, productos[≤500]}` | `Indigo.dbo.GetMultiPrecioExpo` en la sucursal del cliente (`mClientes.Sucursal`) |
| `GET /clients/{id}/credit` | `vCredito` (sucursal) + `mClientes.Credito` − `GetMontoBackOrder` (fórmula de `ExpoMR.GetCredit`) |
| `GET /inventory/{idProducto}` | `mProdVenta` con `IdCondicion=1` en las 3 sucursales |

Pendientes siguientes: `/catalog`, `/clients/{id}`, `POST /orders` (puente ExpoMR + `FacturarExpo`), `/orders/{id}/status`, `/fulfillment/run`.

## Configuración (variables de entorno)
```
Api__ApiKey=...                      # header X-Api-Key; vacío = rechaza todo
ConnectionStrings__ExpoMR=Server=...;Database=ExpoMR;User ID=...;Password=...;TrustServerCertificate=True
Sucursales__1__Indigo=...            # GDL  (Database=Indigo)
Sucursales__2__Indigo=...            # QRO  (Database=QtroIndigo)
Sucursales__3__Indigo=...            # MEX  (Database=EdomxIndigo)
```
Usuario SQL dedicado con permisos mínimos (SELECT en las vistas/tablas usadas; sin Kerberos en Linux). Errores de SQL Server devuelven 503 para que el worker reintente.

## Desarrollo
```
dotnet test            # pruebas con repositorios falsos (no requieren SQL Server)
docker build -t mr-api .
```
Supuesto por validar contra un SQL Server real: `TTCotizar` acepta `INSERT (IdPersona, IdProducto)` (así lo usa `GetPrecioExpo`); la API no depende del resto de sus columnas.
