USE [ExpoMR]
GO
/****** Object:  UserDefinedFunction [dbo].[GetInvSucursales]    Script Date: 13/11/2025 10:37:39 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 13 Noviembre 2025
-- Description:	Regresa un 1 o 0 si hay inventario de un producto en las sucursales sin importar la condicion
-- =============================================
CREATE OR ALTER FUNCTION [dbo].[GetInvSucursales] 
(
	-- Add the parameters for the function here
	@IdProducto SMALLINT
)
RETURNS BIT
AS
BEGIN
	-- Declare the return variable here
	DECLARE @Result BIT = 0

	-- Add the T-SQL statements to compute the return value here
	IF(SELECT SUM(InvSucursales.Cantidad) AS Inv
		FROM (
				SELECT IdProducto, Cantidad
				FROM Indigo.dbo.mProdVenta
				WHERE IdProducto = @IdProducto
				AND IdCondicion = 1 --1 = Normal
				UNION
				SELECT IdProducto, Cantidad
				FROM QtroIndigo.dbo.mProdVenta
				WHERE IdProducto = @IdProducto
				AND IdCondicion = 1 --1 = Normal
				UNION
				SELECT IdProducto, Cantidad
				FROM EdomxIndigo.dbo.mProdVenta
				WHERE IdProducto = @IdProducto
				AND IdCondicion = 1 --1 = Normal
			) AS InvSucursales
		GROUP BY InvSucursales.IdProducto) > 0
	BEGIN
		SET @Result = 1
	END

	-- Return the result of the function
	RETURN @Result

END
