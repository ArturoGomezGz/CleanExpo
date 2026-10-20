Use [ExpoMR]
GO
If exists (Select * From sysobjects Where name = 'MonedasStand')
Begin
	Drop procedure MonedasStand
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
-- Description:	Asigna monedas por visitar un stand
-- =============================================
CREATE PROCEDURE MonedasStand 
	-- Add the parameters for the stored procedure here
	@origen		int,	-- el id del proveedor
	@destino	int,	-- el id del cliente
	@usuario	int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	-- monedas + el acumulado de lo repartido > máximo a repartir
	Declare @success bit = 0,
			@monedas int,
			@idmotivo tinyint = 7	-- IdMotivo 7 = visita al stand

	--Solicito las monedas que tengo que entregar
	Set @monedas = dbo.GetMonedas(@origen, @destino, @idmotivo, null, null)
	
	If @monedas > 0
	Begin
		-- monedas + el acumulado de lo repartido <= máximo a repartir
		If @monedas +  
			(Select ISNULL(SUM(mTransacciones.Monedas), 0)
			From mTransacciones
			Where mTransacciones.IdMotivo = @idmotivo
			And mTransacciones.Origen = @origen   
			And DATEPART(DAYOFYEAR, mTransacciones.Fecha) = DATEPART(DAYOFYEAR, GETDATE())) <=
			(Select ISNULL(dStandsMax.Maximo, 0)
			From dStandsMax
			Where dStandsMax.Proveedor = @origen
			And dStandsMax.Fecha = CAST(GETDATE() as date))
		Begin
			Exec @success = dbo.MonedasTransaccion @origen, @destino, @idmotivo, @monedas, @usuario, null	
		End
	End

	Return @success
END
GO
