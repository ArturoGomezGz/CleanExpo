USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[GetArticulosProv]    Script Date: 13/03/2024 01:07:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 02 Abril 2024
-- Description:	Devuelve los articulos del proveedor
-- =============================================

CREATE OR ALTER FUNCTION [dbo].[GetArticulosProv] (@IdProveedor INT)
RETURNS
	@Result TABLE (IdArticulo SMALLINT, 
				   Nombre NVARCHAR(50), 
				   Descripcion NVARCHAR(250), 
				   IDdArticulos INT, 
				   Foto VARBINARY(MAX), 
				   NumFoto TINYINT)
AS
BEGIN

    INSERT @Result (IdArticulo, Nombre, Descripcion, IDdArticulos, Foto, NumFoto)
	SELECT mArticulos.IdArticulo, mArticulos.Nombre, mArticulos.Descripcion, 		   (SELECT TOP 1 dArticulos.IDdArticulos FROM dArticulos WHERE dArticulos.IdArticulo = mArticulos.IdArticulo ORDER BY dArticulos.NumFoto ASC) AS IDdArticulos,		   (SELECT TOP 1 dArticulos.Foto FROM dArticulos WHERE dArticulos.IdArticulo = mArticulos.IdArticulo ORDER BY dArticulos.NumFoto ASC) AS Foto,		   (SELECT TOP 1 dArticulos.NumFoto FROM dArticulos WHERE dArticulos.IdArticulo = mArticulos.IdArticulo ORDER BY dArticulos.NumFoto ASC) AS NumFoto	FROM mArticulos 	WHERE mArticulos.Proveedor = @IdProveedor	ORDER BY mArticulos.Nombre

	RETURN
END