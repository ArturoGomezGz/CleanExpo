USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[GetPremiosRuleta]    Script Date: 11/04/2024 01:06:41 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue, Carlos
-- Create date: 2 Oct 2020
-- Description:	Devuelve los premios que tiene la ruleta
-- =============================================
CREATE OR ALTER FUNCTION [dbo].[GetPremiosRuleta] (@IdRuleta TINYINT)
RETURNS
	@Result TABLE (IdPremio SMALLINT, 
				   Premio NVARCHAR(25), 
				   Descripcion NVARCHAR(100), 
				   IdRuleta TINYINT, 
				   Seccion TINYINT, 
				   Foto VARBINARY(MAX), 
				   Cantidad SMALLINT)
AS
BEGIN
    -- Insert statements for procedure here
	INSERT @Result (IdPremio, Premio, Descripcion, IdRuleta, Seccion, Foto, Cantidad)
	SELECT mPremios.IdPremio, mPremios.Premio, mPremios.Descripcion, mPremios.IdRuleta, mPremios.Seccion, mPremios.Foto, mPremios.Cantidad
	FROM mPremios INNER JOIN
	mRuleta ON mPremios.IdRuleta = mRuleta.IdRuleta
	WHERE mRuleta.IdRuleta = @IdRuleta
	ORDER BY mPremios.IdRuleta, mPremios.Seccion

	RETURN
END