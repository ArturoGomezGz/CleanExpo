Use [ExpoMR]
GO
If exists (Select * From sysobjects Where name = 'GetMonedas')
Begin
	Drop function GetMonedas
End
-- ================================================
-- Template generated from Template Explorer using:
-- Create Scalar Function (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the function.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Rodrigo Cavero
-- Create date: 6 Sept 2020
-- Description:	Regresa la cantidad de monedas a otorgar según el motivo
-- =============================================
CREATE FUNCTION GetMonedas 
(
	-- Add the parameters for the function here
	@origen	 int,	-- el id del proveedor
	@destino int,	-- el id del cliente
	@idmotivo int,
	@fecha date = null,
	@idproducto smallint = null
)
RETURNS int
AS
BEGIN
	-- Declare the return variable here
	DECLARE @monedas int = 0,
			@venta int = 0

	-- Add the T-SQL statements to compute the return value here
	If @idmotivo = 1	--idmotivo 1 = volumen venta
	Begin
		--saco el monto de venta actual del cliente
		Select @venta = vPedido.Total  
		From vPedido
		Where vPedido.Cliente = @destino

		If @venta between 1000 and 10000
			Set @monedas = 1000 		
		Else if @venta between 10000 and 20000
			Set @monedas = 2500
		Else if @venta between 20000 and 50000
			Set @monedas = 5000
		Else if @venta between 50000 and 100000
			Set @monedas = 10000
		Else if @venta between 100000 and 500000
			Set @monedas = 20000
		Else if @venta > 500000 
			Set @monedas = 40000


		Select @monedas = @monedas - ISNULL(SUM(mTransacciones.Monedas), 0)
		From mTransacciones
		Where mTransacciones.Destino = @destino
		And mTransacciones.IdMotivo = @idmotivo 
	End
	Else
	Begin
		--valido que no haya recibido monedas ya por ese motivo
		If not exists (	Select mTransacciones.IdTransaccion  
						From mTransacciones
						Where mTransacciones.Origen = @origen
						And mTransacciones.Destino = @destino
						And mTransacciones.IdMotivo = @idmotivo
						And (@fecha is null
							Or CAST(mTransacciones.Fecha as date) = @fecha)
						And (@idproducto is null
							Or mTransacciones.IdProducto = @idproducto))
		Begin
			Select @monedas = Case When dbo.GetVIP(null) = 1 Then mMotivos.MonedasVIP Else mMotivos.MonedasExpo End   
			From mMotivos
			Where mMotivos.IdMotivo = @idmotivo

			If @idmotivo = 2	--idmotivo 2 = dotacion expo
				Select @monedas = @monedas * dbo.GetDotacionMultimarca(@destino)
		End
	End

	-- Return the result of the function
	RETURN @monedas

END
GO

