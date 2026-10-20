USE [ExpoMR]
GO

/****** Object:  View [dbo].[vSurtido]    Script Date: 17/10/2025 03:22:33 p. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER VIEW [dbo].[vSurtido] AS

WITH Pedidos AS (
					SELECT vPedidos.IdPedido, vPedidos.Cliente, vPedidos.IdProducto, vPedidos.PorSurtir, vPedidos.PrecioIva AS Precio,
					(SELECT SUM(PedidoAcumulado.PorSurtir) 
					 FROM vPedidos AS PedidoAcumulado 
					 INNER JOIN mClientes ON PedidoAcumulado.Cliente = mClientes.IdPersona
					 WHERE mClientes.Sucursal = 1	--1 = GDL
					 AND mClientes.CreditoExcedido = 0
					 AND PedidoAcumulado.PorSurtir > 0
					 AND PedidoAcumulado.IdProducto = vPedidos.IdProducto
					 AND PedidoAcumulado.IdPedido <= vPedidos.IdPedido) AS Acumulado,
					mProdVenta.Cantidad AS Inventario, mClientes.Sucursal
					FROM vPedidos INNER JOIN
					mClientes ON vPedidos.Cliente = mClientes.IdPersona INNER JOIN
					Indigo.dbo.mProdVenta ON vPedidos.IdProducto = mProdVenta.IdProducto
					WHERE mClientes.CreditoExcedido = 0
					AND mClientes.Sucursal = 1 	--1 = GDL
					AND vPedidos.PorSurtir > 0
					AND mProdVenta.IdCondicion = 1 --1 = NORMAL
					UNION
					SELECT vPedidos.IdPedido, vPedidos.Cliente, vPedidos.IdProducto, vPedidos.PorSurtir, vPedidos.PrecioIva AS Precio,
				    (SELECT SUM(PedidoAcumulado.PorSurtir) 
					 FROM vPedidos AS PedidoAcumulado 
					 INNER JOIN mClientes ON PedidoAcumulado.Cliente = mClientes.IdPersona
					 WHERE mClientes.Sucursal = 2 --2 = QTRO
					 AND mClientes.CreditoExcedido = 0
					 AND PedidoAcumulado.PorSurtir > 0
					 AND PedidoAcumulado.IdProducto = vPedidos.IdProducto
					 AND PedidoAcumulado.IdPedido <= vPedidos.IdPedido) AS Acumulado,
					mProdVenta.Cantidad AS Inventario, mClientes.Sucursal
					FROM vPedidos INNER JOIN
					mClientes ON vPedidos.Cliente = mClientes.IdPersona INNER JOIN
					QtroIndigo.dbo.mProdVenta ON vPedidos.IdProducto = mProdVenta.IdProducto
					WHERE mClientes.CreditoExcedido = 0
					AND mClientes.Sucursal = 2 --2 = QTRO
					AND vPedidos.PorSurtir > 0
					AND mProdVenta.IdCondicion = 1 --1 = NORMAL
					UNION
					SELECT vPedidos.IdPedido, vPedidos.Cliente, vPedidos.IdProducto, vPedidos.PorSurtir, vPedidos.PrecioIva AS Precio,
					(SELECT SUM(PedidoAcumulado.PorSurtir) 
					 FROM vPedidos AS PedidoAcumulado 
					 INNER JOIN mClientes ON PedidoAcumulado.Cliente = mClientes.IdPersona
					 WHERE mClientes.Sucursal = 3  --3 = MEX
					 AND mClientes.CreditoExcedido = 0
					 AND PedidoAcumulado.PorSurtir > 0
					 AND PedidoAcumulado.IdProducto = vPedidos.IdProducto
					 AND PedidoAcumulado.IdPedido <= vPedidos.IdPedido) AS Acumulado,
					 mProdVenta.Cantidad AS Inventario, mClientes.Sucursal
					FROM vPedidos INNER JOIN 
					mClientes ON vPedidos.Cliente = mClientes.IdPersona INNER JOIN
					EdomxIndigo.dbo.mProdVenta ON vPedidos.IdProducto = mProdVenta.IdProducto
					WHERE mClientes.CreditoExcedido = 0
					AND mClientes.Sucursal = 3 --3 = MEX
					AND vPedidos.PorSurtir > 0
					AND mProdVenta.IdCondicion = 1 --1 = NORMAL
), 

Acumulado AS (
				SELECT Pedidos.IdPedido, Pedidos.Sucursal, Pedidos.Cliente, Pedidos.IdProducto, Pedidos.Precio,
					CASE 
						WHEN Pedidos.Inventario >= Pedidos.Acumulado 
						THEN Pedidos.PorSurtir 
						ELSE Pedidos.Inventario - (Pedidos.Acumulado - Pedidos.PorSurtir) 
					END AS Cantidad,
					ROW_NUMBER() OVER (PARTITION BY Pedidos.Cliente ORDER BY Pedidos.IdPedido) AS RowNum
				FROM Pedidos
				WHERE Pedidos.Inventario - (Pedidos.Acumulado - Pedidos.PorSurtir) > 0
)

SELECT IdPedido, Sucursal, Cliente, IdProducto, Precio, Cantidad, ((RowNum - 1) / 32) + 1 AS Factura
FROM Acumulado;
GO


