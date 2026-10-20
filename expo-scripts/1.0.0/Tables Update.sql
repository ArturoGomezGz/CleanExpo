USE [ExpoMR]
GO
/****** Object:  Table [dbo].[dFacturas]    Script Date: 24/10/2020 10:11:35 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dFacturas](
	[Sucursal] [tinyint] NOT NULL,
	[IdFactura] [int] NOT NULL,
	[IdProducto] [smallint] NOT NULL,
	[Unitario] [real] NOT NULL,
	[ConCargo] [int] NOT NULL,
 CONSTRAINT [PK_dFacturas] PRIMARY KEY CLUSTERED 
(
	[Sucursal] ASC,
	[IdFactura] ASC,
	[IdProducto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[lCredito]    Script Date: 24/10/2020 10:11:35 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[lCredito](
	[IdLog] [int] IDENTITY(1,1) NOT NULL,
	[Cliente] [int] NOT NULL,
	[SurtidoParcial] [bit] NOT NULL,
	[Fecha] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_lCredito] PRIMARY KEY CLUSTERED 
(
	[IdLog] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[lSurtido]    Script Date: 24/10/2020 10:11:35 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[lSurtido](
	[IdLog] [smallint] IDENTITY(1,1) NOT NULL,
	[Step] [tinyint] NOT NULL,
	[Fecha] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_lSurtido] PRIMARY KEY CLUSTERED 
(
	[IdLog] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mFacturas]    Script Date: 24/10/2020 10:11:35 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mFacturas](
	[Sucursal] [tinyint] NOT NULL,
	[IdFactura] [int] NOT NULL,
	[IdPersona] [int] NOT NULL,
	[Fecha] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_mFacturas_1] PRIMARY KEY CLUSTERED 
(
	[Sucursal] ASC,
	[IdFactura] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mSurtido]    Script Date: 24/10/2020 10:11:35 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mSurtido](
	[IdPedido] [int] NOT NULL,
	[Sucursal] [tinyint] NOT NULL,
	[Cliente] [int] NOT NULL,
	[IdProducto] [smallint] NOT NULL,
	[Precio] [real] NOT NULL,
	[Cantidad] [int] NOT NULL,
	[Factura] [smallint] NOT NULL,
	[Surtir] [int] NULL,
 CONSTRAINT [PK_mSurtido] PRIMARY KEY CLUSTERED 
(
	[IdPedido] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[lCredito] ADD  CONSTRAINT [DF_lCredito_Fecha]  DEFAULT (getdate()) FOR [Fecha]
GO
ALTER TABLE [dbo].[lSurtido] ADD  CONSTRAINT [DF_lSurtido_Fecha]  DEFAULT (getdate()) FOR [Fecha]
GO
ALTER TABLE [dbo].[dFacturas]  WITH CHECK ADD  CONSTRAINT [FK_dFacturas_mFacturas] FOREIGN KEY([Sucursal], [IdFactura])
REFERENCES [dbo].[mFacturas] ([Sucursal], [IdFactura])
GO
ALTER TABLE [dbo].[dFacturas] CHECK CONSTRAINT [FK_dFacturas_mFacturas]
GO
BEGIN TRANSACTION
GO
ALTER TABLE dbo.mPedidos ADD
	Surtido int NOT NULL CONSTRAINT DF_mPedidos_Surtido DEFAULT 0
GO
ALTER TABLE dbo.mPedidos SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.mClientes ADD
	Sucursal tinyint NOT NULL CONSTRAINT DF_mClientes_Sucursal DEFAULT 0,
	CreditoExcedido bit NULL
GO
ALTER TABLE dbo.mClientes SET (LOCK_ESCALATION = TABLE)
GO
COMMIT