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
CREATE TABLE dbo.mTipoNotificacion
	(
	IdTipoNotificacion tinyint NOT NULL IDENTITY (1, 1),
	Descripcion nvarchar(20) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.mTipoNotificacion ADD CONSTRAINT
	PK_mTipoNotificacion PRIMARY KEY CLUSTERED 
	(
	IdTipoNotificacion
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.mTipoNotificacion SET (LOCK_ESCALATION = TABLE)
GO
COMMIT