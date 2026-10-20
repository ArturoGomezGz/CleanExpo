USE [ExpoMR]
GO

/****** Object:  Table [dbo].[dProductosDescuento]    Script Date: 08/17/2021 14:15:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[dProductosDescuento](
	[IdProducto] [smallint] NOT NULL,
	[Cliente] [int] NOT NULL,
	[Descuento] [decimal](9, 2) NOT NULL,
	[Usuario] [int] NOT NULL,
	[Fecha] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_mDescuentosCliente] PRIMARY KEY CLUSTERED 
(
	[IdProducto] ASC,
	[Cliente] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[dProductosDescuento]  WITH CHECK ADD  CONSTRAINT [FK_dProductosDescuento_mProveedores] FOREIGN KEY([Usuario])
REFERENCES [dbo].[mProveedores] ([IdPersona])
GO

ALTER TABLE [dbo].[dProductosDescuento] CHECK CONSTRAINT [FK_dProductosDescuento_mProveedores]
GO

ALTER TABLE [dbo].[dProductosDescuento]  WITH CHECK ADD  CONSTRAINT [FK_mDescuentosCliente_mClientes] FOREIGN KEY([Cliente])
REFERENCES [dbo].[mClientes] ([IdPersona])
GO

ALTER TABLE [dbo].[dProductosDescuento] CHECK CONSTRAINT [FK_mDescuentosCliente_mClientes]
GO

ALTER TABLE [dbo].[dProductosDescuento]  WITH CHECK ADD  CONSTRAINT [FK_mDescuentosCliente_mProductos] FOREIGN KEY([IdProducto])
REFERENCES [dbo].[mProductos] ([IdProducto])
GO

ALTER TABLE [dbo].[dProductosDescuento] CHECK CONSTRAINT [FK_mDescuentosCliente_mProductos]
GO

ALTER TABLE [dbo].[dProductosDescuento] ADD  CONSTRAINT [DF_dProductosDescuento_Fecha]  DEFAULT (getdate()) FOR [Fecha]
GO


