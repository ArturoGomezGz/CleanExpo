USE [ExpoMR]
GO

/****** Object:  Table [dbo].[dPromoCliente]    Script Date: 11/03/2022 09:37:37 a. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[dPromoCliente](
	[IdPromocion] [smallint] NOT NULL,
	[Cliente] [int] NOT NULL,
	[Cantidad] [smallint] NOT NULL,
 CONSTRAINT [PK_dPromoCliente] PRIMARY KEY CLUSTERED 
(
	[IdPromocion] ASC,
	[Cliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[dPromoCliente]  WITH CHECK ADD  CONSTRAINT [FK_dPromoCliente_mClientes] FOREIGN KEY([Cliente])
REFERENCES [dbo].[mClientes] ([IdPersona])
GO

ALTER TABLE [dbo].[dPromoCliente] CHECK CONSTRAINT [FK_dPromoCliente_mClientes]
GO

ALTER TABLE [dbo].[dPromoCliente]  WITH CHECK ADD  CONSTRAINT [FK_dPromoCliente_mPromociones] FOREIGN KEY([IdPromocion])
REFERENCES [dbo].[mPromociones] ([IdPromocion])
GO

ALTER TABLE [dbo].[dPromoCliente] CHECK CONSTRAINT [FK_dPromoCliente_mPromociones]
GO


