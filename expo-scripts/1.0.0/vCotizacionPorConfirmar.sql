USE [ExpoMR]
GO
IF exists(Select * From sysobjects Where name= 'vCotizacionPorConfirmar')
Begin
	drop view vCotizacionPorConfirmar
End
/****** Object:  View [dbo].[vCotizaciones]    Script Date: 28/08/2020 03:38:21 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vCotizacionPorConfirmar] AS
SELECT mCotizaciones.Cliente, 
COUNT(mCotizaciones.IdProducto) AS Productos,
--subtotal
CAST(SUM(mCotizaciones.Cantidad * (mCotizaciones.Unitario - mCotizaciones.Descuento)) as decimal(18,2)) AS Subtotal, 
--iva
CAST(SUM(CASE WHEN mProductos.Iva = 1 THEN mCotizaciones.Cantidad * (mCotizaciones.Unitario - mCotizaciones.Descuento) * Indigo.dbo.GetIva(0) ELSE 0 END) as decimal(18,2)) AS IvaMonto,
--total cotizacion
CAST(SUM(mCotizaciones.Cantidad * (mCotizaciones.Unitario - mCotizaciones.Descuento)) +
SUM(CASE WHEN mProductos.Iva = 1 THEN mCotizaciones.Cantidad * (mCotizaciones.Unitario - mCotizaciones.Descuento) * Indigo.dbo.GetIva(0) ELSE 0 END) as decimal(18,2)) AS Total
FROM mCotizaciones INNER JOIN 
mProductos ON mCotizaciones.IdProducto = mProductos.IdProducto
WHERE mCotizaciones.Solicitado = 1
And mCotizaciones.PorConfirmar = 1
GROUP BY mCotizaciones.Cliente

GO
