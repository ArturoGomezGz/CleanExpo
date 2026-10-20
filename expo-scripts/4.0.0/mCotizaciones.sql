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
ALTER TABLE dbo.mCotizaciones
	DROP CONSTRAINT FK_mCotizaciones_mUsuarios
GO
ALTER TABLE dbo.mUsuarios SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.mCotizaciones
	DROP CONSTRAINT FK_mCotizaciones_mClientes
GO
ALTER TABLE dbo.mClientes SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_mCotizaciones
	(
	IdCotizacion int NOT NULL IDENTITY (1, 1),
	Cliente int NOT NULL,
	IdDocumento tinyint NOT NULL,
	IdUsoCFDI tinyint NULL,
	IdTipoVenta tinyint NOT NULL,
	IdTransporte int NULL,
	IdPorte tinyint NULL,
	Fiscal int NOT NULL,
	Entrega int NOT NULL,
	Obsv nvarchar(250) NULL,
	Usuario int NOT NULL,
	Fecha smalldatetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_mCotizaciones SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_mCotizaciones ON
GO
IF EXISTS(SELECT * FROM dbo.mCotizaciones)
	 EXEC('INSERT INTO dbo.Tmp_mCotizaciones (IdCotizacion, Cliente, IdDocumento, IdUsoCFDI, IdTipoVenta, IdTransporte, IdPorte, Fiscal, Entrega, Obsv, Usuario, Fecha)
		SELECT IdCotizacion, Cliente, IdDocumento, IdUsoCFDI, IdTipoVenta, IdTransporte, IdPorte, Fiscal, Entrega, Obsv, Usuario, Fecha FROM dbo.mCotizaciones WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_mCotizaciones OFF
GO
ALTER TABLE dbo.dCotizaciones
	DROP CONSTRAINT FK_dCotizaciones_mCotizaciones
GO
DROP TABLE dbo.mCotizaciones
GO
EXECUTE sp_rename N'dbo.Tmp_mCotizaciones', N'mCotizaciones', 'OBJECT' 
GO
ALTER TABLE dbo.mCotizaciones ADD CONSTRAINT
	PK_mCotizaciones PRIMARY KEY CLUSTERED 
	(
	IdCotizacion
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.mCotizaciones ADD CONSTRAINT
	FK_mCotizaciones_mClientes FOREIGN KEY
	(
	Cliente
	) REFERENCES dbo.mClientes
	(
	IdPersona
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.mCotizaciones ADD CONSTRAINT
	FK_mCotizaciones_mUsuarios FOREIGN KEY
	(
	Usuario
	) REFERENCES dbo.mUsuarios
	(
	IdPersona
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.dCotizaciones ADD CONSTRAINT
	FK_dCotizaciones_mCotizaciones FOREIGN KEY
	(
	IdCotizacion
	) REFERENCES dbo.mCotizaciones
	(
	IdCotizacion
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.dCotizaciones SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
