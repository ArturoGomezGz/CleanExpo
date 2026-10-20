Use [ExpoMR]
Go
If exists (Select * From sysobjects Where name = 'vProductos')
drop view vProductos

GO

CREATE VIEW vProductos AS

Select mProductos.IdProducto, mProductos.IdLaboratorio, mProductos.Iva, 
mProductos.Rank, mProductos.Descuento, mProductos.Costo,
vProdNombre.Nombre, vProdNombre.Presentacion, vProdNombre.Formula, vProdNombre.Codigo,
vProdNombre.Nombre + ' ' + vProdNombre.Presentacion as Producto
From mProductos inner join
Indigo.dbo.vProdNombre on mProductos.IdProducto = vProdNombre.IdProducto 
