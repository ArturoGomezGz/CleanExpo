USE [ExpoMR]
GO
if exists(select * from sysobjects where name = 'ProveedoresAgregar')
	drop procedure ProveedoresAgregar
/****** Object:  StoredProcedure [dbo].[ProveedoresAgregar]    Script Date: 08/21/2020 12:58:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================================================
-- Author:		Carlos Rivera
-- Create date: 21 Agosto 2020
-- Description:	Agrega a la pesona como proveedor y se crea su stand para ExpoMR
-- =============================================================================

CREATE PROCEDURE [dbo].[ProveedoresAgregar] 
	@idpersona int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT mProveedores (IdPersona, Nombre, Descripcion)
	SELECT vNombres.IdPersona, SUBSTRING(vNombres.Nombre, 1, 50), SUBSTRING(vNombres.Descripcion, 1, 250) 
	FROM Indigo.dbo.vNombres AS vNombres
	WHERE vNombres.IdPersona = @IdPersona
	
	INSERT mStands (Proveedor)
	VALUES (@idpersona)

	Exec dbo.InicializaStandsMax @idpersona
END