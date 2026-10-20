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
CREATE TABLE dbo.lProvDescCliente
	(
	IdLog bigint NOT NULL IDENTITY (1, 1),
	Cliente int NULL,
	IdProducto smallint NOT NULL,
	Descuento decimal(9, 2) NOT NULL,
	IdEmail int NOT NULL,
	FeModifica smalldatetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.lProvDescCliente ADD  CONSTRAINT [DF_lProvDescCliente_Cliente]  DEFAULT (NULL) FOR [Cliente]
GO
COMMIT
