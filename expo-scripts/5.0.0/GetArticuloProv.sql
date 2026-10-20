USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[GetArticuloProv]    Script Date: 13/03/2024 01:07:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 02 Abril 2024
-- Description:	Devuelve el articulo solicitado del proveedor
-- =============================================

CREATE OR ALTER FUNCTION [dbo].[GetArticuloProv] (@IdArticulo SMALLINT)
RETURNS
  @Result TABLE (IdArticulo SMALLINT, 
				 Nombre NVARCHAR(50), 
				 Descripcion NVARCHAR(250), 
				 IDdArticulos INT, 
				 Foto VARBINARY(MAX), 
				 Proveedor INT, 
				 Cantidad SMALLINT, 
				 NumFoto TINYINT)
AS
BEGIN
    -- Insert statements for procedure here
	INSERT @Result (IdArticulo, Nombre, Descripcion, IDdArticulos, Foto, Proveedor, Cantidad, NumFoto)
	SELECT mArticulos.IdArticulo, mArticulos.Nombre, mArticulos.Descripcion, dArticulos.IDdArticulos, dArticulos.Foto, mArticulos.Proveedor, mArticulos.Cantidad, dArticulos.NumFoto	FROM mArticulos LEFT OUTER JOIN	dArticulos ON mArticulos.IdArticulo = dArticulos.IdArticulo	WHERE mArticulos.IdArticulo = @IdArticulo	ORDER BY mArticulos.IdArticulo, dArticulos.NumFoto

	RETURN
END