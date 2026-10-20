Use [ExpoMR]
GO
If exists (Select * From sysobjects Where name = 'StandAnuncios')
Begin
	Drop procedure StandAnuncios
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
-- Create date: 9 Sept 2020
-- Description:	Regresa los anuncios de un proveedor
-- =============================================
CREATE PROCEDURE StandAnuncios 
	-- Add the parameters for the stored procedure here
	@stand int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select Anuncios.Rank, Anuncios.Anuncio    
	From (	Select ROW_NUMBER() over(partition by mAnuncios.Rank Order by mAnuncios.IdAnuncio desc) as Indice,
		mAnuncios.Rank, mAnuncios.Anuncio
		From mAnuncios
		Where mAnuncios.Proveedor = @stand
		And  GETDATE() Between mAnuncios.FeInicio And mAnuncios.FeTermino) as Anuncios
	Where Anuncios.Indice = 1 
	Order by Anuncios.Rank Asc  
END
GO
