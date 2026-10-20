USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[CotizarEliminarProducto]    Script Date: 18/04/2024 02:30:52 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jaime Cortes
-- Create date: 24 Abr 2024
-- Description:	Asigna 0 en PorConfirmar y Solicitado en un producto de dCotizaciones
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[CotizarEliminarProducto]
	@cliente		INT,
	@usuario		INT,
	@IdProducto		INT
	
AS
BEGIN

	SET NOCOUNT ON;
	
	DECLARE @IdCotizacion INT

	EXEC @idcotizacion = dbo.CotizarGetCotizacion @cliente, @usuario

	UPDATE dCotizaciones
	SET Solicitado = 0,
		PorConfirmar = 0
	WHERE IdCotizacion = @idcotizacion
	AND IdProducto = @IdProducto
END