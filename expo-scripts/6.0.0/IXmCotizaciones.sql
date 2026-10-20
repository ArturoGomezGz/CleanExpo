USE [ExpoMR]
GO

/****** Object:  Index [IXmCotizaciones]    Script Date: 03/03/2025 12:17:19 p. m. ******/
CREATE NONCLUSTERED INDEX [IXmCotizaciones] ON [dbo].[mCotizaciones]
(
	[Cliente] ASC,
	[IdCotizacion] ASC
)
INCLUDE([IdDocumento],[IdUsoCFDI],[IdTipoVenta],[IdTransporte],[IdPorte],[Fiscal],[Entrega],[Obsv],[IdEmail],[Fecha]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
