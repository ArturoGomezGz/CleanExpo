USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[UsuariosEliminar]    Script Date: 08/24/2020 11:29:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Carlos Rivera
-- Create date: 24 Agosto 2020
-- Description:	Elimina el usuario de mUsuarios
-- y se cambia a 0 el campo de VIP en mClientes
-- =============================================
CREATE PROCEDURE [dbo].[UsuariosEliminar] 
	@idpersona int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DELETE mUsuarios
	WHERE mUsuarios.IdPersona = @IdPersona
END