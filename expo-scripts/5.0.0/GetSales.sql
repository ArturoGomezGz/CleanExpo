USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[GetSales]    Script Date: 13/03/2024 01:07:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 02 Abril 2024
-- Description:	Devuelve la sales solicitadas
-- =============================================

CREATE OR ALTER FUNCTION [dbo].[GetSales] (	@IdSal SMALLINT, @letra CHAR, @sal NVARCHAR(45))
RETURNS
	@Result TABLE(IdSal SMALLINT, 
				  Sal NVARCHAR(45), 
				  Habilitado BIT)

AS
BEGIN
    -- Insert statements for procedure here
	IF(@IdSal = 0)
	BEGIN
		INSERT @Result(IdSal, Sal, Habilitado)
		SELECT mSales.IdSal, mSales.Sal,		CAST(CASE WHEN COUNT(mProductos.IdProducto) = 0 THEN 0 ELSE 1 END AS BIT) AS Habilitado		FROM Indigo.dbo.mSales INNER JOIN		Indigo.dbo.dProdSal ON mSales.IdSal = dProdSal.IdSal LEFT OUTER JOIN		mProductos ON dProdSal.IdProducto = mProductos.IdProducto		WHERE UPPER(LEFT(mSales.Sal, 1)) = UPPER(@letra)		OR mSales.Sal Like CASE WHEN @sal = '' THEN NULL ELSE '%' + @sal + '%' END		GROUP BY mSales.IdSal, mSales.Sal		ORDER BY mSales.Sal
	END
	ELSE
	BEGIN
		INSERT @Result(IdSal, Sal, Habilitado)
		SELECT mSales.IdSal, mSales.Sal, CAST(1 AS BIT) AS Habilitado
		FROM Indigo.dbo.mSales
		WHERE mSales.IdSal = @IdSal
	END

	RETURN
END