Use[ExpoMR]
GO
IF exists(Select * From sysobjects Where name= 'vPedidos')
Begin
	drop view vPedidos
End
GO

CREATE VIEW [vPedidos] AS

Select mPedidos.IdPedido, mPedidos.Cliente, mPedidos.IdProducto, mPedidos.Unitario,
mPedidos.Descuento, mPedidos.Cantidad, mPedidos.Usuario, mPedidos.Fecha,
Case When mPedidos.Cantidad > mPedidos.Surtido Then mPedidos.Cantidad - mPedidos.Surtido Else 0 End as PorSurtir,
--precio final sin iva
CAST(mPedidos.Unitario - mPedidos.Descuento as decimal(18, 2)) as PrecioFinal,
--precio final con iva
CAST(mPedidos.Unitario - mPedidos.Descuento * 
	CASE WHEN mProductos.Iva = 1 THEN 1 + Indigo.dbo.GetIva(0) ELSE 1 END as decimal(18, 2)) as PrecioIva,
--total
CAST((mPedidos.Unitario - mPedidos.Descuento) * mPedidos.Cantidad as decimal(18, 2)) as Total,
--venta neta del proveedor
CAST(mPedidos.Cantidad * (mProductos.Costo - mPedidos.Descuento) as decimal(18,2)) as VentaProv
From mPedidos inner join
mProductos on mPedidos.IdProducto = mProductos.IdProducto     

GO