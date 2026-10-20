USE ExpoMR

BEGIN TRAN
	DECLARE @roleId1 AS UNIQUEIDENTIFIER = NEWID()
	INSERT dbo.AspNetRoles (Id, Name)
	VALUES (@roleId1, 'Vendedor')	

	DECLARE @roleId2 AS UNIQUEIDENTIFIER = NEWID()
	INSERT dbo.AspNetRoles (Id, Name)
	VALUES (@roleId2, 'Capturista')
COMMIT

	  
