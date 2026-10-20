USE [ExpoMR]
GO
If exists (select * From sysobjects Where name = 'RuletaGiro')
Begin
	drop procedure RuletaGiro
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
-- Create date: 17 Sep 2020
-- Description:	
-- =============================================
CREATE PROCEDURE RuletaGiro 
	-- Add the parameters for the stored procedure here
	@Cliente int,
	@usuario int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Declare @ruleta tinyint = null,
			@monedas int

	Select @ruleta = mRuleta.IdRuleta,
	@monedas = mRuleta.Monedas  
	From mRuleta
	Where mRuleta.IdRuleta = dbo.GetRuleta(@Cliente)

	If @ruleta > 0
	Begin
		BEGIN TRANSACTION
			exec MonedasTransaccion @cliente, 0, 20, @monedas, @usuario, null --destino 0 = MR, idmotivo 20 = ruleta
		
			Update mJuego
			Set mJuego.Cliente = @cliente
			Where mJuego.IdJuego = (Select MIN(mJuego.IdJuego)
									From mJuego
									Where mJuego.Cliente is null
									And mJuego.IdRuleta = @ruleta) 
	
		COMMIT TRANSACTION
	End
		
	Select mJuego.GradosGiro,   
	mPremios.IdPremio, mPremios.Premio, mPremios.Descripcion      
	From mJuego inner join
	mPremios on mJuego.IdPremio = mPremios.IdPremio 
	Where mJuego.Cliente = @Cliente  

END
GO
