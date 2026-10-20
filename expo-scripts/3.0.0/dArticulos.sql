USE [ExpoMR]
GO

/****** Object:  Table [dbo].[dArticulos]    Script Date: 11/03/2022 10:17:50 a. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[dArticulos](
	[IDdArticulos] [int] IDENTITY(1,1) NOT NULL,
	[IdArticulo] [smallint] NOT NULL,
	[NumFoto] [tinyint] NOT NULL,
	[Foto] [varbinary](max) NOT NULL,
 CONSTRAINT [PK_dArticulos] PRIMARY KEY CLUSTERED 
(
	[IDdArticulos] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[dArticulos]  WITH CHECK ADD  CONSTRAINT [FK_dArticulos_mArticulos] FOREIGN KEY([IdArticulo])
REFERENCES [dbo].[mArticulos] ([IdArticulo])
GO

ALTER TABLE [dbo].[dArticulos] CHECK CONSTRAINT [FK_dArticulos_mArticulos]
GO


