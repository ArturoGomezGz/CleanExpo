USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[GetMonedasRuleta]    Script Date: 13/03/2024 01:07:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 26 Abril 2024
-- Description:	Devuelve el numero de monedas de una ruleta
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[GetMonedasRuleta] 
	-- Add the parameters for the stored procedure here
	@IdRuleta TINYINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Saldo INT

    -- Insert statements for procedure here
	SET @Saldo = (SELECT Monedas
				  FROM mRuleta
				  WHERE IdRuleta = @IdRuleta)

	RETURN @Saldo
END