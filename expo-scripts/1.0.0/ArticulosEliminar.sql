USE [ExpoMR]
GO
If exists (select * From sysobjects Where name = 'ArticulosEliminar')
Begin
	drop procedure ArticulosEliminar
End
/****** Object:  StoredProcedure [dbo].[ProveedoresEliminaArticulo]    Script Date: 09/08/2020 17:46:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =====================================================
-- Author:		Carlos Rivera
-- Create date: 24 Agosto 2020
-- Description:	Elimina fotos y articulo del proveedor
-- =====================================================

CREATE PROCEDURE [dbo].[ArticulosEliminar] 
	@idarticulo INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	    -- Insert statements for procedure here
		IF EXISTS(SELECT mArticulos.IdArticulo
				  FROM mArticulos LEFT OUTER JOIN
				  dPromociones ON mArticulos.IdArticulo = dPromociones.IdArticulo LEFT OUTER JOIN
				  dPromoCliente ON dPromociones.IdPromocion = dPromoCliente.IdPromocion
				  WHERE mArticulos.IdArticulo = @idarticulo
				  And (dPromoCliente.IdPromocion IS NOT NULL
				     OR dPromociones.IdPromocion IS NOT NULL))
		BEGIN
			RETURN 0 --El Articulo no puede ser eliminado
		END
		ELSE
		BEGIN
			DELETE dArticulos
			FROM mArticulos INNER JOIN
			dArticulos ON mArticulos.IdArticulo = dArticulos.IdArticulo
			WHERE dArticulos.IdArticulo = @idarticulo
			
			DELETE mArticulos
			WHERE mArticulos.IdArticulo = @idarticulo
			
			RETURN 1 --El Articulo fue eliminado
		END
END
