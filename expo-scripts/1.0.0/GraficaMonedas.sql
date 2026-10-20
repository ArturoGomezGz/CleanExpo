USE [ExpoMR]
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE name = 'GraficaMonedas')
DROP PROCEDURE GraficaMonedas
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
-- Create date: 22 Sept 2020
-- Description:	Devuelve los datos para la grafica de monedas entregadas
-- =============================================
CREATE PROCEDURE GraficaMonedas 
	-- Add the parameters for the stored procedure here
	@proveedor int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
		--Datos para la grafica
	SELECT CONVERT(date, mTransacciones.Fecha, 23) as Fecha, ISNULL(SUM(mTransacciones.Monedas), 0) as Monedas
	FROM mTransacciones 
	WHERE mTransacciones.Origen = @proveedor
	Group by CONVERT(date, mTransacciones.Fecha, 23) 
END
GO
