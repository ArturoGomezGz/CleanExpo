USE [ExpoMR]
GO

/****** Object:  Table [dbo].[dPromociones]    Script Date: 11/03/2022 09:40:24 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dPromociones](
	[IdPromocion] [smallint] NOT NULL,
	[IdArticulo] [smallint] NOT NULL,
	[Cantidad] [smallint] NOT NULL,
 CONSTRAINT [PK_dPromociones] PRIMARY KEY CLUSTERED 
(
	[IdPromocion] ASC,
	[IdArticulo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[dPromociones]  WITH CHECK ADD  CONSTRAINT [FK_dPromociones_mArticulos] FOREIGN KEY([IdArticulo])
REFERENCES [dbo].[mArticulos] ([IdArticulo])
GO

ALTER TABLE [dbo].[dPromociones] CHECK CONSTRAINT [FK_dPromociones_mArticulos]
GO

ALTER TABLE [dbo].[dPromociones]  WITH CHECK ADD  CONSTRAINT [FK_dPromociones_mPromociones] FOREIGN KEY([IdPromocion])
REFERENCES [dbo].[mPromociones] ([IdPromocion])
GO

ALTER TABLE [dbo].[dPromociones] CHECK CONSTRAINT [FK_dPromociones_mPromociones]
GO


