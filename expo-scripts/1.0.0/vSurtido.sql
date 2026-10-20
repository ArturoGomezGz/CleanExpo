Use [ExpoMR]
GO
if exists(select * from sysobjects where name = 'vSurtido')
	drop view vSurtido
GO
CREATE VIEW vSurtido AS

Select Partidas.IdPedido, Partidas.Sucursal, Partidas.Cliente, Partidas.VIP, Partidas.IdProducto, Partidas.Precio,
Case When Partidas.Inventario >=  Partidas.Acumulado Then Partidas.PorSurtir 
		Else Partidas.Inventario - (Partidas.Acumulado - Partidas.PorSurtir) End as Cantidad,
((ROW_NUMBER() over (partition by Partidas.Cliente order by Partidas.IdPedido) - 1) / 32) + 1 as Factura
From (
		Select vPedidos.IdPedido, vPedidos.Cliente, vPedidos.IdProducto, vPedidos.PorSurtir, vPedidos.PrecioIva as Precio,
			(Select SUM(PedidoAcumulado.PorSurtir) 
			From vPedidos as PedidoAcumulado Inner Join
			mClientes on PedidoAcumulado.Cliente = mClientes.IdPersona
			Where mClientes.Sucursal = 1
			And mClientes.CreditoExcedido = 0
			And vPedidos.PorSurtir > 0
			And PedidoAcumulado.IdProducto = vPedidos.IdProducto
			And PedidoAcumulado.IdPedido < = vPedidos.IdPedido) as Acumulado,
		mProdVenta.Cantidad as Inventario,
		mClientes.Sucursal, mClientes.VIP
		From vPedidos inner join
		mClientes on vPedidos.Cliente = mClientes.IdPersona inner join 
		Indigo.dbo.mProdVenta on vPedidos.IdProducto = mProdVenta.IdProducto
		Where mProdVenta.IdCondicion = 1
		and mClientes.CreditoExcedido = 0
		and mClientes.Sucursal = 1
		and vPedidos.PorSurtir > 0
	) as Partidas 
Where Partidas.Inventario - (Partidas.Acumulado - Partidas.PorSurtir) > 0 
Union
Select Partidas.IdPedido, Partidas.Sucursal, Partidas.Cliente, Partidas.VIP, Partidas.IdProducto, Partidas.Precio,
Case When Partidas.Inventario >=  Partidas.Acumulado Then Partidas.PorSurtir 
		Else Partidas.Inventario - (Partidas.Acumulado - Partidas.PorSurtir) End as Cantidad,
((ROW_NUMBER() over (partition by Partidas.Cliente order by Partidas.IdPedido) - 1) / 32) + 1 as Factura
From (
		Select vPedidos.IdPedido, vPedidos.Cliente, vPedidos.IdProducto, vPedidos.PorSurtir, vPedidos.PrecioIva as Precio,
			(Select SUM(PedidoAcumulado.PorSurtir) 
			From vPedidos as PedidoAcumulado Inner Join
			mClientes on PedidoAcumulado.Cliente = mClientes.IdPersona
			Where mClientes.Sucursal = 2
			And mClientes.CreditoExcedido = 0
			And vPedidos.PorSurtir > 0
			And PedidoAcumulado.IdProducto = vPedidos.IdProducto
			And PedidoAcumulado.IdPedido < = vPedidos.IdPedido) as Acumulado,
		mProdVenta.Cantidad as Inventario,
		mClientes.Sucursal, mClientes.VIP
		From vPedidos inner join
		mClientes on vPedidos.Cliente = mClientes.IdPersona inner join 
		QtroIndigo.dbo.mProdVenta on vPedidos.IdProducto = mProdVenta.IdProducto
		Where mProdVenta.IdCondicion = 1
		and mClientes.CreditoExcedido = 0
		and mClientes.Sucursal = 2
		And vPedidos.PorSurtir > 0		
	) as Partidas 
Where Partidas.Inventario - (Partidas.Acumulado - Partidas.PorSurtir) > 0 
Union
Select Partidas.IdPedido, Partidas.Sucursal, Partidas.Cliente, Partidas.VIP, Partidas.IdProducto, Partidas.Precio,
Case When Partidas.Inventario >=  Partidas.Acumulado Then Partidas.PorSurtir 
		Else Partidas.Inventario - (Partidas.Acumulado - Partidas.PorSurtir) End as Cantidad,
((ROW_NUMBER() over (partition by Partidas.Cliente order by Partidas.IdPedido) - 1) / 32) + 1 as Factura
From (
		Select vPedidos.IdPedido, vPedidos.Cliente, vPedidos.IdProducto, vPedidos.PorSurtir, vPedidos.PrecioIva as Precio,
			(Select SUM(PedidoAcumulado.PorSurtir) 
			From vPedidos as PedidoAcumulado Inner Join
			mClientes on PedidoAcumulado.Cliente = mClientes.IdPersona
			Where mClientes.Sucursal = 3
			And mClientes.CreditoExcedido = 0
			And vPedidos.PorSurtir > 0
			And PedidoAcumulado.IdProducto = vPedidos.IdProducto
			And PedidoAcumulado.IdPedido < = vPedidos.IdPedido) as Acumulado,
		mProdVenta.Cantidad as Inventario,
		mClientes.Sucursal, mClientes.VIP
		From vPedidos inner join
		mClientes on vPedidos.Cliente = mClientes.IdPersona inner join 
		EdomxIndigo.dbo.mProdVenta on vPedidos.IdProducto = mProdVenta.IdProducto
		Where mProdVenta.IdCondicion = 1
		and mClientes.CreditoExcedido = 0
		and mClientes.Sucursal = 3
		And vPedidos.PorSurtir > 0		
	) as Partidas 
Where Partidas.Inventario - (Partidas.Acumulado - Partidas.PorSurtir) > 0 


