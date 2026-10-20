USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[ProveedoresEliminar]    Script Date: 08/24/2020 11:29:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Carlos Rivera
-- Create date: 24 Agosto 2020
-- Description:	Elimina la alta del proveedor
-- =============================================
CREATE PROCEDURE [dbo].[ProveedoresEliminar] 
	@proveedor int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	    -- Insert statements for procedure here
		IF NOT EXISTS(SELECT mUsuarios.IdPersona
					  FROM mUsuarios
					  WHERE mUsuarios.Referencia = @proveedor)
		   AND NOT EXISTS(SELECT mLaboratorios.Proveedor
						  FROM mLaboratorios
						  WHERE mLaboratorios.Proveedor = @proveedor)
		   AND NOT EXISTS(SELECT mPromociones.Proveedor  
						  FROM mPromociones inner join
						  dPromoCliente on mPromociones.IdPromocion = dPromoCliente.IdPromocion   
						  WHERE mPromociones.Proveedor = @proveedor)			
		BEGIN
		
			DELETE dPromociones
			FROM mPromociones INNER JOIN
			dPromociones ON mPromociones.IdPromocion = dPromociones.IdPromocion
			WHERE mPromociones.Proveedor = @proveedor
		
			DELETE mPromociones
			WHERE mPromociones.Proveedor = @proveedor
		
			DELETE dArticulos
			FROM mArticulos INNER JOIN
			dArticulos ON mArticulos.IdArticulo = dArticulos.IdArticulo
			WHERE mArticulos.Proveedor = @proveedor
		
			DELETE mArticulos
			WHERE mArticulos.Proveedor = @proveedor
		
			DELETE mAnuncios
			WHERE mAnuncios.Proveedor = @proveedor
		
			DELETE dStandsMax
			WHERE dStandsMax.Proveedor = @proveedor
		
			DELETE mStands
			WHERE mStands.Proveedor = @proveedor
		
			DELETE mLaboratorios
			WHERE mLaboratorios.Proveedor = @proveedor
		
			DELETE mProveedores
			WHERE mProveedores.IdPersona = @proveedor
		
			RETURN 1 --BAJA CORRECTA
		
		END
		ELSE
		BEGIN
			RETURN 0 --EL PROVEEDOR TIENE USUARIO PARA INGREAR AL SISTEMA O TIENE LABORATORIOS ASIGNADOS
		END
END