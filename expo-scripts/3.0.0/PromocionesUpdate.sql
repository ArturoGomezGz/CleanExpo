USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[PromocionesUpdate]    Script Date: 28/02/2022 02:00:14 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jorge Carrazco
-- Create date: 28 FEB 2022
-- Description: Inserta una nueva promocion
-- =============================================
CREATE PROCEDURE [dbo].[PromocionesUpdate] 
	@Proveedor INT,
	@Usuario INT,
	@IdPromocion SMALLINT,
	@IdTipoPromocion TINYINT,
	@Cliente INT,
	@Idproducto INT,
	@Monto REAL,
	@Piezas SMALLINT,
	@Updated BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @Obtenida BIT = 0

	SET @Updated = 0
	
	IF EXISTS (SELECT dPromoCliente.IdPromocion 
			   FROM dPromoCliente
			   WHERE dPromoCliente.IdPromocion = @IdPromocion)
	BEGIN
		SET @Obtenida = 1
	END
	
	SET @Cliente = CASE WHEN @Cliente = 0 THEN NULL 
						ELSE @Cliente END

	SET @Idproducto = CASE WHEN @Idproducto = 0 THEN NULL 
						   ELSE @Idproducto END

	SET @Monto = CASE WHEN @Monto = 0 THEN NULL 
					  ELSE @Monto END

	SET @Piezas = CASE WHEN @Piezas = 0 THEN NULL 
					   ELSE @Piezas END
	
	IF @Obtenida = 0
		AND	NOT EXISTS (SELECT mPromociones.IdPromocion 
						FROM mPromociones INNER JOIN
						dPromoCondicion ON mPromociones.IdPromocion = dPromoCondicion.IdPromocion
						WHERE mPromociones.IdPromocion = @IdPromocion
						AND mPromociones.IdTipoPromocion = @IdTipoPromocion
						AND mPromociones.Cliente = @Cliente
						AND dPromoCondicion.Monto = @Monto
						AND dPromoCondicion.Piezas = @Piezas
						AND dPromoCondicion.IdProducto = @IdProducto)
	BEGIN
		UPDATE mPromociones
		SET IdTipoPromocion = @IdTipoPromocion,
			Cliente = @Cliente,
			Usuario = @Usuario,
			Fecha = GETDATE()
		WHERE mPromociones.IdPromocion = @IdPromocion

		UPDATE dPromoCondicion
		SET Monto = @Monto,
			Piezas = @Piezas,
			IdProducto = @Idproducto
		WHERE dPromoCondicion.IdPromocion = @IdPromocion

		SET @Updated = 1
	END

	SELECT @Updated
END