Use [ExpoMR]
GO

if exists (select * from sysobjects where name='vSurtidoCredito')
	drop view vSurtidoCredito
GO

CREATE VIEW vSurtidoCredito AS
Select Surtido.IdPedido,
Case When Surtido.MontoAcumulado > Surtido.Credito 
	Then CAST(Surtido.Cantidad - ((Surtido.MontoAcumulado - Surtido.Credito) / Surtido.Precio) as int)
	Else Surtido.Cantidad End as Surtir
From (
	Select mSurtido.IdPedido, mSurtido.Cantidad, mSurtido.Precio,
		vCredito.Credito + vCredito.CreditoInyectado as Credito,
		(Select SUM(SurtidoAcumulado.Precio * SurtidoAcumulado.Cantidad) 
		From mSurtido as SurtidoAcumulado
		Where SurtidoAcumulado.Cliente = mSurtido.Cliente
		And SurtidoAcumulado.IdPedido < = mSurtido.IdPedido) as MontoAcumulado
	From mSurtido Inner Join
	Indigo.dbo.vCredito on mSurtido.Cliente = vCredito.IdPersona
	Where mSurtido.Sucursal = 1
	) as Surtido
Union
Select Surtido.IdPedido,
Case When Surtido.MontoAcumulado > Surtido.Credito 
	Then CAST(Surtido.Cantidad - ((Surtido.MontoAcumulado - Surtido.Credito) / Surtido.Precio) as int)
	Else Surtido.Cantidad End as Surtir
From (
	Select mSurtido.IdPedido, mSurtido.Cantidad, mSurtido.Precio,
		vCredito.Credito + vCredito.CreditoInyectado as Credito,
		(Select SUM(SurtidoAcumulado.Precio * SurtidoAcumulado.Cantidad) 
		From mSurtido as SurtidoAcumulado
		Where SurtidoAcumulado.Cliente = mSurtido.Cliente
		And SurtidoAcumulado.IdPedido < = mSurtido.IdPedido) as MontoAcumulado
	From mSurtido Inner Join
	QtroIndigo.dbo.vCredito on mSurtido.Cliente = vCredito.IdPersona
	Where mSurtido.Sucursal = 2
	) as Surtido
Union
Select Surtido.IdPedido,
Case When Surtido.MontoAcumulado > Surtido.Credito 
	Then CAST(Surtido.Cantidad - ((Surtido.MontoAcumulado - Surtido.Credito) / Surtido.Precio) as int)
	Else Surtido.Cantidad End as Surtir
From (
	Select mSurtido.IdPedido, mSurtido.Cantidad, mSurtido.Precio,
		vCredito.Credito + vCredito.CreditoInyectado as Credito,
		(Select SUM(SurtidoAcumulado.Precio * SurtidoAcumulado.Cantidad) 
		From mSurtido as SurtidoAcumulado
		Where SurtidoAcumulado.Cliente = mSurtido.Cliente
		And SurtidoAcumulado.IdPedido < = mSurtido.IdPedido) as MontoAcumulado
	From mSurtido Inner Join
	EdomxIndigo.dbo.vCredito on mSurtido.Cliente = vCredito.IdPersona
	Where mSurtido.Sucursal = 3
	) as Surtido
