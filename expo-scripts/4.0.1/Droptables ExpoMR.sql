Use ExpoMR

BEGIN TRAN
	DROP TABLE AspNetUserRoles
	DROP TABLE AspNetUserLogins
	DROP TABLE AspNetUserClaims
	DROP TABLE AspNetUserTokens
	DROP TABLE AspNetRoleClaims
	DROP TABLE AspNetUsers
	DROP TABLE AspNetRoles
COMMIT 