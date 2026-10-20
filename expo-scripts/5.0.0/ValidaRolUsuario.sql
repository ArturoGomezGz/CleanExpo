USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[ValidaRolUsuario]    Script Date: 08/04/2024 03:52:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 27 Marzo 2024
-- Description:	Valida si el usuario cuenta un un rol en especifico
-- =============================================

CREATE OR ALTER  PROCEDURE [dbo].[ValidaRolUsuario] 
	-- Add the parameters for the stored procedure here
	@UserName NVARCHAR(50),
	@Rol NVARCHAR(20)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Result BIT = 0 
    -- Insert statements for procedure here

	IF EXISTS (SELECT AspNetRoles.Id
			   FROM AspNetUsers LEFT OUTER JOIN 
			   AspNetUserRoles ON AspNetUsers.Id = AspNetUserRoles.UserId LEFT OUTER JOIN 
			   AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id
			   WHERE AspNetUsers.UserName = LOWER(@UserName)
			   AND AspNetRoles.Name = LOWER(@Rol))
	BEGIN
		SET @Result = 1
	END

	RETURN @Result
END
