USE [ExpoMR]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_mCotizacion_mClientes]') AND parent_object_id = OBJECT_ID(N'[dbo].[mCotizaciones]'))
ALTER TABLE [dbo].[mCotizaciones] DROP CONSTRAINT [FK_mCotizacion_mClientes]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_mCotizaciones_mProductos]') AND parent_object_id = OBJECT_ID(N'[dbo].[mCotizaciones]'))
ALTER TABLE [dbo].[mCotizaciones] DROP CONSTRAINT [FK_mCotizaciones_mProductos]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_mCotizaciones_mUsuarios]') AND parent_object_id = OBJECT_ID(N'[dbo].[mCotizaciones]'))
ALTER TABLE [dbo].[mCotizaciones] DROP CONSTRAINT [FK_mCotizaciones_mUsuarios]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_mCotizaciones_Solicitado]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[mCotizaciones] DROP CONSTRAINT [DF_mCotizaciones_Solicitado]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_mCotizaciones_PorConfirmar]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[mCotizaciones] DROP CONSTRAINT [DF_mCotizaciones_PorConfirmar]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_mCotizaciones_Fecha]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[mCotizaciones] DROP CONSTRAINT [DF_mCotizaciones_Fecha]
END

GO

USE [ExpoMR]
GO

/****** Object:  Table [dbo].[mCotizaciones]    Script Date: 06/21/2021 11:14:05 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[mCotizaciones]') AND type in (N'U'))
DROP TABLE [dbo].[mCotizaciones]
GO


