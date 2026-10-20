USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[EditArticuloProv]    Script Date: 13/03/2024 01:07:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 02 Abril 2024
-- Description:	Inserta la imagen del articulo en 
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[EditArticuloProv] 
	-- Add the parameters for the stored procedure here
	@IdArticulo SMALLINT,
	@Nombre NVARCHAR(50),
	@Descripcion NVARCHAR(250),
	@Cantidad SMALLINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE mArticulos	SET Nombre = CASE WHEN @Nombre = '' THEN mArticulos.Nombre ELSE @Nombre END,		Descripcion = @Descripcion,		Cantidad = CASE WHEN dbo.GetArticulosRegalados(@IdArticulo) > @Cantidad THEN mArticulos.Cantidad ELSE @Cantidad END	WHERE IdArticulo = @IdArticulo

END