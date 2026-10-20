USE [ExpoMR]
GO

/****** Object:  View [dbo].[vPersonas]    Script Date: 07/01/2021 12:17:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




ALTER VIEW [dbo].[vPersonas] AS
SELECT Usuarios.IdPersona, Usuarios.Nombre, Usuarios.Email, Usuarios.IdTipoUsuario, Usuarios.VIP, 
CASE WHEN mUsuarios.IdPersona IS NULL THEN 0 ELSE 1 END Usuario, ISNULL(mUsuarios.Sucursal,0) AS Sucursal,
ISNULL(dbo.AspNetUsers.LockoutEnabled,0) AS Lock
FROM (
		--CLIENTES
		SELECT mClientes.IdPersona, vPersonas.Nombre, vPersonas.Email, 1 AS IdTipoUsuario, ISNULL(ClientesExpo.VIP, 0) AS VIP
		FROM Indigo.dbo.mClientes INNER JOIN
		Indigo.dbo.vPersonas AS vPersonas ON mClientes.IdPersona = vPersonas.IdPersona LEFT OUTER JOIN
		Indigo.dbo.mEmpleados AS mEmpleado ON vPersonas.IdPersona = mEmpleado.IdPersona LEFT OUTER JOIN
		mClientes AS ClientesExpo ON vPersonas.IdPersona = ClientesExpo.IdPersona
		WHERE mEmpleado.IdPersona IS NULL
		UNION
		--CONTACTO DE CLIENTES
		SELECT vPersonas.IdPersona, vPersonas.Nombre, vPersonas.Email, 1 AS IdTipoUsuario, 0 AS VIP
		FROM Indigo.dbo.mPerEmp AS Contacto INNER JOIN
		Indigo.dbo.vPersonas ON Contacto.IdPersona = vPersonas.IdPersona INNER JOIN
		Indigo.dbo.mPersonas ON Contacto.Persona = mPersonas.Persona INNER JOIN
		Indigo.dbo.mPerEmp ON mPersonas.IdReferencia = mPerEmp.Persona INNER JOIN
		mClientes ON mPerEmp.IdPersona = mClientes.IdPersona		
		UNION
		--PROVEEDORES
		SELECT mProveedores.IdPersona, vPersonas.Nombre, vPersonas.Email, 2 AS IdTipoUsuario, 0 AS VIP
		FROM mProveedores INNER JOIN
		Indigo.dbo.vPersonas AS vPersonas ON mProveedores.IdPersona = vPersonas.IdPersona
		UNION
		--CONTACTO DE PROVEEDORES
		SELECT vPersonas.IdPersona, vPersonas.Nombre, vPersonas.Email, 2 AS IdTipoUsuario, 0 AS VIP
		FROM Indigo.dbo.mPerEmp AS Contacto INNER JOIN
		Indigo.dbo.vPersonas ON Contacto.IdPersona = vPersonas.IdPersona INNER JOIN
		Indigo.dbo.mPersonas ON Contacto.Persona = mPersonas.Persona INNER JOIN
		Indigo.dbo.mPerEmp ON mPersonas.IdReferencia = mPerEmp.Persona INNER JOIN
		mProveedores ON mPerEmp.IdPersona = mProveedores.IdPersona 
		UNION
		--EMPLEADOS
		SELECT vPersonas.IdPersona, vPersonas.Nombre, vPersonas.Email, 4 AS IdTipoUsuario, 0 AS VIP
		FROM Indigo.dbo.vPersonas INNER JOIN
		Indigo.dbo.vRoles ON vPersonas.IdPersona = vRoles.IdPersona LEFT OUTER JOIN
		Indigo.dbo.mEmpleados AS GDLEmp ON vPersonas.IdPersona = GDLEmp.IdPersona AND GDLEmp.Activo = 1 LEFT OUTER JOIN
		QtroIndigo.dbo.mEmpleados AS QTROEmp ON vPersonas.IdPersona = QTROEmp.IdPersona AND QTROEmp.Activo = 1 LEFT OUTER JOIN
		EdomxIndigo.dbo.mEmpleados AS EDOMXEmp ON vPersonas.IdPersona = EDOMXEmp.IdPersona AND EDOMXEmp.Activo = 1
		WHERE (GDLEmp.IdPersona IS NOT NULL OR QTROEmp.IdPersona IS NOT NULL OR EDOMXEmp.IdPersona IS NOT NULL)
	) AS Usuarios LEFT OUTER JOIN
mUsuarios ON Usuarios.IdPersona = mUsuarios.IdPersona LEFT OUTER JOIN --AND Usuarios.IdTipoUsuario = mUsuarios.IdTipoUsuario 
AspNetUsers ON mUsuarios.UserName = dbo.AspNetUsers.UserName



GO


