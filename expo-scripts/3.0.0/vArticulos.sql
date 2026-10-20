USE [ExpoMR]
GO

/****** Object:  View [dbo].[vArticulos]    Script Date: 28/02/2022 11:33:32 a. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW	[dbo].[vArticulos] AS

SELECT mArticulos.IdArticulo, mArticulos.Nombre, mArticulos.Descripcion, mArticulos.Proveedor,
mArticulos.Cantidad - SUM(ISNULL(dPromociones.Cantidad, 0) * ISNULL(dPromoCliente.Cantidad, 0)) AS CantidadDisponible,
mArticulos.Cantidad AS TotalArticulos
FROM mArticulos LEFT OUTER JOIN
dPromociones ON mArticulos.IdArticulo = dPromociones.IdArticulo LEFT OUTER JOIN
dPromoCliente ON dPromociones.IdPromocion = dPromoCliente.IdPromocion
GROUP BY mArticulos.IdArticulo, mArticulos.Nombre, mArticulos.Descripcion, mArticulos.Proveedor, mArticulos.Cantidad

GO


