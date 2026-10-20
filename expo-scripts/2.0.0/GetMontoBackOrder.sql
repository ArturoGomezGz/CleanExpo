USE [ExpoMR]
GO
/****** Object:  UserDefinedFunction [dbo].[GetMontoBackOrder]    Script Date: 08/24/2021 15:12:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[GetMontoBackOrder] 
(
	-- Add the parameters for the function here
	@idpersona INT
)
RETURNS REAL
AS
BEGIN
	DECLARE @credito REAL

	SET @credito = (SELECT SUM(CAST((dPedidos.Unitario - dPedidos.Descuento) * 
					CASE WHEN mProductos.Iva = 1 THEN (1 + Indigo.dbo.GetIva(0)) ELSE 1 END AS DECIMAL(18, 2)) * (dPedidos.Cantidad - dPedidos.Surtido)) AS TotalConIva
					FROM mPedidos INNER JOIN
					dPedidos ON mPedidos.IdPedido = dPedidos.IdPedido INNER JOIN
					mProductos ON dPedidos.IdProducto = mProductos.IdProducto
					WHERE mPedidos.Cliente = @idpersona)
	RETURN @credito
END
