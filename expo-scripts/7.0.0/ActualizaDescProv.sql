USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[ActualizaDescProv]    Script Date: 20/10/2025 01:31:03 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 02 Abril 2024
-- Description:	Actualiza el % de descuento dado por el proveedor de un producto
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[ActualizaDescProv] 
	-- Add the parameters for the stored procedure here
	@IdProducto INT,
	@Descuento DECIMAL(9,2),
	@IdEmail INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT lProvDescCliente (IdProducto, Descuento, IdEmail, FeModifica)
	SELECT mProductos.IdProducto, mProductos.Descuento, @IdEmail, GETDATE()
	FROM mProductos
	WHERE mProductos.IdProducto = @IdProducto

	UPDATE mProductos
	SET Descuento = @Descuento
	WHERE IdProducto = @IdProducto

END