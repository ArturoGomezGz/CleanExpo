USE [ExpoMR]
GO

/****** Object:  View [dbo].[vRutasClientes]    Script Date: 14/03/2024 12:06:05 p. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [dbo].[vRutasClientes] AS

	SELECT DISTINCT Rutas.IdRuta, Rutas.Ruta, Rutas.IdEmpleado, Rutas.Empleado, Rutas.IdCliente, Rutas.Cliente
	FROM
		(
  			SELECT mRutas.IdRuta, mRutas.Descripcion AS Ruta, mRutas.IdPersona AS IdEmpleado, Empleado.Nombre AS Empleado, Cliente.IdPersona AS IdCliente, Cliente.Nombre AS Cliente
			FROM Indigo.dbo.mRutas INNER JOIN
			Indigo.dbo.mClientes ON mRutas.IdRuta = mClientes.IdRuta INNER JOIN
			Indigo.dbo.vNombres AS Cliente ON mClientes.IdPersona = Cliente.IdPersona INNER JOIN
			Indigo.dbo.vNombres AS Empleado ON mRutas.IdPersona = Empleado.IdPersona
			WHERE mClientes.Status NOT IN (1, 3) --1 = Inactivo, 3 = No Deseado
			UNION
			SELECT mRutas.IdRuta, mRutas.Descripcion AS Ruta, mRutas.IdPersona AS IdEmpleado, Empleado.Nombre AS Empleado, Cliente.IdPersona AS IdCliente, Cliente.Nombre AS Cliente
			FROM QtroIndigo.dbo.mRutas INNER JOIN
			QtroIndigo.dbo.mClientes ON mRutas.IdRuta = mClientes.IdRuta INNER JOIN
			QtroIndigo.dbo.vNombres AS Cliente ON mClientes.IdPersona = Cliente.IdPersona INNER JOIN
			QtroIndigo.dbo.vNombres AS Empleado ON mRutas.IdPersona = Empleado.IdPersona
			WHERE mClientes.Status NOT IN (1, 3) --1 = Inactivo, 3 = No Deseado
			UNION
			SELECT mRutas.IdRuta, mRutas.Descripcion AS Ruta, mRutas.IdPersona AS IdEmpleado, Empleado.Nombre AS Empleado, Cliente.IdPersona AS IdCliente, Cliente.Nombre AS Cliente
			FROM EdomxIndigo.dbo.mRutas INNER JOIN
			EdomxIndigo.dbo.mClientes ON mRutas.IdRuta = mClientes.IdRuta INNER JOIN
			EdomxIndigo.dbo.vNombres AS Cliente ON mClientes.IdPersona = Cliente.IdPersona INNER JOIN
			EdomxIndigo.dbo.vNombres AS Empleado ON mRutas.IdPersona = Empleado.IdPersona
			WHERE mClientes.Status NOT IN (1, 3) --1 = Inactivo, 3 = No Deseado
		) AS Rutas
GO


