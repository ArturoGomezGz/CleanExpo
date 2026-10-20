Use [Indigo]
GO

if exists (select * from sysobjects where name = 'vCredito')
	drop view vCredito

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW vCredito AS

Select mClientes.IdPersona, 
Case When mClientes.Status > 0 Then 0
	When mClientes.Credito <= ISNULL(Saldos.Saldo, 0) Then 0 
	Else mClientes.Credito - ISNULL(Saldos.Saldo, 0) End as Credito,
mClientes.Credito - ISNULL(Saldos.Saldo, 0) as CreditoNeto,
ISNULL(Creditos.Credito, 0) as CreditoInyectado
From mClientes left outer join
	(	Select SaldosDocs.IdPersona, SUM(SaldosDocs.Saldo) as Saldo  
		From (	Select vSaldoFacturas.IdPersona, vSaldoFacturas.Saldo
				From vSaldoFacturas
				Where vSaldoFacturas.FeCierre is null
				and vSaldoFacturas.Cancelada is null
				and vSaldoFacturas.Saldo > 0
				Union all
				Select vSaldoNotasCargo.IdPersona, vSaldoNotasCargo.Saldo
				From vSaldoNotasCargo
				Where vSaldoNotasCargo.FeCierre is null
				and vSaldoNotasCargo.Cancelada is null
				and vSaldoNotasCargo.Saldo > 0
				Union all
				Select vSaldoNotasCliente.IdPersona, - vSaldoNotasCliente.SaldoNota as Saldo
				From vSaldoNotasCliente
				Where vSaldoNotasCliente.FeCierre is null
				and vSaldoNotasCliente.Cancelada is null
				and vSaldoNotasCliente.SaldoNota > 0
				And vSaldoNotasCliente.NoDocumento is not null
			) as SaldosDocs
		Group by SaldosDocs.IdPersona
	) as Saldos	on mClientes.IdPersona = Saldos.IdPersona left outer join
	(	Select mCredito.IdPersona, SUM(mCredito.Monto) as Credito
		From mCredito
		Where mCredito.IdCotizacion is null
		And mCredito.Vencimiento >= GETDATE()
		Group By mCredito.IdPersona
	) as Creditos on mClientes.IdPersona = Creditos.IdPersona

GO