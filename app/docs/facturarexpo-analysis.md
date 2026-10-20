# Clean Expo — Análisis de `FacturarExpo` y decisión para `POST /orders`

_Fuente: `expo-scripts/Indigo/` (FacturarExpo, vCredito, GetIva, GetPrecioExpo) y `expo-scripts/7.0.0` (versión más reciente de ExpoMR)._

## Qué hace `Indigo.dbo.FacturarExpo(@IdPedido)`
Corre **dentro de Indigo** (una copia por sucursal: Indigo, QtroIndigo, EdomxIndigo) y **lee y escribe tablas de ExpoMR**:

| Acción | Tablas |
|---|---|
| Lee el pedido | `ExpoMR.mPedidos`, `ExpoMR.dPedidos`, `ExpoMR.mProductos` (solo `Iva`) |
| Crea facturas (corte por `FacturarRows` renglones) con solo lo que alcanza en inventario: `ConCargo = min(pendiente, existencia)`, `Demanda = pedido` | `Indigo.mFacturas`, `dFacturas`, `dFacturasCostoLocal` |
| Mueve inventario y kardex | `mProdVenta` (−), `mProdAsignado`, `KardexInsertar`, `mSurtido` (envía a almacén) |
| Devuelve resultado a ExpoMR | `ExpoMR.mFacturas`, `ExpoMR.dFacturas`, y `dPedidos.Surtido += ConCargo`, `Colocado = 1` |
| Consume crédito temporal (cursor sobre `mCredito`, parte/ajusta registros, crea uno de 1 día si falta) | `Indigo.mCredito` |

Consecuencias:
- **Surtido parcial / back order**: lo no surtido queda `Colocado=0` y el proceso `Surtir` lo reintenta (`SurtirBackOrder` → `SurtirSetCredito` → `SurtirSurtido` → `SurtirFacturas{Gdl,Qtro,Edomx}`).
- Hay **ida y vuelta de datos** con ExpoMR: Indigo no puede facturar si el pedido no existe en `ExpoMR.dbo.mPedidos/dPedidos`.
- `vCredito`: crédito = límite − saldos abiertos (facturas, notas de cargo, notas de cliente), 0 si el cliente tiene `Status>0`; `CreditoInyectado` = suma de `mCredito` vigente sin pedido. `GetIva(0)` = IVA/100 y `GetIva(1)` = IVA en %, ambos de `msysvar`.
- `GetPrecioExpo(cliente, producto)` solo envuelve `GetMultiPrecioExpo(TTCotizar)`, **que no está en los scripts** (hace falta para el export de precios).

## Cambios en 7.0.0 que afectan el diseño
- `ExpoPedidoAgregar` (2025) ya **no asigna monedas ni promociones** (bloques comentados) y recibe `@idemail` en lugar de `@usuario`. Tras facturar, recalcula `Habilitar` solo para productos en liquidación (`Status=4`) con `GetInvSucursales`.
- VIP se elimina (`Delete ChangeVIP/GetVIP`, `ValidaExpoVIP 1`, columna `VIP` borrada). **No hay que migrar VIP.**
- ⚠️ A confirmar con MR: ¿monedas, promociones y ruleta siguen activas en la Expo actual? Si no, no entran al alcance.

## Decisión recomendada (fase 1)
**Mantener un "puente ExpoMR" mínimo en SQL Server dentro de la red de MR** y no tocar `FacturarExpo` ni el ERP:

1. La nube guarda el pedido en Postgres (idempotency key = `orderId`) y el worker lo envía a `POST /orders` de la API interna.
2. La API interna, en una transacción, escribe el pedido en `ExpoMR.mPedidos/dPedidos` (esquema reducido: pedido, renglones, `mProductos.Iva`, `mFacturas`, `dFacturas`) y ejecuta `FacturarExpo` en la sucursal del cliente (config de sucursales en la API, no IFs en SQL).
3. La API lee `dPedidos.Surtido/Colocado` y `ExpoMR.dFacturas` y los publica en `GET /orders/{id}/status` para que la nube muestre estado y surtido.
4. `Surtir` pasa a ser un job de la API interna (mismos pasos, con log y reintentos).

Ventajas: cero riesgo en facturación/inventario/crédito, y la responsabilidad de negocio (carrito, descuentos, reglas) sale de los SPs. Costo: ExpoMR sigue existiendo, pero reducido a una base puente.

**Fase 2 (opcional):** reescribir `FacturarExpo` para recibir el pedido como tabla (TVP) y eliminar el puente; implica cambiar el ERP en 3 bases.

## Endpoints internos resultantes
`GET /catalog` · `GET /prices` · `GET /clients/{id}` · `GET /clients/{id}/credit` (vCredito + crédito ExpoMR − back order) · `GET /inventory/{producto}` (liquidación) · `POST /orders` · `GET /orders/{id}/status` · `POST /fulfillment/run`.

## Pendiente
`GetMultiPrecioExpo` y su tipo `TTCotizar`; confirmar que QtroIndigo/EdomxIndigo tienen la misma versión de `FacturarExpo`/`vCredito`; esquema de `ExpoMR.mFacturas/dFacturas`; y si monedas/promociones/ruleta siguen vigentes.
