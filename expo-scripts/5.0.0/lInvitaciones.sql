USE [ExpoMR]
GO

/****** Object:  Table [dbo].[lInvitaciones]    Script Date: 13/05/2024 10:51:10 a. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[lInvitaciones](
	[IdInvitacion] [int] IDENTITY(1,1) NOT NULL,
	[IdEmail] [int] NOT NULL,
	[Tipo] [bit] NOT NULL,
	[Msg] [nvarchar](max) NOT NULL,
	[Fecha] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_lInvitaciones] PRIMARY KEY CLUSTERED 
(
	[IdInvitacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[lInvitaciones] ADD  CONSTRAINT [DF_lInvitaciones_Tipo]  DEFAULT ((0)) FOR [Tipo]
GO


