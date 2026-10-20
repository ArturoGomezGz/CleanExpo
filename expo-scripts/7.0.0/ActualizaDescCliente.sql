USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[ActualizaDescCliente]    Script Date: 20/10/2025 01:31:03 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 20 octubre 2025
-- Description:	Actualiza el descuento proveedor de la cotizacion del cliente
-- =============================================

CREATE OR ALTER  PROCEDURE [dbo].[ActualizaDescCliente] 
	-- Add the parameters for the stored procedure here
	@IdProducto INT,
	@Cliente INT,
	@Descuento DECIMAL(9,2) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	DECLARE @precio REAL,
			@DescuentoProv REAL

	SELECT @precio = Indigo.dbo.GetPrecioExpo(@cliente, @idproducto)

	SELECT @DescuentoProv = CAST(@precio * mProductos.Descuento / 100.0 AS DECIMAL(9, 2)) + --Descuento General
	       CAST(((@precio - CAST(@precio * mProductos.Descuento / 100.0 AS DECIMAL(9, 2))) * --Precio del Cliente
		          @Descuento) / 100.0 AS DECIMAL(9,2))
    FROM mProductos
	WHERE mProductos.IdProducto =  @IdProducto

	UPDATE dCotizaciones
	SET Descuento = @DescuentoProv
	FROM mCotizaciones INNER JOIN 
	dCotizaciones ON mCotizaciones.IdCotizacion = dCotizaciones.IdCotizacion
	WHERE mCotizaciones.Cliente = @Cliente
	AND dCotizaciones.IdProducto = @IdProducto

END
