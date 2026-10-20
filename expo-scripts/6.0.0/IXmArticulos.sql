USE [ExpoMR]
GO

/****** Object:  Index [IXmArticulos]    Script Date: 03/03/2025 11:47:42 a. m. ******/
CREATE NONCLUSTERED INDEX [IXmArticulos] ON [dbo].[mArticulos]
(
	[Proveedor] ASC,
	[IdArticulo] ASC
)
INCLUDE([Nombre],[Descripcion],[Cantidad]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO


