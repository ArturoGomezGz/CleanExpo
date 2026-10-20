USE [ExpoMR]
GO

/****** Object:  Index [IXmTransacciones]    Script Date: 04/03/2025 07:54:53 a. m. ******/
CREATE NONCLUSTERED INDEX [IXmTransacciones] ON [dbo].[mTransacciones]
(
	[Origen] ASC,
	[Destino] ASC
)
INCLUDE([IdTransaccion],[SaldoOrigen],[SaldoDestino],[Monedas],[IdMotivo],[IdProducto],[Usuario],[Fecha]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO


