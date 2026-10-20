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
CREATE TABLE dbo.Tmp_mSurtido
	(
	IdSurtido int NOT NULL IDENTITY (1, 1),
	IdPedido int NOT NULL,
	Sucursal tinyint NOT NULL,
	Cliente int NOT NULL,
	IdProducto smallint NOT NULL,
	Precio real NOT NULL,
	Cantidad int NOT NULL,
	Factura smallint NOT NULL,
	Surtir int NULL,
	Descuento decimal(9, 2) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_mSurtido SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_mSurtido ADD CONSTRAINT
	DF_mSurtido_Descuento DEFAULT 0 FOR Descuento
GO
SET IDENTITY_INSERT dbo.Tmp_mSurtido OFF
GO
IF EXISTS(SELECT * FROM dbo.mSurtido)
	 EXEC('INSERT INTO dbo.Tmp_mSurtido (IdPedido, Sucursal, Cliente, IdProducto, Precio, Cantidad, Factura, Surtir)
		SELECT IdPedido, Sucursal, Cliente, IdProducto, Precio, Cantidad, Factura, Surtir FROM dbo.mSurtido WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.mSurtido
GO
EXECUTE sp_rename N'dbo.Tmp_mSurtido', N'mSurtido', 'OBJECT' 
GO
ALTER TABLE dbo.mSurtido ADD CONSTRAINT
	PK_mSurtido_1 PRIMARY KEY CLUSTERED 
	(
	IdSurtido
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
