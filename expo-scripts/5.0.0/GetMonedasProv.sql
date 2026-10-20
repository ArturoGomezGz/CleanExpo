USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[GetMonedasProv]    Script Date: 13/03/2024 01:07:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 22 Abril 2024
-- Description:	Devuelve el numero de monedas del proveedor
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[GetMonedasProv] 
	-- Add the parameters for the stored procedure here
	@IdProveedor INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Saldo INT

    -- Insert statements for procedure here
	SET @Saldo = (SELECT ISNULL(mMonedero.Saldo,0) AS Saldo
				  FROM mProveedores LEFT OUTER JOIN
				  mMonedero ON mProveedores.IdPersona = mMonedero.IdPersona
				  WHERE mProveedores.IdPersona = @IdProveedor)

	RETURN @Saldo
END