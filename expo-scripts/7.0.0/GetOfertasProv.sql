USE [ExpoMR]
GO
/****** Object:  UserDefinedFunction [dbo].[GetOfertasProv]    Script Date: 06/11/2025 11:06:37 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 06 Noviembre 2025
-- Description:	Devuelve una lista de las ofertas que tiene un proveedor por laboratorio

-- =============================================

CREATE OR ALTER FUNCTION [dbo].[GetOfertasProv] (@IdProveedor INT)
RETURNS
	@Result TABLE (IdProducto SMALLINT, 
	               Producto NVARCHAR(100),
				   Descuento DECIMAL(9,2),
				   Laboratorio NVARCHAR(30))
AS
BEGIN
    -- Insert statements for procedure here
	INSERT @Result (IdProducto, Producto, Descuento, Laboratorio)
	SELECT mProductos.IdProducto, vProductos.Producto, mProductos.Descuento, mLaboratorios.Laboratorio
	FROM mProductos INNER JOIN 
	vProductos ON mProductos.IdProducto = vProductos.IdProducto INNER JOIN 
	mLaboratorios ON mProductos.IdLaboratorio = mLaboratorios.IdLaboratorio INNER JOIN 
	mProveedores ON mLaboratorios.Proveedor = mProveedores.IdPersona
	WHERE mProveedores.IdPersona = @IdProveedor
	ORDER BY mLaboratorios.Laboratorio, vProductos.Producto

	RETURN
END