USE [ExpoMR]
GO

/****** Object:  View [dbo].[vCotizaciones]    Script Date: 06/21/2021 12:51:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[vCotizaciones] AS

SELECT mCotizaciones.Cliente, dCotizaciones.IdProducto, dCotizaciones.Unitario, 
dCotizaciones.Descuento, dCotizaciones.Cantidad, dCotizaciones.Solicitado, dCotizaciones.PorConfirmar, 
mCotizaciones.Usuario, mCotizaciones.Fecha,
--precio final sin iva
CAST(dCotizaciones.Unitario - dCotizaciones.Descuento AS DECIMAL(18, 2)) AS PrecioFinal,
CAST((dCotizaciones.Unitario - dCotizaciones.Descuento) * dCotizaciones.Cantidad AS DECIMAL(18, 2)) AS Total,  
--venta neta del proveedor
CAST(dCotizaciones.Cantidad * (mProductos.Costo - dCotizaciones.Descuento) AS DECIMAL(18, 2)) AS VentaProv
FROM mCotizaciones INNER JOIN
dCotizaciones ON mCotizaciones.IdCotizacion = dCotizaciones.IdCotizacion INNER JOIN
mProductos ON dCotizaciones.IdProducto = mProductos.IdProducto

GO


