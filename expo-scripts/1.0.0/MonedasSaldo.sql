Use [ExpoMR] 
GO
If exists(Select * From sysobjects Where name = 'MonedasSaldo')
drop procedure MonedasSaldo
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
-- Create date: 15 Sep 2020
-- Description:	Regresa el saldo de la persona
-- =============================================
CREATE PROCEDURE MonedasSaldo 
	-- Add the parameters for the stored procedure here
	@idpersona int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Declare @saldo int = 0

	Select @saldo = mMonedero.Saldo  
	From mMonedero
	Where mMonedero.IdPersona = @idpersona

	return @saldo
END
GO
