USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[DeleteAnuncioProv]    Script Date: 13/03/2024 01:07:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 02 Abril 2024
-- Description:	Elimina el anuncio del proveedor
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[DeleteAnuncioProv] 
	-- Add the parameters for the stored procedure here
	@IdAnuncio INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DELETE mAnuncios
	WHERE IdAnuncio = @IdAnuncio
END