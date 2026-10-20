USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[GetLogoProv]    Script Date: 13/03/2024 01:07:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 02 Abril 2024
-- Description:	Devuelve el IdProveedor, Nombre y Logo de los proveedores
-- =============================================

CREATE OR ALTER FUNCTION [dbo].[GetLogoProv] (@IdProveedor INT)
RETURNS
	@Result TABLE (Proveedor INT, 
				   Nombre NVARCHAR(50), 
				   Logo VARBINARY(MAX))
	
AS
BEGIN
    -- Insert statements for procedure here
	INSERT @Result (Proveedor, Nombre, Logo)
	SELECT mStands.Proveedor, mProveedores.Nombre, mStands.Logo	FROM mStands INNER JOIN	mProveedores ON mStands.Proveedor = mProveedores.IdPersona	WHERE @IdProveedor = 0 	OR mProveedores.IdPersona = @IdProveedor	ORDER BY mStands.Rank ASC

	RETURN
END