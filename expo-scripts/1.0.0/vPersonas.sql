USE ExpoMR
GO
if exists(select * from sysobjects where name = 'vPersonas')
	drop view vPersonas
/****** Object:  View [dbo].[vUsuarios]    Script Date: 08/20/2020 13:19:11 ******/
/*
	La vista regresa los nombres de clientes y proveedores que cuenten o no con un usuario para ingresar al sistema de la Expo.
	
	Datos:
	IdTipoUsuario: 1 = Cliente,
				   2 = Proveedor,
				   3 = Administradores
		  
	Usuario: 0 = No tiene usuario
			 1 = Tiene usuario
*/

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vPersonas] AS
SELECT Usuarios.IdPersona, Usuarios.Nombre, Usuarios.Email, Usuarios.IdTipoUsuario, Usuarios.VIP, 
CASE WHEN mUsuarios.IdPersona IS NULL THEN 0 ELSE 1 END Usuario, ISNULL(mUsuarios.Sucursal,0) AS Sucursal,
ISNULL(dbo.AspNetUsers.LockoutEnabled,0) AS Lock
FROM (
		--CLIENTES
		SELECT mClientes.IdPersona, vPersonas.Nombre, vPersonas.Email, 1 AS IdTipoUsuario, ISNULL(ClientesExpo.VIP, 0) AS VIP
		FROM Indigo.dbo.mClientes INNER JOIN
		Indigo.dbo.vPersonas AS vPersonas ON mClientes.IdPersona = vPersonas.IdPersona LEFT OUTER JOIN
		mClientes AS ClientesExpo ON vPersonas.IdPersona = ClientesExpo.IdPersona
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
		--ADMINISTRADORES
		SELECT vPersonas.IdPersona, vPersonas.Nombre, vPersonas.Email, 3 AS IdTipoUsuario, 0 AS VIP
		FROM Indigo.dbo.mEmpleados Inner join
		Indigo.dbo.vPersonas On mEmpleados.IdPersona = vPersonas.IdPersona INNER JOIN
		Indigo.dbo.vRoles ON vPersonas.IdPersona = vRoles.IdPersona 
	) AS Usuarios LEFT OUTER JOIN
mUsuarios ON Usuarios.IdPersona = mUsuarios.IdPersona AND Usuarios.IdTipoUsuario = mUsuarios.IdTipoUsuario LEFT OUTER JOIN
AspNetUsers ON mUsuarios.UserName = dbo.AspNetUsers.UserName
GO