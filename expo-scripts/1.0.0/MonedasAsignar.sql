Use [ExpoMR]
GO
If exists (Select * From sysobjects Where name = 'MonedasAsignar')
Begin
	Drop procedure MonedasAsignar
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
-- Create date: 6 Sept 2020
-- Description:	Asigna monedas por diversos motivos
-- =============================================
CREATE PROCEDURE MonedasAsignar
	-- Add the parameters for the stored procedure here
	@origen int,
	@destino int,	-- el id del cliente
	@idmotivo tinyint,
	@monedas int = null,
	@idproducto smallint = null,
	@usuario int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Declare @success bit = 0,
			@fecha date = null

	--Solicito las monedas que tengo que entregar
	If @idmotivo in (1, 2, 3, 4, 5, 6)
	--idmotivo	1 = Volumen Venta
	--			2 = Dotación Expo
	--			3 = Bienvenida Expo
	--			4 = Confirmación Producto
	--			5 = Primer Pedido
	--			6 = Contacto Proveedor
	Begin
		If @idmotivo = 2
			Set @fecha = GETDATE()
		Set @monedas = dbo.GetMonedas(@origen, @destino, @idmotivo, @fecha, @idproducto)
	End

	If @monedas > 0
	Begin
		Exec @success = dbo.MonedasTransaccion @origen, @destino, @idmotivo, @monedas, @usuario, @idproducto
	End

	Return @success

END
GO
