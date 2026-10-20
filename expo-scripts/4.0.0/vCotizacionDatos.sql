USE [ExpoMR]
GO

/****** Object:  View [dbo].[vCotizacionDatos]    Script Date: 09/08/2023 01:31:18 p. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[vCotizacionDatos] AS

SELECT mCotizaciones.IdCotizacion, mCotizaciones.Cliente AS IdPersona, Cliente.Nombre AS Cliente, Cliente.RFC, mCotizaciones.IdDocumento, TipoDoc.Documento,
mCotizaciones.IdUsoCFDI, UsoCFDI.UsoCFDI, mCotizaciones.IdTipoVenta, TipoVenta.TipoVenta,
mCotizaciones.IdTransporte, Transportista.Nombre AS Transportista, mCotizaciones.Obsv, mCotizaciones.IdPorte, Porte.Porte,
mCotizaciones.Fiscal, dbo.FormatAddress(DirFiscal.CalleNum, DirFiscal.Colonia, DirFiscal.Ciudad, DirFiscal.Estado, DirFiscal.CP) AS DirFiscal,
mCotizaciones.Entrega, dbo.FormatAddress(DirEntrega.CalleNum, DirEntrega.Colonia, DirEntrega.Ciudad, DirEntrega.Estado, DirEntrega.CP) AS DirEntrega
FROM mCotizaciones INNER JOIN
Indigo.dbo.vPersonas AS Cliente ON mCotizaciones.Cliente = Cliente.IdPersona INNER JOIN
Indigo.dbo.mDocumentos AS TipoDoc ON mCotizaciones.IdDocumento = TipoDoc.IdDocumento INNER JOIN
Indigo.dbo.mTipoVenta AS TipoVenta ON mCotizaciones.IdTipoVenta = TipoVenta.IdTipoVenta INNER JOIN
Indigo.dbo.vDirecciones AS DirFiscal ON mCotizaciones.Fiscal = DirFiscal.IdDireccion INNER JOIN
Indigo.dbo.vDirecciones AS DirEntrega ON mCotizaciones.Entrega = DirEntrega.IdDireccion LEFT OUTER JOIN
Indigo.dbo.mSatUsoCFDI AS UsoCFDI ON mCotizaciones.IdUsoCFDI = UsoCFDI.IdUsoCFDI LEFT OUTER JOIN
Indigo.dbo.vPersonas AS Transportista ON mCotizaciones.IdTransporte = Transportista.IdPersona LEFT OUTER JOIN
Indigo.dbo.mPortes AS Porte ON mCotizaciones.IdPorte = Porte.IdPorte
GO


