USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[ChangeVIP]    Script Date: 01/11/2022 12:42:19 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:	Mario valenzuela
-- Create date: 02 noviembre 2022
-- Description:	Cambia El Status de VIP a true O false
-- =============================================
CREATE PROCEDURE [dbo].[ChangeVIP] 
	@VIP BIT,
	@idPersona INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;    

	--Valida que el usuario existe
	IF EXISTS (SELECT mUsuarios.IdPersona 
		       FROM mUsuarios 
		       WHERE mUsuarios.IdPersona = @idPersona)
	BEGIN
		--Actualiza mClientes
		UPDATE mClientes
		SET VIP = @VIP
		WHERE IdPersona = @idPersona

		SELECT 1 AS Agregado, mClientes.Nombre 
		FROM mClientes
		WHERE mClientes.IdPersona = @idPersona
	END
	ELSE
	BEGIN
		SELECT 0 AS Agregado, '' AS Nombre
	END
END
