USE [ExpoMR]
GO

/****** Object:  Table [dbo].[mCotizaciones]    Script Date: 06/21/2021 11:30:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[mCotizaciones](
	[IdCotizacion] [int] IDENTITY(1,1) NOT NULL,
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
 CONSTRAINT [PK_mCotizaciones] PRIMARY KEY CLUSTERED 
(
	[IdCotizacion] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[mCotizaciones]  WITH CHECK ADD  CONSTRAINT [FK_mCotizaciones_mClientes] FOREIGN KEY([Cliente])
REFERENCES [dbo].[mClientes] ([IdPersona])
GO

ALTER TABLE [dbo].[mCotizaciones] CHECK CONSTRAINT [FK_mCotizaciones_mClientes]
GO

ALTER TABLE [dbo].[mCotizaciones]  WITH CHECK ADD  CONSTRAINT [FK_mCotizaciones_mUsuarios] FOREIGN KEY([Usuario])
REFERENCES [dbo].[mUsuarios] ([IdPersona])
GO

ALTER TABLE [dbo].[mCotizaciones] CHECK CONSTRAINT [FK_mCotizaciones_mUsuarios]
GO


