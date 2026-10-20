Use [ExpoMR]
GO
If exists (Select * From sysobjects Where name = 'InicializaDotacion')
Begin
	Drop procedure InicializaDotacion
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
-- Create date: 6 sept 2020
-- Description:	Asigna la dotación de monedas a cada proveedor
-- =============================================
CREATE PROCEDURE InicializaDotacion 
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Declare @proveedor int

	Declare cProveedores cursor for
	Select mProveedores.IdPersona  
	From mProveedores

	Open cProveedores 

	Fetch Next From cProveedores
	Into @proveedor
	While @@FETCH_STATUS = 0
	Begin
		Exec dbo.MonedasAsignar 0, @proveedor, 2, null, null, 0  --origen 0 = MR,	IdMotivo 2 = Dotacion, Usuario 0 = MR
		Fetch Next From cProveedores
		Into @proveedor
	End
	Close cProveedores
	Deallocate cProveedores
END
GO
