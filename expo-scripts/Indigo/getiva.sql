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
-- Create date: 15 Dic 2009
-- Description:	Regresa el iva actual
-- =============================================
CREATE FUNCTION GetIva
(
	-- Add the parameters for the function here
	@opcion as tinyint
)
RETURNS real
AS
BEGIN
	declare @iva real
	
	select @iva = cast(valor as real)
	from msysvar
	where variable = 'Iva'

	If @opcion = 0 
		select @iva = @iva /100
	
	return @iva
END
GO

