USE [ExpoMR]
GO
If exists(select * from sysobjects where name = 'InicializaCostos')
drop procedure InicializaCostos
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
-- Create date: 1 sep 2020
-- Description:	Congela los costos para todos los productos de la expo
-- =============================================
CREATE PROCEDURE InicializaCostos 
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Begin tran

		Update mProductos
		Set Costo = mProdEmp.Costo  
		From Indigo.dbo.mProdEmp 
		Where mProductos.IdProducto = mProdEmp.IdProducto

		Delete Indigo.dbo.mProdExpoCosto

		Insert Indigo.dbo.mProdExpoCosto(IdProducto, Costo)
		Select mProductos.IdProducto, mProductos.Costo
		From mProductos

		If exists(Select mSysVar.Variable
				From mSysVar
				Where mSysVar.Variable = 'CostosCongelados' )
		Begin
			Update mSysVar
			Set mSysVar.Valor = CAST(GETDATE() as nvarchar(50))
			Where mSysVar.Variable = 'CostosCongelados' 
		End
		Else
		Begin
			Insert mSysVar (Variable, Valor)
			Values ('CostosCongelados', CAST(GETDATE() as nvarchar(50))) 
		End

	Commit
END
GO
