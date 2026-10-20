USE [ExpoMR]
GO

/****** Object:  View [dbo].[vPedidos]    Script Date: 08/12/2021 10:24:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[vPedidos] AS

SELECT mPedidos.IdPedido, mPedidos.Cliente, dPedidos.IdProducto, dPedidos.Unitario, 
dPedidos.Descuento, dPedidos.Cantidad, mPedidos.Usuario, mPedidos.Fecha,
CASE WHEN dPedidos.Cantidad > dPedidos.Surtido THEN dPedidos.Cantidad - dPedidos.Surtido ELSE 0 END AS PorSurtir,
--precio final sin iva
CAST(dPedidos.Unitario - dPedidos.Descuento AS DECIMAL(18, 2)) AS PrecioFinal,
--precio final con iva
CAST((dPedidos.Unitario - dPedidos.Descuento) * 
	CASE WHEN mProductos.Iva = 1 THEN (1 + Indigo.dbo.GetIva(0)) ELSE 1 END AS DECIMAL(18, 2)) AS PrecioIva,
--total
CAST((dPedidos.Unitario - dPedidos.Descuento) * dPedidos.Cantidad AS DECIMAL(18, 2)) AS Total,
--venta neta del proveedor
CAST(dPedidos.Cantidad * (mProductos.Costo - dPedidos.Descuento) AS DECIMAL(18,2)) AS VentaProv
FROM mPedidos INNER JOIN
dPedidos ON mPedidos.IdPedido = dPedidos.IdPedido INNER JOIN
mProductos ON dPedidos.IdProducto = mProductos.IdProducto

GO


