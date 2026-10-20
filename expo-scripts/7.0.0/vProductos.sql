USE [ExpoMR]
GO

/****** Object:  View [dbo].[vProductos]    Script Date: 13/11/2025 11:19:23 a. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER VIEW [dbo].[vProductos] AS

Select mProductos.IdProducto, mProductos.IdLaboratorio, mProductos.Iva, 
mProductos.Rank, mProductos.Descuento, mProdEmp.Costo,
vProdNombre.Nombre, vProdNombre.Presentacion, vProdNombre.Formula, vProdNombre.Codigo,
vProdNombre.Nombre + ' ' + vProdNombre.Presentacion as Producto, mProductos.Habilitar
From mProductos inner join
Indigo.dbo.vProdNombre on mProductos.IdProducto = vProdNombre.IdProducto inner join
Indigo.dbo.mProdEmp on mProductos.IdProducto = mProdEmp.IdProducto
GO


