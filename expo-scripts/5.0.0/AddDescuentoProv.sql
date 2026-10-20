USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[AddDescuentoProv]    Script Date: 13/03/2024 01:07:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 02 Abril 2024
-- Description:	Inserta el % de descuento dado por el proveedor de un producto a un cliente
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[AddDescuentoProv] 
	-- Add the parameters for the stored procedure here
	@IdProveedor INT,
	@IdProducto INT,
	@Descuento DECIMAL(9,2),
	@Cliente INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE dProductosDescuento
	SET Descuento = @Descuento,
		Usuario = @IdProveedor,
		Fecha = GETDATE()
	WHERE dProductosDescuento.Cliente = @Cliente
	AND dProductosDescuento.IdProducto = @IdProducto

	IF NOT EXISTS(SELECT dProductosDescuento.IdProducto
				  FROM dProductosDescuento
				  WHERE dProductosDescuento.Cliente = @Cliente
				  AND dProductosDescuento.IdProducto =  @IdProducto)
	BEGIN
		INSERT dProductosDescuento (IdProducto, Cliente, Descuento, Usuario, Fecha)
		VALUES (@IdProducto, @Cliente, @Descuento, @IdProveedor, GETDATE())
	END
END