Use [ExpoMR]
Go

If exists (select * from sysobjects where name = 'RuletaInfo')
	drop procedure RuletaInfo
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
-- Create date: 2 Oct 2020
-- Description:	
-- =============================================
CREATE PROCEDURE RuletaInfo 
	-- Add the parameters for the stored procedure here
	@cliente int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select mRuleta.IdRuleta, mRuleta.Ruleta, mRuleta.Monedas, 
	Case When dbo.GetRuleta(@cliente) = mRuleta.IdRuleta Then 1 Else 0 End as Activa 
	From mRuleta
	Order by mRuleta.IdRuleta
END
GO
