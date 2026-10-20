USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[CotizarEditarCliente]    Script Date: 18/04/2024 02:30:52 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jaime Cortes
-- Create date: 24 Abr 2024
-- Description:	Edita los datos del cliente de una cotizacion
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[CotizarEditarCliente]
	@IdCotizacion		INT,
	@IdDocumento		INT,
	@IdUsoCFDI			INT,
	@IdTipoVenta		INT,
	@IdTransporte		INT,
	@IdPorte			INT,
	@IdEntrega			INT,
	@Obsv				VARCHAR(100)
	
AS
BEGIN

	SET NOCOUNT ON;
	
	UPDATE mCotizaciones
	SET IdDocumento = @IdDocumento,
		IdUsoCFDI = CASE WHEN @IdDocumento = 1 THEN @IdUsoCFDI ELSE 23 END,-- 23 S01 Sin Efectos Fiscales
		IdTipoVenta= @IdTipoVenta,
		IdTransporte = CASE WHEN @IdTransporte = 0 THEN NULL ELSE @IdTransporte END,
		IdPorte = CASE WHEN @IdPorte = 0 THEN NULL ELSE @IdPorte END,
		Entrega = @IdEntrega,
		Obsv = @Obsv
	WHERE IdCotizacion = @IdCotizacion
END