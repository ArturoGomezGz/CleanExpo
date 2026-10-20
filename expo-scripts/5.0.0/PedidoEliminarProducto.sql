USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[PedidoEliminarProducto]    Script Date: 18/04/2024 02:30:52 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jaime Cortes
-- Create date: 24 Abr 2024
-- Description:	Elimina un producto en dPedidos
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[PedidoEliminarProducto]
	@IdPedido		INT,
	@IdProducto		INT
	
AS
BEGIN

	SET NOCOUNT ON;
	
	DELETE dPedidos
	WHERE IdPedido = @IdPedido
	AND IdProducto = @IdProducto
	AND Surtido = 0
END