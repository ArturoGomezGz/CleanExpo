USE [ExpoMR]
GO

/****** Object:  Table [dbo].[mPedidos]    Script Date: 06/21/2021 11:43:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[mPedidos](
	[IdPedido] [int] IDENTITY(1,1) NOT NULL,
	[Cliente] [int] NOT NULL,
	[IdDocumento] [tinyint] NOT NULL,
	[IdUsoCFDI] [tinyint] NOT NULL,
	[IdTipoVenta] [tinyint] NOT NULL,
	[IdTransporte] [int] NULL,
	[IdPorte] [tinyint] NULL,
	[Fiscal] [int] NOT NULL,
	[Entrega] [int] NOT NULL,
	[Usuario] [int] NOT NULL,
	[Fecha] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_mPedidos] PRIMARY KEY CLUSTERED 
(
	[IdPedido] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[mPedidos]  WITH CHECK ADD  CONSTRAINT [FK_mPedidos_mClientes] FOREIGN KEY([Cliente])
REFERENCES [dbo].[mClientes] ([IdPersona])
GO

ALTER TABLE [dbo].[mPedidos] CHECK CONSTRAINT [FK_mPedidos_mClientes]
GO

ALTER TABLE [dbo].[mPedidos]  WITH CHECK ADD  CONSTRAINT [FK_mPedidos_mUsuarios] FOREIGN KEY([Usuario])
REFERENCES [dbo].[mUsuarios] ([IdPersona])
GO

ALTER TABLE [dbo].[mPedidos] CHECK CONSTRAINT [FK_mPedidos_mUsuarios]
GO


