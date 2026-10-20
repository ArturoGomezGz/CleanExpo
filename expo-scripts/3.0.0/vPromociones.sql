USE [ExpoMR]
GO

/****** Object:  View [dbo].[vPromociones]    Script Date: 01/03/2022 01:11:57 p. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER VIEW	[dbo].[vPromociones] AS

SELECT mPromociones.IdPromocion, mPromociones.IdTipoPromocion, mPromociones.Proveedor, mPromociones.Cliente, dPromoCondicion.Monto, dPromoCondicion.Piezas, dPromoCondicion.IdProducto,
MIN(ISNULL(CAST(vArticulos.CantidadDisponible / NULLIF(dPromociones.Cantidad, 0) AS INT), 0)) AS PosiblesRestantes
FROM mPromociones INNER JOIN
dPromoCondicion ON mPromociones.IdPromocion = dPromoCondicion.IdPromocion INNER JOIN
dPromociones ON mPromociones.IdPromocion = dPromociones.IdPromocion LEFT OUTER JOIN
vArticulos ON dPromociones.IdArticulo = vArticulos.IdArticulo
WHERE mPromociones.Activa = 1 
GROUP BY mPromociones.IdPromocion, mPromociones.IdTipoPromocion, mPromociones.Proveedor, mPromociones.Cliente, dPromoCondicion.Monto, dPromoCondicion.Piezas, 
dPromoCondicion.IdProducto

GO


