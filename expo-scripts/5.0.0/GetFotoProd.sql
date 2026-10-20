USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[GetFotoProd]    Script Date: 13/03/2024 01:07:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 02 Abril 2024
-- Description:	Devuelve la foto del producto
-- Valores de atributos: 1 = Isometrica
--						 2 = Indicaciones
--						 4 = Formula
--						 8 = Dosis
--						 16 = Informacion Nutrimental
--						 32 = Informacion General
-- =============================================

CREATE OR ALTER FUNCTION [dbo].[GetFotoProd] (@IdProducto SMALLINT)
RETURNS
	@Result TABLE (IdFoto SMALLINT, 
	               IdProducto SMALLINT, 
				   Foto IMAGE)
AS
BEGIN
    -- Insert statements for procedure here
	INSERT @Result (IdFoto, IdProducto, Foto)
	SELECT IdFoto, IdProducto, Foto
	FROM Fotos.dbo.mFotos
	WHERE IdProducto = @IdProducto
	AND Atributos = 1 --Isometrica

	RETURN
END