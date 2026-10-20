USE [ExpoMR]
GO

/****** Object:  View [dbo].[vClientesSucursales]    Script Date: 07/01/2021 14:09:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[vClientesSucursales] AS

  	SELECT Cliente.IdPersona AS IdCliente, Cliente.Nombre AS Cliente
	FROM Indigo.dbo.mClientes INNER JOIN
	Indigo.dbo.vNombres AS Cliente ON mClientes.IdPersona = Cliente.IdPersona
	WHERE mClientes.Status NOT IN (1, 3) --1 = Inactivo, 3 = No Deseado
	UNION
	SELECT Cliente.IdPersona AS IdCliente, Cliente.Nombre AS Cliente
	FROM QtroIndigo.dbo.mClientes INNER JOIN
	QtroIndigo.dbo.vNombres AS Cliente ON mClientes.IdPersona = Cliente.IdPersona
	WHERE mClientes.Status NOT IN (1, 3) --1 = Inactivo, 3 = No Deseado
	UNION
	SELECT Cliente.IdPersona AS IdCliente, Cliente.Nombre AS Cliente
	FROM EdomxIndigo.dbo.mClientes INNER JOIN
	EdomxIndigo.dbo.vNombres AS Cliente ON mClientes.IdPersona = Cliente.IdPersona
	WHERE mClientes.Status NOT IN (1, 3) --1 = Inactivo, 3 = No Deseado

GO