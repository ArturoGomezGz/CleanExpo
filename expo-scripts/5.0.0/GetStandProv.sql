USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[GetStandProv]    Script Date: 13/03/2024 01:07:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 02 Abril 2024
-- Description:	Devuelve los parametros de configuracion del stand del proveedor
-- =============================================

CREATE OR ALTER FUNCTION [dbo].[GetStandProv] (@IdProveedor INT)
RETURNS
	@Result TABLE(Proveedor INT, 
				  Nombre NVARCHAR(50), 
				  Descripcion NVARCHAR(250), 
				  Template TINYINT, 
				  Rank TINYINT, 
				  Chat NVARCHAR(100), 
				  Paleta TINYINT, 
				  Logo VARBINARY(MAX), 
				  Background VARBINARY(MAX))	
AS
BEGIN
    -- Insert statements for procedure here
	INSERT @Result (Proveedor, Nombre, Descripcion, Template, Rank, Chat, Paleta, Logo, Background)
	SELECT mStands.Proveedor, mProveedores.Nombre, mProveedores.Descripcion,	mStands.Template, mStands.Rank, ISNULL(mStands.Chat, '') AS Chat,	mStands.Paleta, mStands.Logo, mStands.Background	FROM mStands INNER JOIN	mProveedores ON mStands.Proveedor = mProveedores.IdPersona 	WHERE mProveedores.IdPersona = @idProveedor  	ORDER BY mStands.Rank ASC

	RETURN
END