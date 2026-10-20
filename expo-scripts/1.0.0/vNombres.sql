Use [ExpoMR]
GO
If exists (select * from sysobjects where name = 'vNombres')
	drop view vNombres

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW vNombres AS

Select mClientes.IdPersona, mClientes.Nombre   
From mClientes
Union
Select mProveedores.IdPersona, mProveedores.Nombre   
From mProveedores 


GO