USE [ExpoMR]
GO

/****** Object:  View [dbo].[vPedidoDatos]    Script Date: 07/09/2021 11:19:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vPedidoDatos] AS

SELECT mPedidos.IdPedido, mPedidos.Cliente AS IdPersona, Cliente.Nombre AS Cliente, Cliente.RFC, mPedidos.IdDocumento, TipoDoc.Documento,
mPedidos.IdUsoCFDI, UsoCFDI.UsoCFDI, mPedidos.IdTipoVenta, TipoVenta.TipoVenta,
mPedidos.IdTransporte, Transportista.Nombre AS Transportista, mPedidos.IdPorte, Porte.Porte,
mPedidos.Fiscal, dbo.FormatAddress(DirFiscal.CalleNum, DirFiscal.Colonia, DirFiscal.Ciudad, DirFiscal.Estado, DirFiscal.CP) AS DirFiscal,
mPedidos.Entrega, dbo.FormatAddress(DirEntrega.CalleNum, DirEntrega.Colonia, DirEntrega.Ciudad, DirEntrega.Estado, DirEntrega.CP) AS DirEntrega
FROM mPedidos INNER JOIN
Indigo.dbo.vPersonas AS Cliente ON mPedidos.Cliente = Cliente.IdPersona INNER JOIN
Indigo.dbo.mDocumentos AS TipoDoc ON mPedidos.IdDocumento = TipoDoc.IdDocumento INNER JOIN
Indigo.dbo.mTipoVenta AS TipoVenta ON mPedidos.IdTipoVenta = TipoVenta.IdTipoVenta INNER JOIN
Indigo.dbo.vDirecciones AS DirFiscal ON mPedidos.Fiscal = DirFiscal.IdDireccion INNER JOIN
Indigo.dbo.vDirecciones AS DirEntrega ON mPedidos.Entrega = DirEntrega.IdDireccion INNER JOIN
Indigo.dbo.mSatUsoCFDI AS UsoCFDI ON mPedidos.IdUsoCFDI = UsoCFDI.IdUsoCFDI LEFT OUTER JOIN
Indigo.dbo.vPersonas AS Transportista ON mPedidos.IdTransporte = Transportista.IdPersona LEFT OUTER JOIN
Indigo.dbo.mPortes AS Porte ON mPedidos.IdPorte = Porte.IdPorte


GO