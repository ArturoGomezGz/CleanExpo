USE [ExpoMR]
GO
If exists (select * From sysobjects Where name = 'PromocionesEliminar')
Begin
	drop procedure PromocionesEliminar
End
/****** Object:  StoredProcedure [dbo].[ProveedoresEliminaPromocion]    Script Date: 09/10/2020 10:13:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ==================================================
-- Author:		Carlos Rivera
-- Create date: 24 Agosto 2020
-- Description:	Elimina la promocion del proveedor
-- ==================================================

CREATE PROCEDURE [dbo].[PromocionesEliminar] 
	@idpromocion int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	    -- Insert statements for procedure here
		IF EXISTS(SELECT dPromoCliente.IdPromocion
				  FROM dPromoCliente
				  WHERE dPromoCliente.IdPromocion = @idpromocion)
		BEGIN
			RETURN 0 --La promocion no puede ser eliminada
		
		END
		ELSE
		BEGIN
			DELETE dPromociones
			FROM mPromociones INNER JOIN
			dPromociones ON mPromociones.IdPromocion = dPromociones.IdPromocion
			WHERE mPromociones.IdPromocion = @idpromocion		
		
			DELETE mPromociones
			WHERE mPromociones.IdPromocion = @idpromocion
		
			RETURN 1 --La promocion fue eliminada
		END
END
