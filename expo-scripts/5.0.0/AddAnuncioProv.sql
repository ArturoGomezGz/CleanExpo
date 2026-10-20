USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[AddAnuncioProv]    Script Date: 13/03/2024 01:07:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 02 Abril 2024
-- Description:	Inserta el anuncio del proveedor en mAnuncios
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[AddAnuncioProv] 
	-- Add the parameters for the stored procedure here
	@IdAnuncio INT,
	@Proveedor INT,
	@Anuncio NVARCHAR(250),
	@FeInicio SMALLDATETIME,
	@FeTermino SMALLDATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF(@IdAnuncio = 0)
	BEGIN
		INSERT mAnuncios (Proveedor, Anuncio, FeInicio, FeTermino, Rank)		VALUES (@Proveedor, @Anuncio, @FeInicio, @FeTermino, 1)
	END
	ELSE
	BEGIN
		UPDATE mAnuncios		SET Anuncio = @Anuncio,			FeInicio = @FeInicio,			FeTermino = @FeTermino		WHERE mAnuncios.IdAnuncio = @IdAnuncio
	END
END