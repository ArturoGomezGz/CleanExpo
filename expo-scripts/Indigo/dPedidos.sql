USE [ExpoMR]
GO

/****** Object:  Table [dbo].[dPedidos]    Script Date: 06/21/2021 14:48:26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[dPedidos](
	[IdPedido] [int] NOT NULL,
	[IdProducto] [smallint] NOT NULL,
	[Unitario] [real] NOT NULL,
	[Descuento] [decimal](9, 2) NOT NULL,
	[Cantidad] [int] NOT NULL,
	[Surtido] [int] NOT NULL,
	[Fecha] [smalldatetime] NOT NULL,
	[Colocado] [bit] NOT NULL,
CONSTRAINT [PK_dPedidos] PRIMARY KEY CLUSTERED 
(
	[IdPedido] ASC,
	[IdProducto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[dPedidos]  WITH CHECK ADD  CONSTRAINT [FK_dPedidos_mPedidos] FOREIGN KEY([IdPedido])
REFERENCES [dbo].[mPedidos] ([IdPedido])
GO

ALTER TABLE [dbo].[dPedidos] CHECK CONSTRAINT [FK_dPedidos_mPedidos]
GO

ALTER TABLE [dbo].[dPedidos]  WITH CHECK ADD  CONSTRAINT [FK_dPedidos_mProductos] FOREIGN KEY([IdProducto])
REFERENCES [dbo].[mProductos] ([IdProducto])
GO

ALTER TABLE [dbo].[dPedidos] CHECK CONSTRAINT [FK_dPedidos_mProductos]
GO

ALTER TABLE [dbo].[dPedidos] ADD  CONSTRAINT [DF_dPedidos_Colocado]  DEFAULT ((0)) FOR [Colocado]
GO