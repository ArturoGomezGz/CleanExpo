Use [ExpoMR]
GO
If exists(select * from sysobjects Where name = 'SurtirSetCredito')
	drop procedure SurtirSetCredito
GO
-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Rodrigo Cavero
-- Create date: 23 oct 2020
-- Description:	Limpia el campo de credito y marca quienes credito
-- =============================================
CREATE PROCEDURE SurtirSetCredito 
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	BEGIN TRANSACTION 
		Update mClientes
		Set mClientes.CreditoExcedido = null

		Update mClientes
		Set mClientes.CreditoExcedido = Case When vCredito.Credito + vCredito.CreditoInyectado > 0 Then 0 Else 1 End
		From Indigo.dbo.vCredito 
		Where mClientes.IdPersona = vCredito.IdPersona
		and mClientes.Sucursal = 1

		Update mClientes
		Set mClientes.CreditoExcedido = Case When vCredito.Credito + vCredito.CreditoInyectado > 0 Then 0 Else 1 End
		From QtroIndigo.dbo.vCredito 
		Where mClientes.IdPersona = vCredito.IdPersona
		and mClientes.Sucursal = 2

		Update mClientes
		Set mClientes.CreditoExcedido = Case When vCredito.Credito + vCredito.CreditoInyectado > 0 Then 0 Else 1 End
		From EdomxIndigo.dbo.vCredito 
		Where mClientes.IdPersona = vCredito.IdPersona
		and mClientes.Sucursal = 3

		Insert lCredito (Cliente, SurtidoParcial)
		Select mClientes.IdPersona, 0
		From mClientes
		Where mClientes.CreditoExcedido = 1
	COMMIT
END
GO
