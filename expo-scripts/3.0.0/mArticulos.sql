USE [ExpoMR]
GO

/****** Object:  Table [dbo].[mArticulos]    Script Date: 18/02/2022 04:21:14 p. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[mArticulos](
	[IdArticulo] [smallint] IDENTITY(1,1) NOT NULL,
	[Proveedor] [int] NOT NULL,
	[Nombre] [nvarchar](50) NOT NULL,
	[Descripcion] [nvarchar](250) NULL,
	[Cantidad] [smallint] NOT NULL,
 CONSTRAINT [PK_mArticulos] PRIMARY KEY CLUSTERED 
(
	[IdArticulo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[mArticulos]  WITH CHECK ADD  CONSTRAINT [FK_mArticulos_mProveedores] FOREIGN KEY([Proveedor])
REFERENCES [dbo].[mProveedores] ([IdPersona])
GO

ALTER TABLE [dbo].[mArticulos] CHECK CONSTRAINT [FK_mArticulos_mProveedores]
GO


