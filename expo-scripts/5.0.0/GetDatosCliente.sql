USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[GetDatosCliente]    Script Date: 13/03/2024 01:07:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 29 Abril 2024
-- Description:	Devuelve los datos del cliente
-- =============================================

CREATE OR ALTER FUNCTION [dbo].[GetDatosCliente] (@IdPersona INT)
RETURNS
	@Result TABLE (Nombre NVARCHAR(100), 
	               RFC NVARCHAR(13), 
				   IdDocumento TINYINT, 
				   IdUsoCFDI TINYINT, 
				   IdTipoVenta TINYINT, 
				   IdDireccion INT, 
				   Direccion NVARCHAR(200), 
				   IdTransporte INT, 
				   IdPorte TINYINT)
	
AS
BEGIN
    -- Insert statements for procedure here
	INSERT @Result (Nombre, RFC, IdDocumento, IdUsoCFDI, IdTipoVenta, IdDireccion, Direccion, IdTransporte, IdPorte)
	SELECT vPersonas.Nombre, vPersonas.RFC, mDocumentos.IdDocumento , ISNULL(mSatUsoCFDI.IdUsoCFDI,0) AS IdUsoCFDI, mClientes.IdTipoVenta,
	dDirecciones.IdDireccion, Indigo.dbo.FormatAddress(vPersonas.CalleNum, vPersonas.Colonia, vPersonas.Ciudad, vPersonas.Estado, vPersonas.CP) AS Direccion,
	ISNULL(mClientes.IdTransporte,0) AS IdTransporte, ISNULL(mClientes.IdPorte,0) AS IdPorte
	FROM ExpoMR.dbo.mClientes INNER JOIN
	Indigo.dbo.mDocumentos ON mClientes.IdDocumento = mDocumentos.IdDocumento INNER JOIN
	Indigo.dbo.mPerEmp ON mClientes.IdPersona = mPerEmp.IdPersona INNER JOIN
	Indigo.dbo.vPersonas ON mPerEmp.IdPersona = vPersonas.IdPersona INNER JOIN
	Indigo.dbo.dDirecciones ON mPerEmp.Persona = dDirecciones.Persona AND dDirecciones.Fiscal = 1 LEFT OUTER JOIN
	Indigo.dbo.mSatUsoCFDI ON mClientes.IdUsoCFDI = mSatUsoCFDI.IdUsoCFDI
	WHERE mClientes.IdPersona = @IdPersona

	RETURN
END

