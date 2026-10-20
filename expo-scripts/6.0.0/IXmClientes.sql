USE [ExpoMR]
GO

/****** Object:  Index [IXmClientes]    Script Date: 03/03/2025 03:34:15 p. m. ******/
CREATE NONCLUSTERED INDEX [IXmClientes] ON [dbo].[mClientes]
(
	[CreditoExcedido] ASC,
	[Sucursal] ASC,
	[IdPersona] ASC
)
INCLUDE([Nombre],[IdDocumento],[IdUsoCFDI],[IdTipoVenta],[Fiscal],[Entrega],[IdTransporte],[IdPorte],[Credito]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO