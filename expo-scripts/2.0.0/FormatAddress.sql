USE ExpoMR
GO
/****** Object:  UserDefinedFunction [dbo].[FormatAddress]    Script Date: 06/22/2021 13:27:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Rodrigo Cavero
-- Create date: 15 Nov 2017
-- Description:	Da formato a la direción
-- =============================================
CREATE FUNCTION [dbo].[FormatAddress] 
(
	-- Add the parameters for the function here
	@calle NVARCHAR(100),
	@colonia NVARCHAR(100),
	@ciudad NVARCHAR(100),
	@estado NVARCHAR(50),
	@cp NVARCHAR(5)
)
RETURNS NVARCHAR(400)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @address NVARCHAR(400)

	-- Add the T-SQL statements to compute the return value here
	SET @address = @calle + CHAR(10) + @colonia + CHAR(10) + @ciudad + ', ' + @estado + ', México. C.P. ' + @cp

	-- Return the result of the function
	RETURN @address

END
