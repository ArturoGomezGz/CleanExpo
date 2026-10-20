# Clean Expo — De stored procedures a API: qué responsabilidad se mueve y a dónde

> **Actualización:** esta versión se basó en 1.0.0–6.0.0. Ver [facturarexpo-analysis.md](facturarexpo-analysis.md): con 7.0.0 monedas/promociones salieron del flujo de pedido, VIP se eliminó, y el riesgo de la sección 4 se confirmó (FacturarExpo lee/escribe ExpoMR); la decisión recomendada es un puente ExpoMR mínimo en red interna.


_Base: `expo-scripts/` tomando la versión más reciente de cada objeto (carpetas 1.0.0 → 6.0.0, 204 objetos únicos; no existe 7.0.0 en el repo). Los scripts de Indigo (`FacturarExpo`, `vCredito`, `GetIva`, `GetPrecioExpo`, `vProdNombre`, etc.) **no están** en la carpeta; solo se conoce su uso._

## Principio
SQL Server (Indigo) se queda como **sistema de registro del ERP**: datos maestros, crédito, precios, inventario y facturación. Todo lo demás (reglas de la Expo) pasa a **código de la API** con Postgres como base propia.

## 1. Lógica que hoy vive en SPs/funciones y pasa a la API en la nube (Postgres)
| Regla hoy en SQL | Dónde está | Nuevo dueño |
|---|---|---|
| Carrito/cotización: crear, agregar, editar cantidad, quitar, confirmar/desconfirmar, carga masiva | `CotizarGetCotizacion`, `CotizarAgregar`, `CotizarAgregarMasivo`, `CotizarEditar*`, `CotizarEliminar*`, `ConfirmarProducto`, `CotizarActualizarDatos` | Módulo **Cart** de la API |
| Cálculo de descuento al cotizar: descuento general del producto % + descuento específico del cliente (`dProductosDescuento`) sobre el precio ya descontado | `CotizarAgregar` | Función pura de dominio (testeable) |
| Totales/IVA/subtotal del carrito y de pedidos | `vCotizacionPorConfirmar`, `vPedidos`, `GetTotales`, `GetTotalPedido` (usan `Indigo.dbo.GetIva(0)`) | Servicio de precios en API; la tasa de IVA se trae una vez de Indigo |
| Crear pedido a partir del carrito (copiar encabezado y renglones, limpiar carrito) | `ExpoPedidoAgregar` | **Orders** de la API (transacción en Postgres + outbox) |
| Monedas (lealtad): reglas por primer pedido, producto especial, volumen de venta; transacciones y saldo | `MonedasAsignar`, `GetMonedas`, `MonedasTransaccion`, `MonedasSaldo*`, cursor por producto especial | Módulo **Coins** con reglas en código |
| Promociones (escalonadas, acumulables, regalos, asignación al pedido y a stands) | `Promociones*`, `GetPromocionesEscalonadas`, `GetCantidadPromoAcumulable`, `GetDisponibilidadPromocion`, `GetArticulosRegalados` | Módulo **Promotions** |
| Ruleta y premios | `GetRuleta`, `mRuleta`, `GetPremiosRuleta`, `GetMonedasRuleta` | Módulo **Roulette** |
| Calendario de la expo, día válido, horas inicio/fin, VIP | `ValidaDiaExpo`, `ValidaExpoVIP`, `GetVIP*`, `mCalendario`, `GetSysVar`/`mSysVar` | Configuración de la API (tabla de settings) |
| Alta/baja de proveedores, labs, stands, artículos, anuncios, fotos | `AddProv`, `EditProv`, `AddArticuloProv`, `AddFotoArticuloProv`, … | Módulo **Stands** (+ almacenamiento de objetos para fotos) |
| Estadísticas, gráficas, bitácoras | `Estadistica*`, `Grafica*`, `log*`, `GetVisitasStandProv` | Consultas en Postgres / vistas materializadas |
| Reglas de visibilidad de producto (`Habilitar`) | `InicializaLaboratorio`, `ExpoPedidoAgregar` (recalcula en liquidación con inventario de 3 sucursales) | Regla en API; el inventario se consulta a Indigo solo para `Status=4` |

## 2. Reglas duplicadas que hay que centralizar
- **Ruteo por sucursal (1=GDL, 2=QRO, 3=MEX)** está copiado en `ExpoPedidoAgregar`, `GetCredit`, `GetInvSucursales`, `SurtirFacturasGdl/Qtro/Edomx`, `UsuariosAgregar`. → una sola tabla de configuración de sucursales (base/URL de cada Indigo) en la API interna.
- **Crédito disponible** (`ExpoMR.GetCredit`) = crédito de la sucursal en Indigo + `mClientes.Credito` − monto en back order. → la API calcula la fórmula; Indigo solo entrega el dato crudo (`vCredito`: crédito + crédito inyectado).

## 3. Lo que debe seguir en SQL Server / ERP (solo detrás de la API interna)
| Capacidad | Origen en Indigo | Endpoint interno propuesto |
|---|---|---|
| Catálogo y costos | `vProdNombre`, `mProdEmp` (Status 0/3/4/5, Costo), `mProductos`, `mLaboratorios` | `GET /catalog?laboratorio=` (export por lote) |
| Precio para el cliente | `GetPrecioExpo` / `GetMultiPrecioExpo` | `GET /prices?cliente=&productos=` (o incluido en el export) |
| Clientes y usuarios | `mPersonas`, `mPerEmp`, `mClientes`, `vNombres`, `mEmails`, `dDirecciones`, `mTipoVenta`, `mDocumentos`, `mSatUsoCFDI`, `mPortes` | `GET /clients/{id}`, `GET /clients/by-email` |
| Crédito | `vCredito`, `GetCredit` por sucursal | `GET /clients/{id}/credit` |
| Inventario (solo liquidación) | `mProdVenta` ×3 sucursales | `GET /inventory/{producto}` |
| **Facturar el pedido** | `FacturarExpo` ×3 sucursales | `POST /orders` (idempotente por `orderId` de la nube) |
| Surtido y facturas posteriores | `SurtirSurtido`, `SurtirFacturas*`, `mSurtido`, `vSurtidoCredito` | Job de orquestación en API interna (reemplaza el SP `Surtir` y sus cursores) |

## 4. Riesgo principal a resolver antes de diseñar `POST /orders`
`ExpoMR` e `Indigo` están en la misma instancia y `FacturarExpo(@IdPedido)` recibe solo el id del pedido, así que **casi seguro lee `ExpoMR.dbo.mPedidos/dPedidos` desde Indigo**. Si el pedido vive solo en Postgres, la API interna tendría que:
(a) escribir el pedido en una tabla de ExpoMR (staging) y luego llamar `FacturarExpo`, o
(b) reemplazar `FacturarExpo` por una versión que reciba el pedido completo.
Necesito el código de `FacturarExpo` (y `vCredito`, `GetIva`, `GetPrecioExpo`) para decidir.

## 5. Siguiente entregable propuesto
Modelo de datos Postgres (de `Tablas.sql` + versiones posteriores) y contrato OpenAPI de la API interna (6–8 endpoints de la sección 3).
