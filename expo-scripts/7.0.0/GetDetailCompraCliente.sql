USE [ExpoMR]
GO
/****** Object:  UserDefinedFunction [dbo].[GetDetailCompraCliente]    Script Date: 28/10/2025 04:28:57 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 02 Abril 2024
-- Description:	Devuelve el resumen de compra por Proveedor o por producto especifico de un cliente
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[GetDetailCompraCliente] 
	@IdCliente INT,
	@TipoDetalle BIT, 
	@Filtro NVARCHAR(50)
AS
BEGIN
	;WITH Detalle AS
	(
		SELECT  UPPER(Proveedores.Nombre) AS Proveedor,
				Productos.Producto AS Producto,
				Pedidos.IdPedido AS IdPedido,
				Pedidos.IdProducto AS IdProducto,
				Pedidos.Cantidad AS Cantidad,
				Pedidos.Total AS Total
		FROM vPedidos AS Pedidos INNER JOIN
		vProductos AS Productos ON Pedidos.IdProducto = Productos.IdProducto INNER JOIN
		mLaboratorios AS Laboratorios ON Productos.IdLaboratorio = Laboratorios.IdLaboratorio INNER JOIN
		mProveedores AS Proveedores  ON Laboratorios.Proveedor = Proveedores.IdPersona
		WHERE Pedidos.Cliente = @IdCliente
		  AND (
				   (@TipoDetalle = 0 AND Proveedores.Nombre LIKE '%' + @Filtro + '%')
				OR (@TipoDetalle = 1 AND Productos.Producto LIKE '%' + @Filtro + '%')
			  )
	)
	SELECT Detalle.Proveedor, ISNULL(CASE WHEN @TipoDetalle = 1 THEN Detalle.Producto END, '') AS Producto,
		   COUNT(DISTINCT Detalle.IdPedido) AS NumPedidos,
		   ISNULL(CASE WHEN @TipoDetalle = 0 THEN COUNT(DISTINCT Detalle.IdProducto) END, 0) AS TotalProds,
		   SUM(Detalle.Cantidad) AS TotalPiezas, SUM(Detalle.Total) AS TotalCompra
	FROM Detalle
	GROUP BY Detalle.Proveedor, CASE WHEN @TipoDetalle = 1 THEN Detalle.Producto END
	ORDER BY Detalle.Proveedor, CASE WHEN @TipoDetalle = 1 THEN Detalle.Producto END;

	RETURN
END