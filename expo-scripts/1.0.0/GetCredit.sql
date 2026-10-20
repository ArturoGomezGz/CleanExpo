Use [ExpoMR]
Go
If exists (select * from sysobjects Where name = 'GetCredit')
	drop function GetCredit
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
-- Create date: 27 sept 2020
-- Description:	
-- =============================================
CREATE FUNCTION [dbo].[GetCredit] 
(
	-- Add the parameters for the function here
	@idpersona int,
	@inyeccion bit
)
RETURNS real
AS
BEGIN
	DECLARE @credito real,
			@sucursal tinyint

	Select @sucursal = mUsuarios.Sucursal  
	From mUsuarios
	Where mUsuarios.Referencia = @idpersona
	And mUsuarios.IdTipoUsuario = 1		--IdTipoUsuario 1 = cliente

	If @sucursal = 1	--sucursal 1 = Guadalajara
	Begin
		Set @credito = Indigo.dbo.GetCredit(@idpersona, @inyeccion)
	End
	Else If @sucursal = 2	--sucursal 2 = Queretaro
	Begin
		Set @credito = QtroIndigo.dbo.GetCredit(@idpersona, @inyeccion)
	End
	Else If @sucursal = 3	--sucursal 3 = Mexico
	Begin
		Set @credito = EdomxIndigo.dbo.GetCredit(@idpersona, @inyeccion)
	End

	Select @credito = @credito + ISNULL(mClientes.Credito, 0)
	From mClientes
	Where mClientes.IdPersona = @idpersona

	RETURN @credito
END
GO
