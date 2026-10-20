USE [ExpoMR]
GO

/****** Object:  Table [dbo].[mUsuarios]    Script Date: 13/05/2024 10:50:25 a. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[mUsuarios](
	[IdUsuario] [int] IDENTITY(1,1) NOT NULL,
	[IdEmail] [int] NOT NULL,
	[IdTipoUsuario] [tinyint] NOT NULL,
	[IdPersona] [int] NOT NULL,
	[Sucursal] [tinyint] NOT NULL,
	[Link] [nvarchar](9) NULL,
	[FeUltimaNotificacion] [smalldatetime] NULL,
 CONSTRAINT [PK_mUsuarios] PRIMARY KEY CLUSTERED 
(
	[IdUsuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[mUsuarios]  WITH CHECK ADD  CONSTRAINT [FK_mUsuarios_mTipoUsuario] FOREIGN KEY([IdTipoUsuario])
REFERENCES [dbo].[mTipoUsuario] ([IdTipoUsuario])
GO

ALTER TABLE [dbo].[mUsuarios] CHECK CONSTRAINT [FK_mUsuarios_mTipoUsuario]
GO


