Use[ExpoMR]
GO
IF exists(Select * From sysobjects Where name= 'vCotizaciones')
Begin
	drop view vCotizaciones
End
GO
CREATE VIEW [vCotizaciones] AS

Select mCotizaciones.Cliente, mCotizaciones.IdProducto, mCotizaciones.Unitario,
mCotizaciones.Descuento, mCotizaciones.Cantidad, mCotizaciones.Solicitado, mCotizaciones.PorConfirmar,   
mCotizaciones.Usuario, mCotizaciones.Fecha,
--precio final sin iva
CAST(mCotizaciones.Unitario - mCotizaciones.Descuento as decimal(18, 2)) as PrecioFinal,
CAST((mCotizaciones.Unitario - mCotizaciones.Descuento) * mCotizaciones.Cantidad as decimal(18, 2)) as Total,  
--venta neta del proveedor
CAST(mCotizaciones.Cantidad * (mProductos.Costo - mCotizaciones.Descuento) as decimal(18, 2)) as VentaProv
From mCotizaciones inner join
mProductos on mCotizaciones.IdProducto = mProductos.IdProducto

GO