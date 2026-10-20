USE [ExpoMR]
GO
If exists (select * From sysobjects Where name = 'vPromociones')
Begin
	drop view vPromociones
End
GO

CREATE VIEW	vPromociones AS
Select mPromociones.IdPromocion, mPromociones.Proveedor, mPromociones.Cliente, mPromociones.Monto,
mPromociones.NumPromos - COUNT(dPromoCliente.IdPromocion) as Restantes
From mPromociones left outer join
dPromoCliente on mPromociones.IdPromocion = dPromoCliente.IdPromocion
Where mPromociones.Activa = 1 
Group by mPromociones.IdPromocion, mPromociones.Proveedor, mPromociones.Cliente, mPromociones.Monto, mPromociones.NumPromos