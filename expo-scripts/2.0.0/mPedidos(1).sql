USE [ExpoMR]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_mPedido_mClientes]') AND parent_object_id = OBJECT_ID(N'[dbo].[mPedidos]'))
ALTER TABLE [dbo].[mPedidos] DROP CONSTRAINT [FK_mPedido_mClientes]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_mPedidos_mProductos]') AND parent_object_id = OBJECT_ID(N'[dbo].[mPedidos]'))
ALTER TABLE [dbo].[mPedidos] DROP CONSTRAINT [FK_mPedidos_mProductos]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_mPedidos_mUsuarios]') AND parent_object_id = OBJECT_ID(N'[dbo].[mPedidos]'))
ALTER TABLE [dbo].[mPedidos] DROP CONSTRAINT [FK_mPedidos_mUsuarios]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_mPedidos_Fecha]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[mPedidos] DROP CONSTRAINT [DF_mPedidos_Fecha]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_mPedidos_Surtido]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[mPedidos] DROP CONSTRAINT [DF_mPedidos_Surtido]
END

GO

USE [ExpoMR]
GO

/****** Object:  Table [dbo].[mPedidos]    Script Date: 06/21/2021 11:34:28 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[mPedidos]') AND type in (N'U'))
DROP TABLE [dbo].[mPedidos]
GO


