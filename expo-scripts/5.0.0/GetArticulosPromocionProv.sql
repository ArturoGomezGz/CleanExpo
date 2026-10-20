USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[GetArticulosPromocionProv]    Script Date: 13/03/2024 01:07:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 02 Abril 2024
-- Description:	Devuelve la lista de los articulos de una promocion de proveedor
-- =============================================

CREATE OR ALTER FUNCTION [dbo].[GetArticulosPromocionProv] (@IdPromocion SMALLINT)
RETURNS
	 @Result TABLE (IdArticulo SMALLINT, 
	                Articulo NVARCHAR(50), 
					Foto VARBINARY(MAX), 
					Cantidad SMALLINT)
AS
BEGIN

	INSERT @Result (IdArticulo, Articulo, Foto, Cantidad)
	SELECT dPromociones.IdArticulo, mArticulos.Nombre AS Articulo,
	(SELECT TOP 1 dArticulos.Foto FROM dArticulos WHERE dArticulos.IdArticulo = mArticulos.IdArticulo ORDER BY dArticulos.NumFoto ASC) AS Foto,
	dPromociones.Cantidad
	FROM dPromociones INNER JOIN
	mArticulos ON dPromociones.IdArticulo = mArticulos.IdArticulo
	WHERE dPromociones.IdPromocion = @IdPromocion

	RETURN
END