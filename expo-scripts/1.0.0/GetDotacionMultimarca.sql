Use [ExpoMR]
GO

If exists (Select * From sysobjects Where name = 'GetDotacionMultimarca')
	drop function GetDotacionMultimarca
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
-- Create date: 8 Oct 2020
-- Description:	Regresa el porcentaje extra a dar para la dotación de los proveedores multimarca
-- =============================================
CREATE FUNCTION GetDotacionMultimarca 
(
	-- Add the parameters for the function here
	@proveedor int
)
RETURNS decimal(9,2)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @Result decimal(9,2)

	-- Add the T-SQL statements to compute the return value here
	Select @Result= CAST(Case	When COUNT(mLaboratorios.IdLaboratorio) = 1 Then 1
								When COUNT(mLaboratorios.IdLaboratorio) > 1 Then 1 + CAST(dbo.GetSysVar('DotacionMultimarca') as decimal(9,2)) / 100  
								Else 0 End as decimal(9,2))
	From mLaboratorios
	Where mLaboratorios.Proveedor = @proveedor


	-- Return the result of the function
	RETURN @Result

END
GO

