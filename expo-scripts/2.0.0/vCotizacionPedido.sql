USE [ExpoMR]
GO

/****** Object:  View [dbo].[vCotizacionPedido]    Script Date: 06/21/2021 12:55:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[vCotizacionPedido] AS

SELECT Todo.Cliente, Todo.IdProducto, Todo.PrecioFinal, Todo.Cantidad, 
CAST(Todo.PrecioFinal * Todo.Cantidad AS DECIMAL(18,2)) AS Total, 
mProductos.IdLaboratorio
FROM (	
		SELECT ISNULL(mPedidos.Cliente, mCotizaciones.Cliente) AS Cliente,
		ISNULL(dPedidos.IdProducto, mCotizaciones.IdProducto) AS IdProducto, 
		CAST(ISNULL(dPedidos.Unitario - dPedidos.Descuento, mCotizaciones.Unitario - mCotizaciones.Descuento) AS DECIMAL(18, 2)) AS PrecioFinal,
		ISNULL(SUM(dPedidos.Cantidad), 0) + ISNULL(mCotizaciones.Cantidad, 0) AS Cantidad 
		FROM (	
				SELECT mCotizaciones.Cliente, dCotizaciones.IdProducto, 
				dCotizaciones.Cantidad, dCotizaciones.Unitario, dCotizaciones.Descuento
				FROM mCotizaciones INNER JOIN
				dCotizaciones ON mCotizaciones.IdCotizacion = dCotizaciones.IdCotizacion
				WHERE dCotizaciones.Solicitado = 1
			  ) AS mCotizaciones FULL OUTER JOIN
		mPedidos ON mCotizaciones.Cliente = mPedidos.Cliente FULL OUTER JOIN
		dPedidos ON mPedidos.IdPedido = dPedidos.IdPedido AND mCotizaciones.IdProducto = dPedidos.IdProducto
		GROUP BY mPedidos.Cliente, mCotizaciones.Cliente, dPedidos.IdProducto, mCotizaciones.IdProducto, dPedidos.Unitario, dPedidos.Descuento,
		mCotizaciones.Unitario, mCotizaciones.Descuento, mCotizaciones.Cantidad
	  ) AS Todo INNER JOIN
mProductos ON Todo.IdProducto = mProductos.IdProducto

GO


