USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[ProveedoresEliminaLaboratorio]    Script Date: 08/24/2020 11:29:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================================
-- Author:		Carlos Rivera
-- Create date: 24 Agosto 2020
-- Description:	Elimina el laboratorio y sus productos asignados al proveedor
-- ===========================================================================

CREATE PROCEDURE [dbo].[ProveedoresEliminaLaboratorio] 
	@idLaboratorio int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	    -- Insert statements for procedure here
		IF EXISTS(SELECT Productos.IdProducto
					FROM (
							SELECT dProductosDescuento.IdProducto
							FROM dProductosDescuento
							UNION
							SELECT mTransacciones.IdProducto
							FROM mTransacciones
							UNION
							SELECT mCotizaciones.IdProducto
							FROM mCotizaciones
							UNION
							SELECT mPedidos.IdProducto
							FROM mPedidos
						) AS Productos INNER JOIN
				  mProductos ON Productos.IdProducto = mProductos.IdProducto INNER JOIN
				  mLaboratorios ON mProductos.IdLaboratorio = mLaboratorios.IdLaboratorio
				  WHERE mLaboratorios.IdLaboratorio = @idLaboratorio		
				 )
		BEGIN
			RETURN 0 --El Laboratorio no puede ser eliminado
		
		END
		ELSE
		BEGIN
			DELETE mProductos
			WHERE mProductos.IdLaboratorio = @idLaboratorio
			
			DELETE mLaboratorios
			WHERE mLaboratorios.IdLaboratorio = @idLaboratorio
		
			RETURN 1 --El Laboratorio fue eliminado
		END
END
