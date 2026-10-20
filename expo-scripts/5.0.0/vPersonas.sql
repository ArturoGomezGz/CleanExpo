USE [ExpoMR]
GO

/****** Object:  View [dbo].[vPersonas]    Script Date: 19/03/2024 11:26:59 a. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[vPersonas] AS
SELECT Usuarios.IdPersona, Usuarios.Nombre, Usuarios.IdEmail, Usuarios.Email, Usuarios.IdTipoUsuario, Usuarios.VIP, 
CASE WHEN mUsuarios.IdPersona IS NULL THEN 0 ELSE 1 END Usuario, ISNULL(mUsuarios.Sucursal,0) AS Sucursal,
CASE WHEN AspNetUsers.LockoutEnd IS NULL THEN 0 ELSE 1 END AS Lock
FROM (
		--CLIENTES GDL
		SELECT vRoles.IdPersona, vNombres.Nombre, mEmails.IdEmail, mEmails.Email, 1 AS IdTipoUsuario, ClientesExpo.VIP
		FROM Indigo.dbo.vRoles INNER JOIN 
		Indigo.dbo.vNombres ON vRoles.IdPersona = vNombres.IdPersona INNER JOIN
		Indigo.dbo.mPerEmp ON vRoles.IdPersona = mPerEmp.IdPersona LEFT OUTER JOIN
		Indigo.dbo.mEmails ON mPerEmp.Persona = mEmails.Persona LEFT OUTER JOIN
		Indigo.dbo.dEmails ON mEmails.IdEmail = dEmails.IdEmail LEFT OUTER JOIN
		mClientes AS ClientesExpo ON mPerEmp.IdPersona = ClientesExpo.IdPersona
		WHERE vRoles.Cliente = 1
		AND dEmails.IdEmailFuncion = 11 -- 11 = Acceso Expo Cliente

		UNION

		--CLIENTES QTRO
		SELECT vRoles.IdPersona, vNombres.Nombre, mEmails.IdEmail, mEmails.Email, 1 AS IdTipoUsuario, ClientesExpo.VIP
		FROM QtroIndigo.dbo.vRoles INNER JOIN 
		QtroIndigo.dbo.vNombres ON vRoles.IdPersona = vNombres.IdPersona INNER JOIN
		QtroIndigo.dbo.mPerEmp ON vRoles.IdPersona = mPerEmp.IdPersona LEFT OUTER JOIN
		QtroIndigo.dbo.mEmails ON mPerEmp.Persona = mEmails.Persona LEFT OUTER JOIN
		QtroIndigo.dbo.dEmails ON mEmails.IdEmail = dEmails.IdEmail LEFT OUTER JOIN
		mClientes AS ClientesExpo ON mPerEmp.IdPersona = ClientesExpo.IdPersona
		WHERE vRoles.Cliente = 1
		AND dEmails.IdEmailFuncion = 11 -- 11 = Acceso Expo Cliente

		UNION

		--CLIENTES MEX
		SELECT vRoles.IdPersona, vNombres.Nombre, mEmails.IdEmail, mEmails.Email, 1 AS IdTipoUsuario, ClientesExpo.VIP
		FROM EdomxIndigo.dbo.vRoles INNER JOIN 
		EdomxIndigo.dbo.vNombres ON vRoles.IdPersona = vNombres.IdPersona INNER JOIN
		EdomxIndigo.dbo.mPerEmp ON vRoles.IdPersona = mPerEmp.IdPersona LEFT OUTER JOIN
		EdomxIndigo.dbo.mEmails ON mPerEmp.Persona = mEmails.Persona LEFT OUTER JOIN
		EdomxIndigo.dbo.dEmails ON mEmails.IdEmail = dEmails.IdEmail LEFT OUTER JOIN
		mClientes AS ClientesExpo ON mPerEmp.IdPersona = ClientesExpo.IdPersona
		WHERE vRoles.Cliente = 1
		AND dEmails.IdEmailFuncion = 11 -- 11 = Acceso Expo Cliente

		UNION

		--PROVEEDORES
		SELECT vRoles.IdPersona, vNombres.Nombre, mEmails.IdEmail, mEmails.Email, 2 AS IdTipoUsuario, CAST(0 AS BIT) AS VIP
		FROM Indigo.dbo.vRoles INNER JOIN 
		Indigo.dbo.vNombres ON vRoles.IdPersona = vNombres.IdPersona INNER JOIN
		Indigo.dbo.mPerEmp ON vRoles.IdPersona = mPerEmp.IdPersona LEFT OUTER JOIN
		Indigo.dbo.mEmails ON mPerEmp.Persona = mEmails.Persona LEFT OUTER JOIN
		Indigo.dbo.dEmails ON mEmails.IdEmail = dEmails.IdEmail
		WHERE vRoles.Proveedor = 1 
		AND dEmails.IdEmailFuncion = 12 -- 12 = Acceso Expo Proveedor

		UNION

		--EMPLEADOS GDL
		SELECT vRoles.IdPersona, vNombres.Nombre, mEmails.IdEmail, mEmails.Email,  4 AS IdTipoUsuario, CAST(0 AS BIT) AS VIP
		FROM Indigo.dbo.vRoles INNER JOIN 
		Indigo.dbo.vNombres ON vRoles.IdPersona = vNombres.IdPersona INNER JOIN
		Indigo.dbo.mEmpleados ON vNombres.IdPersona = mEmpleados.IdPersona INNER JOIN
		Indigo.dbo.mPerEmp ON vRoles.IdPersona = mPerEmp.IdPersona LEFT OUTER JOIN
		Indigo.dbo.mEmails ON mPerEmp.Persona = mEmails.Persona LEFT OUTER JOIN
		Indigo.dbo.dEmails ON mEmails.IdEmail = dEmails.IdEmail
		WHERE vRoles.Empleado = 1
		AND mEmpleados.Activo = 1
		AND dEmails.IdEmailFuncion = 14 -- 14 = Acceso Expo Empleado
		
		UNION
		
		--EMPLEADOS QTRO
		SELECT vRoles.IdPersona, vNombres.Nombre, mEmails.IdEmail, mEmails.Email,  4 AS IdTipoUsuario, CAST(0 AS BIT) AS VIP
		FROM QtroIndigo.dbo.vRoles INNER JOIN 
		QtroIndigo.dbo.vNombres ON vRoles.IdPersona = vNombres.IdPersona INNER JOIN
		QtroIndigo.dbo.mEmpleados ON vNombres.IdPersona = mEmpleados.IdPersona INNER JOIN
		QtroIndigo.dbo.mPerEmp ON vRoles.IdPersona = mPerEmp.IdPersona LEFT OUTER JOIN
		QtroIndigo.dbo.mEmails ON mPerEmp.Persona = mEmails.Persona LEFT OUTER JOIN
		QtroIndigo.dbo.dEmails ON mEmails.IdEmail = dEmails.IdEmail
		WHERE vRoles.Empleado = 1
		AND mEmpleados.Activo = 1
		AND dEmails.IdEmailFuncion = 14 -- 14 = Acceso Expo Empleado

		UNION

		--EMPLEADOS MEX
		SELECT vRoles.IdPersona, vNombres.Nombre, mEmails.IdEmail, mEmails.Email,  4 AS IdTipoUsuario, CAST(0 AS BIT) AS VIP
		FROM EdomxIndigo.dbo.vRoles INNER JOIN 
		EdomxIndigo.dbo.vNombres ON vRoles.IdPersona = vNombres.IdPersona INNER JOIN
		EdomxIndigo.dbo.mEmpleados ON vNombres.IdPersona = mEmpleados.IdPersona INNER JOIN
		EdomxIndigo.dbo.mPerEmp ON vRoles.IdPersona = mPerEmp.IdPersona LEFT OUTER JOIN
		EdomxIndigo.dbo.mEmails ON mPerEmp.Persona = mEmails.Persona LEFT OUTER JOIN
		EdomxIndigo.dbo.dEmails ON mEmails.IdEmail = dEmails.IdEmail
		WHERE vRoles.Empleado = 1
		AND mEmpleados.Activo = 1
		AND dEmails.IdEmailFuncion = 14 -- 14 = Acceso Expo Empleado

	) AS Usuarios LEFT OUTER JOIN
mUsuarios ON Usuarios.IdEmail = mUsuarios.IdEmail LEFT OUTER JOIN
AspNetUsers ON Usuarios.Email = dbo.AspNetUsers.UserName
GO