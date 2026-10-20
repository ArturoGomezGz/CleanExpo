Use [ExpoMR]
GO
If exists (Select * From sysobjects Where name = 'GetVIP')
Begin
	Drop function GetVIP
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
-- Create date: 29 ago 2020
-- Description:	regresa si coincide la fecha con un dia VIP en el calendario
-- =============================================
CREATE FUNCTION GetVIP 
(
	-- Add the parameters for the function here
	@fecha	date = null
)
RETURNS bit
AS
BEGIN
	-- Declare the return variable here
	DECLARE @Result bit = null

	-- Add the T-SQL statements to compute the return value here
	Select @Result = mCalendario.VIP
	From mCalendario
	Where mCalendario.Fecha = ISNULL(@fecha, CAST(GETDATE() as date))

	-- Return the result of the function
	RETURN @Result

END
GO

