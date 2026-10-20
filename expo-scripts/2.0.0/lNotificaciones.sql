USE ExpoMR
/* Para evitar posibles problemas de pérdida de datos, debe revisar este script detalladamente antes de ejecutarlo fuera del contexto del diseñador de base de datos.*/
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
ALTER TABLE dbo.mTipoNotificacion SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.mUsuarios SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.lNotificaciones
	(
	IdNotificacion int NOT NULL IDENTITY (1, 1),
	IdPersona int NOT NULL,
	IdTipoNotificacion tinyint NOT NULL,
	Fecha smalldatetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.lNotificaciones ADD CONSTRAINT
	PK_lNotificaciones PRIMARY KEY CLUSTERED 
	(
	IdNotificacion
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.lNotificaciones ADD CONSTRAINT
	FK_lNotificaciones_mUsuarios FOREIGN KEY
	(
	IdPersona
	) REFERENCES dbo.mUsuarios
	(
	IdPersona
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.lNotificaciones ADD CONSTRAINT
	FK_lNotificaciones_mTipoNotificacion FOREIGN KEY
	(
	IdTipoNotificacion
	) REFERENCES dbo.mTipoNotificacion
	(
	IdTipoNotificacion
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.lNotificaciones SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
