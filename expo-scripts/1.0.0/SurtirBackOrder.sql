Use [ExpoMR]
GO
if exists(select * from sysobjects where name = 'SurtirBackOrder')
	drop procedure SurtirBackOrder
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
-- Create date: 24 Oct 2020
-- Description:	
-- =============================================
CREATE PROCEDURE SurtirBackOrder 
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Update mPedidos
	Set mPedidos.Surtido = Surtido.Surtido
	From (	Select mFacturas.IdPersona, dFacturas.IdProducto, SUM(dFacturas.ConCargo) as Surtido
			From mFacturas Inner Join 
			dFacturas on mFacturas.Sucursal = dFacturas.Sucursal And mFacturas.IdFactura = dFacturas.IdFactura
			Group by mFacturas.IdPersona, dFacturas.IdProducto) as Surtido
	Where mPedidos.Cliente = Surtido.IdPersona 
	And mPedidos.IdProducto = Surtido.IdProducto



END
GO
