USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[PedidoEditarCantidadProducto]    Script Date: 18/04/2024 02:30:52 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jaime Cortes
-- Create date: 24 Abr 2024
-- Description:	modifica un producto en dPedidos
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[PedidoEditarCantidadProducto]
	@IdPedido		INT,
	@Cantidad		INT,
	@IdProducto		INT
	
AS
BEGIN

	SET NOCOUNT ON;
	
	UPDATE dPedidos
	SET Cantidad = Surtido + @Cantidad
	WHERE IdPedido = @IdPedido
	AND IdProducto = @IdProducto
END