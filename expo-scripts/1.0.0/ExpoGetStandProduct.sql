Use [ExpoMR]
GO
If exists (Select * From sysobjects Where name = 'ExpoGetStandProduct')
Begin
	Drop procedure ExpoGetStandProduct
End
-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Rodrigo Cavero
-- Create date: 5 sept 2020
-- Description:	Valida que concuerden los datos  
--				y regresa el nombre del proveedor
-- =============================================
CREATE PROCEDURE ExpoGetStandProduct 
	-- Add the parameters for the stored procedure here
	@stand int, 
	@idproducto smallint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select mStands.Chat, mStands.Paleta, 
	mProveedores.Nombre, mProveedores.Descripcion,
	vProductos.Producto  
	From mStands inner join
	mProveedores on mStands.Proveedor = mProveedores.IdPersona inner join
	mLaboratorios on mProveedores.IdPersona = mLaboratorios.Proveedor inner join
	vProductos on mLaboratorios.IdLaboratorio = vProductos.IdLaboratorio And vProductos.IdProducto = @idproducto   
	Where mStands.Proveedor = @stand

END
GO
