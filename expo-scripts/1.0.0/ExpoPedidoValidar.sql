USE [ExpoMR]
GO
If exists (select * From sysobjects Where name = 'ExpoPedidoValidar')
Begin
	drop procedure ExpoPedidoValidar
End
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
-- Create date: 20 Oct 2020
-- Description:	Valida si los productos seleccionados se pueden poner en firme
--				según su crédito
-- =============================================
CREATE PROCEDURE ExpoPedidoValidar 
	-- Add the parameters for the stored procedure here
	@cliente int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	IF Exists(	SELECT vCotizacionPorConfirmar.Cliente
				FROM vCotizacionPorConfirmar
				WHERE vCotizacionPorConfirmar.Cliente = @cliente)
	BEGIN
		If (SELECT vCotizacionPorConfirmar.Total + ISNULL(vPedido.Total, 0)
			FROM vCotizacionPorConfirmar LEFT OUTER JOIN
			vPedido ON vCotizacionPorConfirmar.Cliente = vPedido.Cliente
			WHERE vCotizacionPorConfirmar.Cliente = @cliente) <= dbo.GetCredit(@cliente, 1)
		Begin
			SELECT 2
		End
		Else
		Begin
			SELECT 1
		End
	END
	ELSE
	BEGIN
		SELECT 0
	END

END
GO
