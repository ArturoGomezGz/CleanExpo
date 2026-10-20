USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[EditProv]    Script Date: 22/03/2024 02:22:10 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:	Josue Pulido, Carlos Rivera
-- Create date: 22 Abril 2024
-- Description:	Actualiza los datos del proveedor
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[EditProv] 
	@IdProveedor INT,
	@Nombre NVARCHAR(50),
	@Descripcion NVARCHAR(250)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;    

	UPDATE mProveedores
	SET Nombre = CASE WHEN @Nombre = '' THEN mProveedores.Nombre ELSE UPPER(@Nombre) END,
		Descripcion = CASE WHEN @Descripcion = '' THEN NULL ELSE UPPER(@Descripcion) END
	WHERE mProveedores.IdPersona = @IdProveedor
END
