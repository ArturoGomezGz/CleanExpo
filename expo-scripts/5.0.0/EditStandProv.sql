USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[EditStandProv]    Script Date: 22/03/2024 02:22:10 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:	Josue Pulido, Carlos Rivera
-- Create date: 19 Abril 2024
-- Description:	Actualiza los parametros definidos por el proveedor para su stand
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[EditStandProv] 
	@IdProveedor INT,
	@Nombre NVARCHAR(50),
	@Descripcion NVARCHAR(250),
	@Paleta TINYINT,
	@Logo VARBINARY(MAX),
	@Background VARBINARY(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;    

	UPDATE mProveedores
	SET Nombre = @Nombre,
		Descripcion = @Descripcion
	WHERE mProveedores.IdPersona = @IdProveedor

	UPDATE mStands
	SET	Paleta = @Paleta,
		Logo = CASE WHEN @Logo = 0x   --0x = 0 bytes
					THEN CASE WHEN Logo IS NOT NULL THEN Logo ELSE NULL END
					ELSE @Logo
			   END,
		Background = CASE WHEN @Background = 0x --0x = 0 bytes
						  THEN CASE WHEN Background IS NOT NULL THEN Background ELSE NULL END
						  ELSE @Background
					 END 
	WHERE Proveedor = @IdProveedor
END
