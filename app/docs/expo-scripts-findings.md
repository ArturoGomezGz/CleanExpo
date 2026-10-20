# Clean Expo — Hallazgos de `expo-scripts/` (67 scripts de ExpoMR)

_Fuente: `expo-scripts/` en el repo (archivos .sql, varios en UTF-16). Son sobre todo `ALTER PROCEDURE/VIEW` más algunos cambios de tabla/índice; **no hay `CREATE TABLE` de las tablas principales**._

## 1. Cómo se coloca un pedido hoy (`ExpoPedidoAgregar`)
En una sola transacción en ExpoMR:
1. `CotizarGetCotizacion` obtiene la cotización (carrito) del cliente.
2. Inserta `mPedidos` + `dPedidos` con los productos confirmados (`PorConfirmar=1`, `Solicitado=1`, `vProductos.Habilitar=1`).
3. **Factura en Indigo, síncrono**, según `mClientes.Sucursal`: 1=GDL → `Indigo.dbo.FacturarExpo`, 2=QRO → `QtroIndigo.dbo.FacturarExpo`, 3=MEX → `EdomxIndigo.dbo.FacturarExpo`.
4. Si el producto está en liquidación (`mProdEmp.Status=4`) recalcula `Habilitar` con inventario de las 3 sucursales (`GetInvSucursales` lee `mProdVenta` de las 3 bases Indigo).
5. Borra los renglones de la cotización; (monedas y promociones del pedido están comentadas en el SP).

→ Hay **3 bases Indigo (una por sucursal)**, no una. La API interna debe ejecutar el mismo flujo (o llamar `FacturarExpo`) en la sucursal correcta.

## 2. Validación previa (`ExpoPedidoValidar`)
Compara el total por confirmar contra `dbo.GetCredit(cliente,1)` → 0 sin productos, 1 sin crédito, 2 OK. **El crédito vive en Indigo**: hace falta consulta en vivo (o snapshot de límite/saldo con riesgo de desfase).

## 3. Catálogo ya funciona como "snapshot por laboratorio"
`InicializaLaboratorio` copia a `ExpoMR.mProductos` los productos del laboratorio desde `Indigo.mProductos` + `mProdEmp` con `Status IN (0 activo, 3 nuevo, 4 liquidación, 5 pre-nuevo)`. Esto respalda la hipótesis del corte de catálogo (confirmar con MR). Pero:
- `vProductos` une en vivo con `Indigo.vProdNombre` (nombre, presentación, fórmula, código) y `mProdEmp.Costo`.
- El precio se calcula al cotizar (`Indigo.dbo.GetPrecioExpo` / `GetMultiPrecioExpo`) y se guarda en `dCotizaciones.Unitario`.

## 4. Clientes y usuarios
`UsuariosAgregar` copia el cliente de Indigo a `ExpoMR.mClientes` (documento, uso CFDI, tipo de venta, direcciones fiscal/entrega, sucursal). La identidad es `IdEmail`/`IdPersona` de Indigo. → Snapshot de clientes se puede exportar antes de la expo; alta de usuarios nuevos durante la expo requiere consultar Indigo.

## 5. Proceso posterior: `Surtir`
Job por pasos (`lSurtido`): `SurtirBackOrder` → `SurtirSetCredito` → `SurtirSurtido` → `SurtirFacturasGdl/Qtro/Edomx` → `SurtirBackOrder`. Actualiza `dPedidos.Surtido/Colocado`. Es un proceso batch **interno** (se queda en la red de MR).

## 6. Dependencias que NO están en los scripts (hay que pedirlas)
`GetCredit`, `GetSysVar`, `GetIva`, `GetPrecioExpo`/`GetMultiPrecioExpo`, `MonedasAsignar`, `PromocionesAsignar`, `CotizarGetCotizacion`, `InicializaProductosComprados`, `SurtirSetCredito`, `SurtirFacturas*`, `Indigo.dbo.FacturarExpo`, y los `CREATE TABLE` de `mPedidos, dPedidos, mCotizaciones, dCotizaciones, mProductos, mClientes, mUsuarios`, etc.

## 7. Cambios al plan de frontera de datos
| Tema | Antes | Ahora |
|---|---|---|
| Colocar pedido | `POST /pedidos` genérico | API interna ejecuta insert + `FacturarExpo` por sucursal (1/2/3); idempotencia por id de pedido de la nube |
| Crédito | no contemplado | `GET /clientes/{id}/credito` en vivo (o snapshot) |
| Inventario | opcional | solo importa para liquidación (`Status 4`): `GetInvSucursales` en 3 bases |
| Catálogo | export único | export por laboratorio con los mismos filtros de `Status`, incluyendo nombre/presentación/costo y precios por cliente/lista |
| Surtido | no contemplado | se queda interno; la nube solo lee estado/surtido |
