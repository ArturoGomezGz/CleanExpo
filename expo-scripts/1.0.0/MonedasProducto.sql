Use [ExpoMR]
GO
If exists (Select * From sysobjects Where name = 'MonedasProducto')
Begin
	Drop procedure MonedasProducto
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
-- Create date: 5 Sept 2020
-- Description:	Asigna monedas por visitar un producto
-- =============================================
CREATE PROCEDURE MonedasProducto 
	-- Add the parameters for the stored procedure here
	@origen	 int,	-- el id del proveedor
	@destino int,	-- el id del cliente
	@idproducto smallint,
	@usuario int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Declare @success bit = 0,
			@monedas int,
			@idmotivo tinyint = 8	-- IdMotivo 8 = visita al producto

	--Solicito las monedas que tengo que entregar
	Set @monedas = dbo.GetMonedas(@origen, @destino, @idmotivo, null, @idproducto)
	
	If @monedas > 0
	Begin
		-- monedas + el acumulado de lo repartido <= máximo a repartir
		If @monedas + 
			(Select ISNULL(SUM(mTransacciones.Monedas), 0)
			From mTransacciones
			Where mTransacciones.IdMotivo = @idmotivo
			And mTransacciones.IdProducto = @idproducto  
			And DATEPART(DAYOFYEAR, mTransacciones.Fecha) = DATEPART(DAYOFYEAR, GETDATE()) ) <=
			(Select ISNULL(dProductosMax.Maximo, 0)
			From dProductosMax
			Where dProductosMax.IdProducto = @idproducto
			And dProductosMax.Fecha = CAST(GETDATE() as date))
		Begin
			Exec @success = dbo.MonedasTransaccion @origen, @destino, @idmotivo, @monedas, @usuario, @idproducto	
		End
	End

	Return @success
END
GO