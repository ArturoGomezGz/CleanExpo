USE ExpoMR
/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.mArticulos
	DROP CONSTRAINT FK_mArticulos_mProveedores
GO
ALTER TABLE dbo.mProveedores SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_mArticulos
	(
	IdArticulo smallint NOT NULL IDENTITY (1, 1),
	Proveedor int NOT NULL,
	Nombre nvarchar(50) NOT NULL,
	Descripcion nvarchar(250) NULL,
	Cantidad int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_mArticulos SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_mArticulos ON
GO
IF EXISTS(SELECT * FROM dbo.mArticulos)
	 EXEC('INSERT INTO dbo.Tmp_mArticulos (IdArticulo, Proveedor, Nombre, Descripcion, Cantidad)
		SELECT IdArticulo, Proveedor, Nombre, Descripcion, CONVERT(int, Cantidad) FROM dbo.mArticulos WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_mArticulos OFF
GO
ALTER TABLE dbo.dArticulos
	DROP CONSTRAINT FK_dArticulos_mArticulos
GO
ALTER TABLE dbo.dPromociones
	DROP CONSTRAINT FK_dPromociones_mArticulos
GO
DROP TABLE dbo.mArticulos
GO
EXECUTE sp_rename N'dbo.Tmp_mArticulos', N'mArticulos', 'OBJECT' 
GO
ALTER TABLE dbo.mArticulos ADD CONSTRAINT
	PK_mArticulos PRIMARY KEY CLUSTERED 
	(
	IdArticulo
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.mArticulos ADD CONSTRAINT
	FK_mArticulos_mProveedores FOREIGN KEY
	(
	Proveedor
	) REFERENCES dbo.mProveedores
	(
	IdPersona
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.dPromociones ADD CONSTRAINT
	FK_dPromociones_mArticulos FOREIGN KEY
	(
	IdArticulo
	) REFERENCES dbo.mArticulos
	(
	IdArticulo
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.dPromociones SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.dArticulos ADD CONSTRAINT
	FK_dArticulos_mArticulos FOREIGN KEY
	(
	IdArticulo
	) REFERENCES dbo.mArticulos
	(
	IdArticulo
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.dArticulos SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
