USE [ExpoMR]
GO

/****** Object:  Index [IXmProductos]    Script Date: 03/03/2025 11:56:19 a. m. ******/
CREATE NONCLUSTERED INDEX [IXmProductos] ON [dbo].[mProductos]
(
	[IdLaboratorio] ASC,
	[IdProducto] ASC
)
INCLUDE([Iva],[Rank],[Descuento],[Especial]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

