Use [ExpoMR]
Go
if exists(select * from sysobjects Where name = 'ProveedoresRankProductos')
	drop procedure ProveedoresRankProductos
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
-- Create date: 28 Sept 2020
-- Description:	modifica los rank de los productos de un proveedor
-- =============================================
CREATE PROCEDURE ProveedoresRankProductos
	-- Add the parameters for the stored procedure here
	@proveedor int,
	@idproducto smallint,
	@newrank tinyint = null		-- null el elemento se elimina, 0 el elemento se agrega, n el elemento se mueve
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Declare @oldrank tinyint = null

	Begin Tran
		If @newrank = 0
		Begin
			--se esta asignanado un nuevo elemento y se obtinee el ultimo numero en la lista
			Select @newrank = ISNULL(MAX(mProductos.Rank), 0) + 1 
			From mLaboratorios inner join
			mProductos on mLaboratorios.IdLaboratorio = mProductos.IdLaboratorio
			Where mLaboratorios.Proveedor = @proveedor
			And mProductos.Rank > 0
		End
		Else
		Begin
			--el elemento ya existe y se obtiene su actual rank
			Select @oldrank = mProductos.Rank 
			From mProductos
			Where mProductos.IdProducto = @idproducto 
		End

		If @newrank is null
		Begin
			--se recorren hacia abajo los elementos superiores al que se esta eliminando
			Update mProductos
			Set mProductos.Rank = mProductos.Rank - 1 
			From mLaboratorios inner join
			mProductos on mLaboratorios.IdLaboratorio = mProductos.IdLaboratorio
			Where mLaboratorios.Proveedor = @proveedor
			And mProductos.Rank > @oldrank  
		End
		If @oldrank < @newrank
		Begin
			--el elemento se desplaza hacia arriba se recorren todos hacia abajo
			Update mProductos
			Set mProductos.Rank = mProductos.Rank - 1 
			From mLaboratorios inner join
			mProductos on mLaboratorios.IdLaboratorio = mProductos.IdLaboratorio
			Where mLaboratorios.Proveedor = @proveedor
			And mProductos.Rank between @oldrank and @newrank 
		End
		Else if @oldrank > @newrank
		Begin
			--el elemento se desplaza hacia abajo se recorren todos hacia arriba
			Update mProductos
			Set mProductos.Rank = mProductos.Rank + 1 
			From mLaboratorios inner join
			mProductos on mLaboratorios.IdLaboratorio = mProductos.IdLaboratorio
			Where mLaboratorios.Proveedor = @proveedor
			And mProductos.Rank between   @newrank and @oldrank
		End
		Update mProductos
		Set mProductos.Rank = @newrank
		Where mProductos.IdProducto = @idproducto
	Commit
END
GO
