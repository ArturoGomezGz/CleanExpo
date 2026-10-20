USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[AddArticuloProv]    Script Date: 13/03/2024 01:07:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 02 Abril 2024
-- Description:	Inserta el articulo del proveedor en mArticulos
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[AddArticuloProv] 
	-- Add the parameters for the stored procedure here
	@Nombre NVARCHAR(50),
	@Descripcion NVARCHAR(250),
	@Cantidad SMALLINT,
	@IdProveedor INT,
	@Foto VARBINARY(MAX),
	@Orden TINYINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	DECLARE @LastId INT

	SET NOCOUNT ON;

	INSERT mArticulos (Nombre, Descripcion, Cantidad, Proveedor)	VALUES (@Nombre, @Descripcion, @Cantidad, @IdProveedor)

	SET @LastId = SCOPE_IDENTITY()
	
	IF(@Foto <> 0x) --0x = 0 bytes
	BEGIN
		INSERT dArticulos (IdArticulo, NumFoto, Foto) 
		VALUES (@LastId, @Orden, @Foto)
	END

END