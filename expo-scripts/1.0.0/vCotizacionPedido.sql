Use[ExpoMR]
GO
IF exists(Select * From sysobjects Where name= 'vCotizacionPedido')
Begin
	drop view vCotizacionPedido
End
GO
CREATE VIEW [vCotizacionPedido] AS

Select Todo.Cliente, Todo.IdProducto, Todo.PrecioFinal, Todo.Cantidad,
CAST(Todo.PrecioFinal * Todo.Cantidad as decimal(18,2)) as Total,
mProductos.IdLaboratorio
From (	Select 
		ISNULL(mPedidos.Cliente, mCotizaciones.Cliente) as Cliente,
		ISNULL(mPedidos.IdProducto, mCotizaciones.IdProducto) as IdProducto, 
		CAST(ISNULL(mPedidos.Unitario - mPedidos.Descuento, mCotizaciones.Unitario - mCotizaciones.Descuento) as decimal(18, 2)) as PrecioFinal,
		ISNULL(mPedidos.Cantidad, 0) + ISNULL(mCotizaciones.Cantidad, 0) as Cantidad 
		From (	Select mCotizaciones.Cliente, mCotizaciones.IdProducto, 
				mCotizaciones.Cantidad, mCotizaciones.Unitario, mCotizaciones.Descuento
				From mCotizaciones
				Where mCotizaciones.Solicitado = 1) as mCotizaciones full outer join
		mPedidos on mCotizaciones.Cliente = mPedidos.Cliente And mCotizaciones.IdProducto = mPedidos.IdProducto) as Todo  inner join
mProductos on Todo.IdProducto = mProductos.IdProducto
