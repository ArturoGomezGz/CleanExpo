USE [ExpoMR]
GO

/****** Object:  View [dbo].[vSurtido]    Script Date: 03/03/2025 03:45:32 p. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[vSurtido] AS

--Select Partidas.IdPedido, Partidas.Sucursal, Partidas.Cliente, Partidas.VIP, Partidas.IdProducto, Partidas.Precio,
--Case When Partidas.Inventario >=  Partidas.Acumulado Then Partidas.PorSurtir 
--		Else Partidas.Inventario - (Partidas.Acumulado - Partidas.PorSurtir) End as Cantidad,
--((ROW_NUMBER() over (partition by Partidas.Cliente order by Partidas.IdPedido) - 1) / 32) + 1 as Factura
--From (
--		Select vPedidos.IdPedido, vPedidos.Cliente, vPedidos.IdProducto, vPedidos.PorSurtir, vPedidos.PrecioIva as Precio,
--			(Select SUM(PedidoAcumulado.PorSurtir) 
--			From vPedidos as PedidoAcumulado Inner Join
--			mClientes on PedidoAcumulado.Cliente = mClientes.IdPersona
--			Where mClientes.Sucursal = 1
--			And mClientes.CreditoExcedido = 0
--			And vPedidos.PorSurtir > 0
--			And PedidoAcumulado.IdProducto = vPedidos.IdProducto
--			And PedidoAcumulado.IdPedido < = vPedidos.IdPedido) as Acumulado,
--		mProdVenta.Cantidad as Inventario,
--		mClientes.Sucursal, mClientes.VIP
--		From vPedidos inner join
--		mClientes on vPedidos.Cliente = mClientes.IdPersona inner join 
--		Indigo.dbo.mProdVenta on vPedidos.IdProducto = mProdVenta.IdProducto
--		Where mProdVenta.IdCondicion = 1
--		and mClientes.CreditoExcedido = 0
--		and mClientes.Sucursal = 1
--		and vPedidos.PorSurtir > 0
--	) as Partidas 
--Where Partidas.Inventario - (Partidas.Acumulado - Partidas.PorSurtir) > 0 
--Union
--Select Partidas.IdPedido, Partidas.Sucursal, Partidas.Cliente, Partidas.VIP, Partidas.IdProducto, Partidas.Precio,
--Case When Partidas.Inventario >=  Partidas.Acumulado Then Partidas.PorSurtir 
--		Else Partidas.Inventario - (Partidas.Acumulado - Partidas.PorSurtir) End as Cantidad,
--((ROW_NUMBER() over (partition by Partidas.Cliente order by Partidas.IdPedido) - 1) / 32) + 1 as Factura
--From (
--		Select vPedidos.IdPedido, vPedidos.Cliente, vPedidos.IdProducto, vPedidos.PorSurtir, vPedidos.PrecioIva as Precio,
--			(Select SUM(PedidoAcumulado.PorSurtir) 
--			From vPedidos as PedidoAcumulado Inner Join
--			mClientes on PedidoAcumulado.Cliente = mClientes.IdPersona
--			Where mClientes.Sucursal = 2
--			And mClientes.CreditoExcedido = 0
--			And vPedidos.PorSurtir > 0
--			And PedidoAcumulado.IdProducto = vPedidos.IdProducto
--			And PedidoAcumulado.IdPedido < = vPedidos.IdPedido) as Acumulado,
--		mProdVenta.Cantidad as Inventario,
--		mClientes.Sucursal, mClientes.VIP
--		From vPedidos inner join
--		mClientes on vPedidos.Cliente = mClientes.IdPersona inner join 
--		QtroIndigo.dbo.mProdVenta on vPedidos.IdProducto = mProdVenta.IdProducto
--		Where mProdVenta.IdCondicion = 1
--		and mClientes.CreditoExcedido = 0
--		and mClientes.Sucursal = 2
--		And vPedidos.PorSurtir > 0		
--	) as Partidas 
--Where Partidas.Inventario - (Partidas.Acumulado - Partidas.PorSurtir) > 0 
--Union
--Select Partidas.IdPedido, Partidas.Sucursal, Partidas.Cliente, Partidas.VIP, Partidas.IdProducto, Partidas.Precio,
--Case When Partidas.Inventario >=  Partidas.Acumulado Then Partidas.PorSurtir 
--		Else Partidas.Inventario - (Partidas.Acumulado - Partidas.PorSurtir) End as Cantidad,
--((ROW_NUMBER() over (partition by Partidas.Cliente order by Partidas.IdPedido) - 1) / 32) + 1 as Factura
--From (
--		Select vPedidos.IdPedido, vPedidos.Cliente, vPedidos.IdProducto, vPedidos.PorSurtir, vPedidos.PrecioIva as Precio,
--			(Select SUM(PedidoAcumulado.PorSurtir) 
--			From vPedidos as PedidoAcumulado Inner Join
--			mClientes on PedidoAcumulado.Cliente = mClientes.IdPersona
--			Where mClientes.Sucursal = 3
--			And mClientes.CreditoExcedido = 0
--			And vPedidos.PorSurtir > 0
--			And PedidoAcumulado.IdProducto = vPedidos.IdProducto
--			And PedidoAcumulado.IdPedido < = vPedidos.IdPedido) as Acumulado,
--		mProdVenta.Cantidad as Inventario,
--		mClientes.Sucursal, mClientes.VIP
--		From vPedidos inner join
--		mClientes on vPedidos.Cliente = mClientes.IdPersona inner join 
--		EdomxIndigo.dbo.mProdVenta on vPedidos.IdProducto = mProdVenta.IdProducto
--		Where mProdVenta.IdCondicion = 1
--		and mClientes.CreditoExcedido = 0
--		and mClientes.Sucursal = 3
--		And vPedidos.PorSurtir > 0		
--	) as Partidas 
--Where Partidas.Inventario - (Partidas.Acumulado - Partidas.PorSurtir) > 0

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
					mProdVenta.Cantidad AS Inventario, mClientes.Sucursal, mClientes.VIP
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
					mProdVenta.Cantidad AS Inventario, mClientes.Sucursal, mClientes.VIP
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
					 mProdVenta.Cantidad AS Inventario, mClientes.Sucursal, mClientes.VIP
					FROM vPedidos INNER JOIN 
					mClientes ON vPedidos.Cliente = mClientes.IdPersona INNER JOIN
					EdomxIndigo.dbo.mProdVenta ON vPedidos.IdProducto = mProdVenta.IdProducto
					WHERE mClientes.CreditoExcedido = 0
					AND mClientes.Sucursal = 3 --3 = MEX
					AND vPedidos.PorSurtir > 0
					AND mProdVenta.IdCondicion = 1 --1 = NORMAL
), 

Acumulado AS (
				SELECT Pedidos.IdPedido, Pedidos.Sucursal, Pedidos.Cliente, Pedidos.VIP, Pedidos.IdProducto, Pedidos.Precio,
					CASE 
						WHEN Pedidos.Inventario >= Pedidos.Acumulado 
						THEN Pedidos.PorSurtir 
						ELSE Pedidos.Inventario - (Pedidos.Acumulado - Pedidos.PorSurtir) 
					END AS Cantidad,
					ROW_NUMBER() OVER (PARTITION BY Pedidos.Cliente ORDER BY Pedidos.IdPedido) AS RowNum
				FROM Pedidos
				WHERE Pedidos.Inventario - (Pedidos.Acumulado - Pedidos.PorSurtir) > 0
)

SELECT IdPedido, Sucursal, Cliente, VIP, IdProducto, Precio, Cantidad, ((RowNum - 1) / 32) + 1 AS Factura
FROM Acumulado;
GO


