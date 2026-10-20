Use [ExpoMR]
GO

If exists(Select * From sysobjects Where name = 'SurtirSurtido')
	drop procedure SurtirSurtido
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
-- Create date: 24 Oct 2020
-- Description:	
-- =============================================
CREATE PROCEDURE SurtirSurtido 
	-- Add the parameters for the stored procedure here
	@vip bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	BEGIN TRANSACTION

		Delete mSurtido

		Insert mSurtido (IdPedido, Sucursal, Cliente, IdProducto, Precio, Cantidad, Factura)
		Select vSurtido.IdPedido, vSurtido.Sucursal, vSurtido.Cliente, vSurtido.IdProducto, 
		vSurtido.Precio, vSurtido.Cantidad, vSurtido.Factura
		From vSurtido
		Where vSurtido.VIP = @vip

		Update mSurtido
		Set mSurtido.Surtir = vSurtidoCredito.Surtir
		From vSurtidoCredito
		Where mSurtido.IdPedido = vSurtidoCredito.IdPedido
		And vSurtidoCredito.Surtir > 0

		Insert lCredito (Cliente, SurtidoParcial)
		Select mSurtido.Cliente, 1
		From mSurtido
		Where mSurtido.Surtir Is Null
		Group by mSurtido.Cliente

		Delete mSurtido
		Where mSurtido.Surtir Is Null

	COMMIT
END
GO
