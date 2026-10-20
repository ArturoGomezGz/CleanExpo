USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[EditOrdenFotoArticuloProv]    Script Date: 13/03/2024 01:07:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 02 Abril 2024
-- Description:	Asigna el orden a la foto
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[EditOrdenFotoArticuloProv] 
	-- Add the parameters for the stored procedure here
	@IDdArticulos INT,
	@NumFoto TINYINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @IdArticulo INT = (SELECT IdArticulo FROM dArticulos WHERE IDdArticulos = @IDdArticulos)	DECLARE @NumFotoOld TINYINT = (SELECT NumFoto FROM dArticulos WHERE IDdArticulos = @IDdArticulos)		UPDATE dArticulos	SET NumFoto = @NumFotoOld	WHERE IdArticulo = @IdArticulo	AND IDdArticulos <> @IDdArticulos	AND NumFoto = @NumFoto		UPDATE dArticulos	SET NumFoto = @NumFoto	WHERE IDdArticulos = @IDdArticulos

END