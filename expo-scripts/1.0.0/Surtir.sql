USE [ExpoMR]
GO
If exists(Select * From sysobjects Where name = 'Surtir')
	drop procedure Surtir
GO

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
-- Create date: 24 Oct 2020
-- Description:	
-- =============================================
CREATE PROCEDURE Surtir 
	-- Add the parameters for the stored procedure here
	@vip	bit 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Insert lSurtido (Step)
	Values (0)

	Exec dbo.SurtirBackOrder
	Insert lSurtido (Step)
	Values (1)

	Exec dbo.SurtirSetCredito
	Insert lSurtido (Step)
	Values (2)

	Exec dbo.SurtirSurtido @vip
	Insert lSurtido (Step)
	Values (3)

	Exec dbo.SurtirFacturasGdl
	Insert lSurtido (Step)
	Values (4)

	Exec dbo.SurtirFacturasQtro
	Insert lSurtido (Step)
	Values (5)

	Exec dbo.SurtirFacturasEdomx
	Insert lSurtido (Step)
	Values (6)

	Exec dbo.SurtirBackOrder
	Insert lSurtido (Step)
	Values (7)


END
GO
