USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[PromocionesValEditar]    Script Date: 08/08/2024 12:11:59 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ==================================================
-- Author:		Jorge Carrazco
-- Create date: 08 Ago 2024
-- Description:	Valida que se pueda editar la promocion del proveedor
-- ==================================================

CREATE OR ALTER PROCEDURE [dbo].[PromocionesValEditar] 
	@idpromocion SMALLINT,
	@editado BIT OUT
AS
BEGIN
	SET NOCOUNT ON;
	
	SET @editado = 0

	IF EXISTS (SELECT dPromoCliente.IdPromocion 
			   FROM dPromoCliente 
			   WHERE dPromoCliente.IdPromocion = @idpromocion)
	BEGIN
		SET @editado = 1
	END
END