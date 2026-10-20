Use [ExpoMR]
GO
If exists (Select * From sysobjects Where name = 'GetVIPStr')
Begin
	Drop function GetVIPStr
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
-- Description:	regresa el string VIP si coincide la fecha con un dia VIP en el calendario
-- =============================================
CREATE FUNCTION GetVIPStr 
(
	-- Add the parameters for the function here
	@fecha date = null
)
RETURNS nvarchar(3)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @Result nvarchar(3) = ''

	-- Add the T-SQL statements to compute the return value here
	If dbo.GetVIP(@fecha) = 1 
		Set @Result = 'VIP'

	-- Return the result of the function
	RETURN @Result

END
GO

