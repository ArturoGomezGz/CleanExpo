Use [ExpoMR]
GO
If exists (Select * From sysobjects Where name = 'MonedasTransaccion')
Begin
	Drop procedure MonedasTransaccion
End
--================================================
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
-- Create date: 29 Ago 2020
-- Description:	Realiza una transaccion de movimiento de monedas
-- =============================================
CREATE PROCEDURE MonedasTransaccion 
	-- Add the parameters for the stored procedure here
	@origen		int,
	@destino	int,
	@idmotivo	tinyint,
	@monedas	int,
	@usuario	int,
	@idproducto smallint = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    -- Insert statements for procedure here
	Declare @success bit = 0
	
	--valida que el destino tenga monedero
	If not exists (Select mMonedero.IdPersona
					From mMonedero 
					Where mMonedero.IdPersona = @destino)
	Begin
		--inicializa el monedero
		Insert mMonedero (IdPersona, Saldo)
		Values (@destino, 0)
	End

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
	BEGIN TRANSACTION;
		--valida que el origen sea el sistema (0)
		--o que tenga saldo suficiente
		If @origen = 0	
			Or @monedas <=
				(Select mMonedero.Saldo   
				From mMonedero
				Where mMonedero.IdPersona = @origen)  
		Begin
			--guarda la transaccion
			Insert mTransacciones (Origen, SaldoOrigen, Destino, SaldoDestino, Monedas, IdMotivo, IdProducto, Usuario)
			Select @origen, origen.Saldo, @destino, destino.Saldo, @monedas, @idmotivo, @idproducto, @usuario 
			From mMonedero as origen,
			mMonedero as destino
			Where origen.IdPersona = @origen
			And destino.IdPersona = @destino

			--sustrae las monedas del origen
			Update mMonedero
			Set mMonedero.Saldo = mMonedero.Saldo - @monedas
			Where mMonedero.IdPersona = @origen
			
			--agrega las monedas al destino
			Update mMonedero
			Set mMonedero.Saldo = mMonedero.Saldo + @monedas
			Where mMonedero.IdPersona = @destino 

			Set @success = 1
		End
	COMMIT TRANSACTION;
	
	return @success

END
GO
