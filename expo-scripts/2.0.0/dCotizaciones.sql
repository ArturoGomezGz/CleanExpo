USE [ExpoMR]
GO

/****** Object:  Table [dbo].[dCotizaciones]    Script Date: 06/21/2021 14:47:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[dCotizaciones](
	[IdCotizacion] [int] NOT NULL,
	[IdProducto] [smallint] NOT NULL,
	[Unitario] [real] NOT NULL,
	[Descuento] [decimal](9, 2) NULL,
	[Cantidad] [int] NOT NULL,
	[Solicitado] [bit] NOT NULL,
	[PorConfirmar] [bit] NOT NULL,
	[Fecha] [smalldatetime] NOT NULL,
CONSTRAINT [PK_dCotizaciones] PRIMARY KEY CLUSTERED 
(
	[IdCotizacion] ASC,
	[IdProducto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[dCotizaciones]  WITH CHECK ADD  CONSTRAINT [FK_dCotizaciones_mCotizaciones] FOREIGN KEY([IdCotizacion])
REFERENCES [dbo].[mCotizaciones] ([IdCotizacion])
GO

ALTER TABLE [dbo].[dCotizaciones] CHECK CONSTRAINT [FK_dCotizaciones_mCotizaciones]
GO

ALTER TABLE [dbo].[dCotizaciones]  WITH CHECK ADD  CONSTRAINT [FK_dCotizaciones_mProductos] FOREIGN KEY([IdProducto])
REFERENCES [dbo].[mProductos] ([IdProducto])
GO

ALTER TABLE [dbo].[dCotizaciones] CHECK CONSTRAINT [FK_dCotizaciones_mProductos]
GO
GO


