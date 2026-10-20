Use [ExpoMR]
GO

If exists(Select * From sysobjects where name = 'ExpoLogin')
	drop procedure ExpoLogin
--================================================
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
-- Create date: 4 Oct 2020
-- Description:	Regresa que tipo de expo esta ocurriendo
-- =============================================
CREATE PROCEDURE ExpoLogin 
	-- Add the parameters for the stored procedure here
	@link nvarchar(9) = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select vCalendario.VIP as Expo, vCalendario.Fin, UsuVIP.UserName
	From vCalendario left outer join
		(Select mUsuarios.UserName, mClientes.VIP
		From mUsuarios inner join
		mClientes on mUsuarios.Referencia = mClientes.IdPersona
		Where mUsuarios.Link = @link) as UsuVIP on vCalendario.VIP = UsuVIP.VIP
	Where GETDATE() Between vCalendario.Inicio And vCalendario.Fin
	
END
GO
