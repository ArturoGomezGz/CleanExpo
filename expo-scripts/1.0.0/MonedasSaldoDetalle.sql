Use [ExpoMR]
GO
If exists (Select * From sysobjects Where name = 'MonedasSaldoDetalle')
Begin
	Drop procedure MonedasSaldoDetalle
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
-- Create date: 13 Sep 2020
-- Description:	Regresa el detalle de todas las transacciones
-- =============================================
CREATE PROCEDURE MonedasSaldoDetalle
	-- Add the parameters for the stored procedure here
	@idpersona	int,
	@saldo	int output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select @saldo = mMonedero.Saldo
	From mMonedero
	Where mMonedero.IdPersona = @idpersona  
	
	Select mTransacciones.IdTransaccion, mTransacciones.Fecha,
	mTransacciones.SaldoDestino + mTransacciones.Monedas as Saldo, mTransacciones.Monedas, 
	Case When mTransacciones.Origen = 0 Then 'ExpoMR' Else mProveedores.Nombre End as Persona,
	mMotivos.Motivo + ' ' + ISNULL(vProductos.Nombre, '')  as Concepto
	From mTransacciones inner join
	mMotivos on mTransacciones.IdMotivo = mMotivos.IdMotivo left outer join
	vProductos on mTransacciones.IdProducto = vProductos.IdProducto left outer join
	mProveedores on mTransacciones.Origen = mProveedores.IdPersona    
	Where mTransacciones.Destino = @idpersona  
	Union
	Select mTransacciones.IdTransaccion, mTransacciones.Fecha,
	mTransacciones.SaldoOrigen - mTransacciones.Monedas as Saldo, - mTransacciones.Monedas, 
	Case When mTransacciones.Destino = 0 Then 'ExpoMR' Else vNombres.Nombre End as Persona,
	mMotivos.Motivo	+ ' ' + ISNULL(vProductos.Nombre, '') as Concepto
	From mTransacciones inner join
	mMotivos on mTransacciones.IdMotivo = mMotivos.IdMotivo left outer join
	vProductos on mTransacciones.IdProducto = vProductos.IdProducto left outer join
	vNombres on mTransacciones.Destino = vNombres.IdPersona   
	Where mTransacciones.Origen = @idpersona 
	Order by mTransacciones.IdTransaccion desc

END
GO
