USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[ConfirmarProducto]    Script Date: 22/03/2024 02:22:10 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:	Josue, Carlos
-- Create date: 06 Mayo 2024
-- Description:	Cambia el bit de confirmar del producto de una cotizacion de cliente
--1 = Confirma todos los productos
--2 = Desconfirma todos los productos
--3 = Confirma o Desconfirma un producto especifico
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[ConfirmarProducto]
	@IdProducto SMALLINT,
	@Cliente INT,
	@Usuario INT,
	@Confirmado TINYINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @idcotizacion INT	EXECUTE @idcotizacion = CotizarGetCotizacion @cliente, @usuario

	UPDATE dCotizaciones	SET dCotizaciones.PorConfirmar = (CASE WHEN @Confirmado = 1 THEN 1										   WHEN @Confirmado = 2 THEN 0										   ELSE (CASE WHEN PorConfirmar = 1 THEN 0 										              ELSE 1 END)									  END),		dCotizaciones.Fecha = GETDATE()	WHERE dCotizaciones.IdCotizacion = @idcotizacion	AND dCotizaciones.IdProducto = @IdProducto
END