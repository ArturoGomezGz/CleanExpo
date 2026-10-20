USE [ExpoMR]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK__AspNetRo__3214EC06F078B934] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[UserId] [nvarchar](128) NOT NULL,
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](128) NOT NULL,
	[UserName] [nvarchar](256) NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[LegacyPasswordHash] [nvarchar](max) NULL,
	[Email] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PhoneNumber] [nvarchar](256) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[LockoutEndDateUtc] [datetime] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[dArticulos]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dArticulos](
	[IDdArticulos] [int] IDENTITY(1,1) NOT NULL,
	[IdArticulo] [int] NOT NULL,
	[NumFoto] [tinyint] NOT NULL,
	[Foto] [varbinary](max) NOT NULL,
 CONSTRAINT [PK_dArticulos] PRIMARY KEY CLUSTERED 
(
	[IDdArticulos] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[dProductosComprados]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dProductosComprados](
	[IdProducto] [smallint] NOT NULL,
	[Cliente] [int] NOT NULL,
 CONSTRAINT [PK_dProductosComprados] PRIMARY KEY CLUSTERED 
(
	[IdProducto] ASC,
	[Cliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[dProductosDescuento]    Script Date: 09/10/2020 11:47:49 p. m. ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[dProductosMax]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dProductosMax](
	[IdProducto] [smallint] NOT NULL,
	[Fecha] [date] NOT NULL,
	[Maximo] [int] NOT NULL,
 CONSTRAINT [PK_dProductosMax] PRIMARY KEY CLUSTERED 
(
	[IdProducto] ASC,
	[Fecha] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[dPromociones]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dPromociones](
	[IdPromocion] [int] NOT NULL,
	[IdArticulo] [int] NOT NULL,
	[Cantidad] [smallint] NOT NULL,
 CONSTRAINT [PK_dPromociones] PRIMARY KEY CLUSTERED 
(
	[IdPromocion] ASC,
	[IdArticulo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[dPromoCliente]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dPromoCliente](
	[IdPromocion] [int] NOT NULL,
	[Cliente] [int] NOT NULL,
 CONSTRAINT [PK_dPromoCliente] PRIMARY KEY CLUSTERED 
(
	[IdPromocion] ASC,
	[Cliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[dStandsMax]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dStandsMax](
	[Proveedor] [int] NOT NULL,
	[Fecha] [date] NOT NULL,
	[Maximo] [int] NOT NULL,
 CONSTRAINT [PK_dStandsMax] PRIMARY KEY CLUSTERED 
(
	[Proveedor] ASC,
	[Fecha] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[lBusqueda]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[lBusqueda](
	[IdLog] [bigint] IDENTITY(1,1) NOT NULL,
	[Cliente] [int] NOT NULL,
	[Proveedor] [int] NOT NULL,
	[Busqueda] [nvarchar](50) NOT NULL,
	[Fecha] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_lBusqueda] PRIMARY KEY CLUSTERED 
(
	[IdLog] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[lSesion]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[lSesion](
	[IdLog] [bigint] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[FeInicioSesion] [smalldatetime] NOT NULL,
	[FeFinSesion] [smalldatetime] NULL,
	[IP] [nvarchar](15) NULL,
	[PlataformName] [nvarchar](20) NULL,
	[BrowserName] [nvarchar](20) NULL,
	[BrowserType] [nvarchar](20) NULL,
	[BrowserVersion] [nvarchar](20) NULL,
	[BrowserPlataform] [nvarchar](20) NULL,
	[BrowserBeta] [bit] NULL,
	[BrowserCrawler] [bit] NULL,
	[BrowserWin16] [bit] NULL,
	[BrowserWin32] [bit] NULL,
 CONSTRAINT [PK_mLog] PRIMARY KEY CLUSTERED 
(
	[IdLog] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[lVisita]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[lVisita](
	[IdLog] [bigint] IDENTITY(1,1) NOT NULL,
	[Cliente] [int] NOT NULL,
	[Proveedor] [int] NULL,
	[IdProducto] [smallint] NULL,
	[Fecha] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_lVisita] PRIMARY KEY CLUSTERED 
(
	[IdLog] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mAnuncios]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mAnuncios](
	[IdAnuncio] [int] IDENTITY(1,1) NOT NULL,
	[Proveedor] [int] NOT NULL,
	[Anuncio] [nvarchar](250) NOT NULL,
	[FeInicio] [smalldatetime] NOT NULL,
	[FeTermino] [smalldatetime] NOT NULL,
	[Rank] [tinyint] NOT NULL,
 CONSTRAINT [PK_mAnuncios] PRIMARY KEY CLUSTERED 
(
	[IdAnuncio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mArticulos]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mArticulos](
	[IdArticulo] [int] IDENTITY(1,1) NOT NULL,
	[Proveedor] [int] NOT NULL,
	[Nombre] [nvarchar](50) NOT NULL,
	[Descripcion] [nvarchar](250) NULL,
 CONSTRAINT [PK_mArticulos] PRIMARY KEY CLUSTERED 
(
	[IdArticulo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mCalendario]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mCalendario](
	[Fecha] [date] NOT NULL,
	[VIP] [bit] NOT NULL,
 CONSTRAINT [PK_mCalendario] PRIMARY KEY CLUSTERED 
(
	[Fecha] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mClientes]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mClientes](
	[IdPersona] [int] NOT NULL,
	[Nombre] [nvarchar](150) NOT NULL,
	[IdDocumento] [tinyint] NOT NULL,
	[IdUsoCFDI] [tinyint] NOT NULL,
	[IdTipoVenta] [tinyint] NOT NULL,
	[Fiscal] [int] NOT NULL,
	[Entrega] [int] NOT NULL,
	[IdTransporte] [int] NULL,
	[IdPorte] [tinyint] NULL,
	[VIP] [bit] NOT NULL,
 CONSTRAINT [PK_mClientes] PRIMARY KEY CLUSTERED 
(
	[IdPersona] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mCotizaciones]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mCotizaciones](
	[Cliente] [int] NOT NULL,
	[IdProducto] [smallint] NOT NULL,
	[Unitario] [real] NOT NULL,
	[Descuento] [decimal](9, 2) NULL,
	[Cantidad] [int] NOT NULL,
	[Solicitado] [bit] NOT NULL,
	[PorConfirmar] [bit] NOT NULL,
	[Usuario] [int] NOT NULL,
	[Fecha] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_mCotizacion] PRIMARY KEY CLUSTERED 
(
	[Cliente] ASC,
	[IdProducto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mJuego]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mJuego](
	[IdJuego] [int] IDENTITY(1,1) NOT NULL,
	[IdRuleta] [tinyint] NOT NULL,
	[GradosGiro] [decimal](9, 1) NOT NULL,
	[IdPremio] [smallint] NOT NULL,
	[Cliente] [int] NULL,
 CONSTRAINT [PK_mJuego] PRIMARY KEY CLUSTERED 
(
	[IdJuego] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mLaboratorios]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mLaboratorios](
	[IdLaboratorio] [int] NOT NULL,
	[Laboratorio] [nvarchar](30) NOT NULL,
	[Proveedor] [int] NOT NULL,
 CONSTRAINT [PK_mLaboratorios] PRIMARY KEY CLUSTERED 
(
	[IdLaboratorio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mMonedero]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mMonedero](
	[IdPersona] [int] NOT NULL,
	[Saldo] [int] NOT NULL,
 CONSTRAINT [PK_mMonedero] PRIMARY KEY CLUSTERED 
(
	[IdPersona] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mMotivos]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mMotivos](
	[IdMotivo] [tinyint] NOT NULL,
	[Motivo] [nvarchar](25) NOT NULL,
	[MonedasExpo] [int] NULL,
	[MonedasVIP] [int] NULL,
 CONSTRAINT [PK_mMotivos] PRIMARY KEY CLUSTERED 
(
	[IdMotivo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mPedidos]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mPedidos](
	[Cliente] [int] NOT NULL,
	[IdProducto] [smallint] NOT NULL,
	[Unitario] [real] NOT NULL,
	[Descuento] [decimal](9, 2) NOT NULL,
	[Cantidad] [int] NOT NULL,
	[Usuario] [int] NOT NULL,
	[Fecha] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_mPedido] PRIMARY KEY CLUSTERED 
(
	[Cliente] ASC,
	[IdProducto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mPremios]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mPremios](
	[IdPremio] [smallint] NOT NULL,
	[Premio] [nvarchar](25) NOT NULL,
	[Descripcion] [nvarchar](100) NULL,
	[IdRuleta] [tinyint] NOT NULL,
	[Seccion] [tinyint] NOT NULL,
	[Foto] [varbinary](max) NULL,
	[Cantidad] [smallint] NULL,
 CONSTRAINT [PK_mPremios] PRIMARY KEY CLUSTERED 
(
	[IdPremio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mProductos]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mProductos](
	[IdProducto] [smallint] NOT NULL,
	[IdLaboratorio] [int] NOT NULL,
	[Iva] [bit] NOT NULL,
	[Rank] [tinyint] NULL,
	[Descuento] [tinyint] NOT NULL,
	[Costo] [decimal](9, 2) NOT NULL,
	[Especial] [bit] NOT NULL,
 CONSTRAINT [PK_mProductos] PRIMARY KEY CLUSTERED 
(
	[IdProducto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mPromociones]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mPromociones](
	[IdPromocion] [int] IDENTITY(1,1) NOT NULL,
	[Proveedor] [int] NOT NULL,
	[Cliente] [int] NULL,
	[Monto] [real] NOT NULL,
	[NumPromos] [smallint] NULL,
	[Activa] [bit] NOT NULL,
	[Usuario] [int] NOT NULL,
	[Fecha] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_mPromociones] PRIMARY KEY CLUSTERED 
(
	[IdPromocion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mProveedores]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mProveedores](
	[IdPersona] [int] NOT NULL,
	[Nombre] [nvarchar](50) NOT NULL,
	[Descripcion] [nvarchar](250) NULL,
 CONSTRAINT [PK_mProveedores] PRIMARY KEY CLUSTERED 
(
	[IdPersona] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mr]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mr](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[I] [int] NULL,
	[vueltas] [int] NULL,
	[IdRuleta] [tinyint] NULL,
	[giro] [decimal](9, 1) NULL,
	[gradosgiro] [decimal](9, 1) NULL,
	[Seccion] [tinyint] NULL,
	[IdPremio] [smallint] NULL,
	[borrar] [bit] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mRuleta]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mRuleta](
	[IdRuleta] [tinyint] NOT NULL,
	[Ruleta] [nvarchar](10) NOT NULL,
	[Monedas] [int] NOT NULL,
 CONSTRAINT [PK_mRuleta] PRIMARY KEY CLUSTERED 
(
	[IdRuleta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mStands]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mStands](
	[Proveedor] [int] NOT NULL,
	[Template] [tinyint] NOT NULL,
	[Rank] [tinyint] NOT NULL,
	[Chat] [nvarchar](100) NULL,
	[Paleta] [tinyint] NOT NULL,
	[Logo] [varbinary](max) NULL,
	[Background] [varbinary](max) NULL,
 CONSTRAINT [PK_mStands] PRIMARY KEY CLUSTERED 
(
	[Proveedor] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mSysVar]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mSysVar](
	[Variable] [nvarchar](20) NOT NULL,
	[Valor] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_mSysVar_1] PRIMARY KEY CLUSTERED 
(
	[Variable] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mTipoUsuario]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mTipoUsuario](
	[IdTipoUsuario] [tinyint] NOT NULL,
	[TipoUsuario] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_mTipoUsuario] PRIMARY KEY CLUSTERED 
(
	[IdTipoUsuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mTransacciones]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mTransacciones](
	[IdTransaccion] [int] IDENTITY(1,1) NOT NULL,
	[Origen] [int] NOT NULL,
	[SaldoOrigen] [int] NOT NULL,
	[Destino] [int] NOT NULL,
	[SaldoDestino] [int] NOT NULL,
	[Monedas] [int] NOT NULL,
	[IdMotivo] [tinyint] NOT NULL,
	[IdProducto] [smallint] NULL,
	[Usuario] [int] NOT NULL,
	[Fecha] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_mTransacciones] PRIMARY KEY CLUSTERED 
(
	[IdTransaccion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mUsuarios]    Script Date: 09/10/2020 11:47:49 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mUsuarios](
	[IdPersona] [int] NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[IdTipoUsuario] [tinyint] NOT NULL,
	[Referencia] [int] NOT NULL,
	[Sucursal] [tinyint] NOT NULL,
	[Link] [nvarchar](9) NULL,
 CONSTRAINT [PK_mUsuarios] PRIMARY KEY CLUSTERED 
(
	[IdPersona] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[dProductosDescuento] ADD  CONSTRAINT [DF_dProductosDescuento_Fecha]  DEFAULT (getdate()) FOR [Fecha]
GO
ALTER TABLE [dbo].[lBusqueda] ADD  CONSTRAINT [DF_lBusqueda_Fecha]  DEFAULT (getdate()) FOR [Fecha]
GO
ALTER TABLE [dbo].[lVisita] ADD  CONSTRAINT [DF_lVisita_Fecha]  DEFAULT (getdate()) FOR [Fecha]
GO
ALTER TABLE [dbo].[mCalendario] ADD  CONSTRAINT [DF_mCalendario_VIP]  DEFAULT ((0)) FOR [VIP]
GO
ALTER TABLE [dbo].[mClientes] ADD  CONSTRAINT [DF_mClientes_VIP]  DEFAULT ((0)) FOR [VIP]
GO
ALTER TABLE [dbo].[mCotizaciones] ADD  CONSTRAINT [DF_mCotizaciones_Solicitado]  DEFAULT ((1)) FOR [Solicitado]
GO
ALTER TABLE [dbo].[mCotizaciones] ADD  CONSTRAINT [DF_mCotizaciones_PorConfirmar]  DEFAULT ((0)) FOR [PorConfirmar]
GO
ALTER TABLE [dbo].[mCotizaciones] ADD  CONSTRAINT [DF_mCotizaciones_Fecha]  DEFAULT (getdate()) FOR [Fecha]
GO
ALTER TABLE [dbo].[mPedidos] ADD  CONSTRAINT [DF_mPedidos_Fecha]  DEFAULT (getdate()) FOR [Fecha]
GO
ALTER TABLE [dbo].[mProductos] ADD  CONSTRAINT [DF_mProductos_Descuento]  DEFAULT ((0)) FOR [Descuento]
GO
ALTER TABLE [dbo].[mProductos] ADD  CONSTRAINT [DF_mProductos_Especial]  DEFAULT ((0)) FOR [Especial]
GO
ALTER TABLE [dbo].[mPromociones] ADD  CONSTRAINT [DF_mPromociones_Activa]  DEFAULT ((1)) FOR [Activa]
GO
ALTER TABLE [dbo].[mPromociones] ADD  CONSTRAINT [DF_mPromociones_Fecha]  DEFAULT (getdate()) FOR [Fecha]
GO
ALTER TABLE [dbo].[mr] ADD  CONSTRAINT [DF_mr_borrar]  DEFAULT ((0)) FOR [borrar]
GO
ALTER TABLE [dbo].[mStands] ADD  CONSTRAINT [DF_mStands_Template]  DEFAULT ((0)) FOR [Template]
GO
ALTER TABLE [dbo].[mStands] ADD  CONSTRAINT [DF_mStands_Rank]  DEFAULT ((0)) FOR [Rank]
GO
ALTER TABLE [dbo].[mStands] ADD  CONSTRAINT [DF_mStands_Paleta]  DEFAULT ((1)) FOR [Paleta]
GO
ALTER TABLE [dbo].[mTransacciones] ADD  CONSTRAINT [DF_mTransacciones_Fecha]  DEFAULT (getdate()) FOR [Fecha]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_User_Id] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_User_Id]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[dArticulos]  WITH CHECK ADD  CONSTRAINT [FK_dArticulos_mArticulos] FOREIGN KEY([IdArticulo])
REFERENCES [dbo].[mArticulos] ([IdArticulo])
GO
ALTER TABLE [dbo].[dArticulos] CHECK CONSTRAINT [FK_dArticulos_mArticulos]
GO
ALTER TABLE [dbo].[dProductosComprados]  WITH CHECK ADD  CONSTRAINT [FK_dProductosComprados_mClientes] FOREIGN KEY([Cliente])
REFERENCES [dbo].[mClientes] ([IdPersona])
GO
ALTER TABLE [dbo].[dProductosComprados] CHECK CONSTRAINT [FK_dProductosComprados_mClientes]
GO
ALTER TABLE [dbo].[dProductosComprados]  WITH CHECK ADD  CONSTRAINT [FK_dProductosComprados_mProductos] FOREIGN KEY([IdProducto])
REFERENCES [dbo].[mProductos] ([IdProducto])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[dProductosComprados] CHECK CONSTRAINT [FK_dProductosComprados_mProductos]
GO
ALTER TABLE [dbo].[dProductosDescuento]  WITH CHECK ADD  CONSTRAINT [FK_dProductosDescuento_mUsuarios] FOREIGN KEY([Usuario])
REFERENCES [dbo].[mUsuarios] ([IdPersona])
GO
ALTER TABLE [dbo].[dProductosDescuento] CHECK CONSTRAINT [FK_dProductosDescuento_mUsuarios]
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
ALTER TABLE [dbo].[dProductosMax]  WITH CHECK ADD  CONSTRAINT [FK_dProductosMax_mCalendario] FOREIGN KEY([Fecha])
REFERENCES [dbo].[mCalendario] ([Fecha])
GO
ALTER TABLE [dbo].[dProductosMax] CHECK CONSTRAINT [FK_dProductosMax_mCalendario]
GO
ALTER TABLE [dbo].[dProductosMax]  WITH CHECK ADD  CONSTRAINT [FK_dProductosMax_mProductos] FOREIGN KEY([IdProducto])
REFERENCES [dbo].[mProductos] ([IdProducto])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[dProductosMax] CHECK CONSTRAINT [FK_dProductosMax_mProductos]
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
ALTER TABLE [dbo].[dStandsMax]  WITH CHECK ADD  CONSTRAINT [FK_dStandsMax_mCalendario] FOREIGN KEY([Fecha])
REFERENCES [dbo].[mCalendario] ([Fecha])
GO
ALTER TABLE [dbo].[dStandsMax] CHECK CONSTRAINT [FK_dStandsMax_mCalendario]
GO
ALTER TABLE [dbo].[dStandsMax]  WITH CHECK ADD  CONSTRAINT [FK_dStandsMax_mStands] FOREIGN KEY([Proveedor])
REFERENCES [dbo].[mStands] ([Proveedor])
GO
ALTER TABLE [dbo].[dStandsMax] CHECK CONSTRAINT [FK_dStandsMax_mStands]
GO
ALTER TABLE [dbo].[mAnuncios]  WITH CHECK ADD  CONSTRAINT [FK_mAnuncios_mProveedores1] FOREIGN KEY([Proveedor])
REFERENCES [dbo].[mProveedores] ([IdPersona])
GO
ALTER TABLE [dbo].[mAnuncios] CHECK CONSTRAINT [FK_mAnuncios_mProveedores1]
GO
ALTER TABLE [dbo].[mArticulos]  WITH CHECK ADD  CONSTRAINT [FK_mArticulos_mProveedores1] FOREIGN KEY([Proveedor])
REFERENCES [dbo].[mProveedores] ([IdPersona])
GO
ALTER TABLE [dbo].[mArticulos] CHECK CONSTRAINT [FK_mArticulos_mProveedores1]
GO
ALTER TABLE [dbo].[mCotizaciones]  WITH CHECK ADD  CONSTRAINT [FK_mCotizacion_mClientes] FOREIGN KEY([Cliente])
REFERENCES [dbo].[mClientes] ([IdPersona])
GO
ALTER TABLE [dbo].[mCotizaciones] CHECK CONSTRAINT [FK_mCotizacion_mClientes]
GO
ALTER TABLE [dbo].[mCotizaciones]  WITH CHECK ADD  CONSTRAINT [FK_mCotizaciones_mProductos] FOREIGN KEY([IdProducto])
REFERENCES [dbo].[mProductos] ([IdProducto])
GO
ALTER TABLE [dbo].[mCotizaciones] CHECK CONSTRAINT [FK_mCotizaciones_mProductos]
GO
ALTER TABLE [dbo].[mCotizaciones]  WITH CHECK ADD  CONSTRAINT [FK_mCotizaciones_mUsuarios] FOREIGN KEY([Usuario])
REFERENCES [dbo].[mUsuarios] ([IdPersona])
GO
ALTER TABLE [dbo].[mCotizaciones] CHECK CONSTRAINT [FK_mCotizaciones_mUsuarios]
GO
ALTER TABLE [dbo].[mJuego]  WITH CHECK ADD  CONSTRAINT [FK_mJuego_mPremios] FOREIGN KEY([IdPremio])
REFERENCES [dbo].[mPremios] ([IdPremio])
GO
ALTER TABLE [dbo].[mJuego] CHECK CONSTRAINT [FK_mJuego_mPremios]
GO
ALTER TABLE [dbo].[mJuego]  WITH CHECK ADD  CONSTRAINT [FK_mJuego_mRuleta] FOREIGN KEY([IdRuleta])
REFERENCES [dbo].[mRuleta] ([IdRuleta])
GO
ALTER TABLE [dbo].[mJuego] CHECK CONSTRAINT [FK_mJuego_mRuleta]
GO
ALTER TABLE [dbo].[mLaboratorios]  WITH CHECK ADD  CONSTRAINT [FK_mLaboratorios_mProveedores] FOREIGN KEY([Proveedor])
REFERENCES [dbo].[mProveedores] ([IdPersona])
GO
ALTER TABLE [dbo].[mLaboratorios] CHECK CONSTRAINT [FK_mLaboratorios_mProveedores]
GO
ALTER TABLE [dbo].[mPedidos]  WITH CHECK ADD  CONSTRAINT [FK_mPedido_mClientes] FOREIGN KEY([Cliente])
REFERENCES [dbo].[mClientes] ([IdPersona])
GO
ALTER TABLE [dbo].[mPedidos] CHECK CONSTRAINT [FK_mPedido_mClientes]
GO
ALTER TABLE [dbo].[mPedidos]  WITH CHECK ADD  CONSTRAINT [FK_mPedidos_mProductos] FOREIGN KEY([IdProducto])
REFERENCES [dbo].[mProductos] ([IdProducto])
GO
ALTER TABLE [dbo].[mPedidos] CHECK CONSTRAINT [FK_mPedidos_mProductos]
GO
ALTER TABLE [dbo].[mPedidos]  WITH CHECK ADD  CONSTRAINT [FK_mPedidos_mUsuarios] FOREIGN KEY([Usuario])
REFERENCES [dbo].[mUsuarios] ([IdPersona])
GO
ALTER TABLE [dbo].[mPedidos] CHECK CONSTRAINT [FK_mPedidos_mUsuarios]
GO
ALTER TABLE [dbo].[mPremios]  WITH CHECK ADD  CONSTRAINT [FK_mPremios_mRuleta] FOREIGN KEY([IdRuleta])
REFERENCES [dbo].[mRuleta] ([IdRuleta])
GO
ALTER TABLE [dbo].[mPremios] CHECK CONSTRAINT [FK_mPremios_mRuleta]
GO
ALTER TABLE [dbo].[mProductos]  WITH CHECK ADD  CONSTRAINT [FK_mProductos_mLaboratorios] FOREIGN KEY([IdLaboratorio])
REFERENCES [dbo].[mLaboratorios] ([IdLaboratorio])
GO
ALTER TABLE [dbo].[mProductos] CHECK CONSTRAINT [FK_mProductos_mLaboratorios]
GO
ALTER TABLE [dbo].[mPromociones]  WITH CHECK ADD  CONSTRAINT [FK_mPromociones_mClientes] FOREIGN KEY([Cliente])
REFERENCES [dbo].[mClientes] ([IdPersona])
GO
ALTER TABLE [dbo].[mPromociones] CHECK CONSTRAINT [FK_mPromociones_mClientes]
GO
ALTER TABLE [dbo].[mPromociones]  WITH CHECK ADD  CONSTRAINT [FK_mPromociones_mProveedores] FOREIGN KEY([Proveedor])
REFERENCES [dbo].[mProveedores] ([IdPersona])
GO
ALTER TABLE [dbo].[mPromociones] CHECK CONSTRAINT [FK_mPromociones_mProveedores]
GO
ALTER TABLE [dbo].[mPromociones]  WITH CHECK ADD  CONSTRAINT [FK_mPromociones_mUsuarios] FOREIGN KEY([Usuario])
REFERENCES [dbo].[mUsuarios] ([IdPersona])
GO
ALTER TABLE [dbo].[mPromociones] CHECK CONSTRAINT [FK_mPromociones_mUsuarios]
GO
ALTER TABLE [dbo].[mStands]  WITH CHECK ADD  CONSTRAINT [FK_mStands_mProveedores] FOREIGN KEY([Proveedor])
REFERENCES [dbo].[mProveedores] ([IdPersona])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[mStands] CHECK CONSTRAINT [FK_mStands_mProveedores]
GO
ALTER TABLE [dbo].[mTransacciones]  WITH CHECK ADD  CONSTRAINT [FK_mTransacciones_mMotivos] FOREIGN KEY([IdMotivo])
REFERENCES [dbo].[mMotivos] ([IdMotivo])
GO
ALTER TABLE [dbo].[mTransacciones] CHECK CONSTRAINT [FK_mTransacciones_mMotivos]
GO
ALTER TABLE [dbo].[mTransacciones]  WITH CHECK ADD  CONSTRAINT [FK_mTransacciones_mProductos] FOREIGN KEY([IdProducto])
REFERENCES [dbo].[mProductos] ([IdProducto])
GO
ALTER TABLE [dbo].[mTransacciones] CHECK CONSTRAINT [FK_mTransacciones_mProductos]
GO
ALTER TABLE [dbo].[mUsuarios]  WITH CHECK ADD  CONSTRAINT [FK_mUsuarios_mTipoUsuario] FOREIGN KEY([IdTipoUsuario])
REFERENCES [dbo].[mTipoUsuario] ([IdTipoUsuario])
GO
ALTER TABLE [dbo].[mUsuarios] CHECK CONSTRAINT [FK_mUsuarios_mTipoUsuario]
GO
