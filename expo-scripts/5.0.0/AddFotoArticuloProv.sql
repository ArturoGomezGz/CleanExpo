USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[AddFotoArticuloProv]    Script Date: 13/03/2024 01:07:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 02 Abril 2024
-- Description:	Inserta la imagen del articulo
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[AddFotoArticuloProv] 
	-- Add the parameters for the stored procedure here
	@IdArticulo SMALLINT,
	@Orden TINYINT,
	@Foto VARBINARY(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF(SELECT COUNT(dArticulos.IDdArticulos) 
       FROM dArticulos
	   WHERE dArticulos.IdArticulo = @IdArticulo ) < 3 -- 3 = Maximo permitido de fotos por articulos
    BEGIN 
		INSERT dArticulos (IdArticulo, NumFoto, Foto) 
		VALUES (@IdArticulo, @Orden, @Foto) 
    END

END