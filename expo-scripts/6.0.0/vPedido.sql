USE [ExpoMR]
GO

/****** Object:  View [dbo].[vPedido]    Script Date: 03/03/2025 12:32:41 p. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[vPedido] AS

SELECT mPedidos.Cliente, 
COUNT(dPedidos.IdProducto) AS Productos,
--subtotal
CAST(SUM(dPedidos.Cantidad * (dPedidos.Unitario - dPedidos.Descuento)) AS DECIMAL(18,2)) AS Subtotal, 
--iva
CAST(SUM(CASE WHEN mProductos.Iva = 1 THEN dPedidos.Cantidad * (dPedidos.Unitario - dPedidos.Descuento) * Indigo.dbo.GetIva(0) ELSE 0 END) AS DECIMAL(18,2))  AS IvaMonto,
--total cotizacion
CAST(SUM(dPedidos.Cantidad * (dPedidos.Unitario - dPedidos.Descuento)) +
	 SUM(CASE WHEN mProductos.Iva = 1 THEN dPedidos.Cantidad * (dPedidos.Unitario - dPedidos.Descuento) * Indigo.dbo.GetIva(0) ELSE 0 END) AS DECIMAL(18,2)) AS Total
FROM mPedidos WITH (INDEX(IXmPedidos)) INNER JOIN
dPedidos ON mPedidos.IdPedido = dPedidos.IdPedido INNER JOIN
mProductos WITH (INDEX(IXmProductos)) on dPedidos.IdProducto = mProductos.IdProducto 
GROUP BY mPedidos.Cliente
GO


