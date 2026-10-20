Use [ExpoMR]
GO
If exists (Select * From sysobjects Where name = 'GetRuleta')
Begin
	Drop function GetRuleta
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
-- Create date: 17 sep 2020
-- Description:	Regresa la ruleta que le corresponde segun su saldo
-- =============================================
CREATE FUNCTION GetRuleta 
(
	-- Add the parameters for the function here
	@cliente int
)
RETURNS tinyint
AS
BEGIN
	-- Declare the return variable here
	DECLARE @Result tinyint = null

	-- Add the T-SQL statements to compute the return value here
	If not exists (	Select mTransacciones.IdTransaccion  
					From mTransacciones 
					Where mTransacciones.Origen = @cliente
					And mTransacciones.IdMotivo = 20)	--IdMotivo 20 = Ruleta
	Begin
		Select @Result = ISNULL(MAX(mRuleta.IdRuleta), 0)
		From mMonedero left outer join
		mRuleta on mMonedero.Saldo >= mRuleta.Monedas
		Where mMonedero.IdPersona = @cliente
	End
	-- Return the result of the function
	RETURN @Result

END
GO

