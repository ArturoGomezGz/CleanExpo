USE [ExpoMR]
GO

/****** Object:  View [dbo].[vSurtidoCredito]    Script Date: 03/03/2025 05:12:57 p. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[vSurtidoCredito] AS
--Select Surtido.IdPedido,
--Case When Surtido.MontoAcumulado > Surtido.Credito 
--	Then CAST(Surtido.Cantidad - ((Surtido.MontoAcumulado - Surtido.Credito) / Surtido.Precio) as int)
--	Else Surtido.Cantidad End as Surtir
--From (
--	Select mSurtido.IdPedido, mSurtido.Cantidad, mSurtido.Precio,
--		vCredito.Credito + vCredito.CreditoInyectado as Credito,
--		(Select SUM(SurtidoAcumulado.Precio * SurtidoAcumulado.Cantidad) 
--		From mSurtido as SurtidoAcumulado
--		Where SurtidoAcumulado.Cliente = mSurtido.Cliente
--		And SurtidoAcumulado.IdPedido < = mSurtido.IdPedido) as MontoAcumulado
--	From mSurtido Inner Join
--	Indigo.dbo.vCredito on mSurtido.Cliente = vCredito.IdPersona
--	Where mSurtido.Sucursal = 1
--	) as Surtido
--Union
--Select Surtido.IdPedido,
--Case When Surtido.MontoAcumulado > Surtido.Credito 
--	Then CAST(Surtido.Cantidad - ((Surtido.MontoAcumulado - Surtido.Credito) / Surtido.Precio) as int)
--	Else Surtido.Cantidad End as Surtir
--From (
--	Select mSurtido.IdPedido, mSurtido.Cantidad, mSurtido.Precio,
--		vCredito.Credito + vCredito.CreditoInyectado as Credito,
--		(Select SUM(SurtidoAcumulado.Precio * SurtidoAcumulado.Cantidad) 
--		From mSurtido as SurtidoAcumulado
--		Where SurtidoAcumulado.Cliente = mSurtido.Cliente
--		And SurtidoAcumulado.IdPedido < = mSurtido.IdPedido) as MontoAcumulado
--	From mSurtido Inner Join
--	QtroIndigo.dbo.vCredito on mSurtido.Cliente = vCredito.IdPersona
--	Where mSurtido.Sucursal = 2
--	) as Surtido
--Union
--Select Surtido.IdPedido,
--Case When Surtido.MontoAcumulado > Surtido.Credito 
--	Then CAST(Surtido.Cantidad - ((Surtido.MontoAcumulado - Surtido.Credito) / Surtido.Precio) as int)
--	Else Surtido.Cantidad End as Surtir
--From (
--	Select mSurtido.IdPedido, mSurtido.Cantidad, mSurtido.Precio,
--		vCredito.Credito + vCredito.CreditoInyectado as Credito,
--		(Select SUM(SurtidoAcumulado.Precio * SurtidoAcumulado.Cantidad) 
--		From mSurtido as SurtidoAcumulado
--		Where SurtidoAcumulado.Cliente = mSurtido.Cliente
--		And SurtidoAcumulado.IdPedido < = mSurtido.IdPedido) as MontoAcumulado
--	From mSurtido Inner Join
--	EdomxIndigo.dbo.vCredito on mSurtido.Cliente = vCredito.IdPersona
--	Where mSurtido.Sucursal = 3
--	) as Surtido

WITH Surtido AS (
    SELECT mSurtido.IdPedido, mSurtido.Cliente, mSurtido.Sucursal, mSurtido.Cantidad, mSurtido.Precio,
    vCredito.Credito + vCredito.CreditoInyectado AS Credito,
    SUM(mSurtido.Precio * mSurtido.Cantidad) OVER (PARTITION BY mSurtido.Cliente ORDER BY mSurtido.IdPedido) AS MontoAcumulado        
    FROM mSurtido INNER JOIN
	Indigo.dbo.vCredito ON mSurtido.Cliente = vCredito.IdPersona
    WHERE mSurtido.Sucursal = 1 --1 = GDL

    UNION

    SELECT mSurtido.IdPedido, mSurtido.Cliente, mSurtido.Sucursal, mSurtido.Cantidad, mSurtido.Precio,
    vCredito.Credito + vCredito.CreditoInyectado AS Credito,
    SUM(mSurtido.Precio * mSurtido.Cantidad) OVER (PARTITION BY mSurtido.Cliente ORDER BY mSurtido.IdPedido) AS MontoAcumulado   
    FROM mSurtido INNER JOIN
	QtroIndigo.dbo.vCredito ON mSurtido.Cliente = vCredito.IdPersona
    WHERE mSurtido.Sucursal = 2 --QTRO

    UNION

    SELECT mSurtido.IdPedido, mSurtido.Cliente, mSurtido.Sucursal, mSurtido.Cantidad, mSurtido.Precio,
    vCredito.Credito + vCredito.CreditoInyectado AS Credito,
    SUM(mSurtido.Precio * mSurtido.Cantidad) OVER (PARTITION BY mSurtido.Cliente ORDER BY mSurtido.IdPedido) AS MontoAcumulado   
    FROM mSurtido INNER JOIN
	EdomxIndigo.dbo.vCredito ON mSurtido.Cliente = vCredito.IdPersona
    WHERE mSurtido.Sucursal = 3 --MEX
)

SELECT IdPedido,
    CASE 
        WHEN MontoAcumulado > Credito 
        THEN CAST(Cantidad - ((MontoAcumulado - Credito) / Precio) AS INT)
        ELSE Cantidad 
    END AS Surtir
FROM Surtido;
GO


