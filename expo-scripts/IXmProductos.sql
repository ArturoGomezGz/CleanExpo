USE [ExpoMR]
GO

DROP INDEX IXmProductos ON [mProductos]

GO

/****** Object:  Index [IXmProductos]    Script Date: 22/10/2025 01:00:48 p. m. ******/
CREATE NONCLUSTERED INDEX [IXmProductos] ON [dbo].[mProductos]
(
	[IdLaboratorio] ASC,
	[IdProducto] ASC
)
INCLUDE([Iva],[Rank],[Descuento],[Especial]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO


