USE [ExpoMR]
GO

/****** Object:  View [dbo].[vCotizacionPorConfirmar]    Script Date: 03/03/2025 01:44:46 p. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[vCotizacionPorConfirmar] AS

SELECT mCotizaciones.Cliente, mCotizaciones.Usuario, 
COUNT(dCotizaciones.IdProducto) AS Productos,
--subtotal
CAST(SUM(dCotizaciones.Cantidad * (dCotizaciones.Unitario - dCotizaciones.Descuento)) AS DECIMAL(18,2)) AS Subtotal, 
--iva
CAST(SUM(CASE WHEN mProductos.Iva = 1 THEN dCotizaciones.Cantidad * (dCotizaciones.Unitario - dCotizaciones.Descuento) * Indigo.dbo.GetIva(0) ELSE 0 END) AS DECIMAL(18,2)) AS IvaMonto,
--total cotizacion
CAST(SUM(dCotizaciones.Cantidad * (dCotizaciones.Unitario - dCotizaciones.Descuento)) +
     SUM(CASE WHEN mProductos.Iva = 1 THEN dCotizaciones.Cantidad * (dCotizaciones.Unitario - dCotizaciones.Descuento) * Indigo.dbo.GetIva(0) ELSE 0 END) AS DECIMAL(18,2)) AS Total
FROM mCotizaciones WITH(INDEX(IXmCotizaciones)) INNER JOIN 
dCotizaciones ON mCotizaciones.IdCotizacion = dCotizaciones.IdCotizacion INNER JOIN
mProductos WITH(INDEX(IXmProductos)) ON dCotizaciones.IdProducto = mProductos.IdProducto
WHERE dCotizaciones.Solicitado = 1
And dCotizaciones.PorConfirmar = 1
GROUP BY mCotizaciones.Cliente, mCotizaciones.Usuario
GO


