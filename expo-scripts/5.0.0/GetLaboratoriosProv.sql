USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[GetLaboratoriosProv]    Script Date: 13/03/2024 01:07:45 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Josue Pulido, Carlos Rivera
-- Create date: 22 Abril 2024
-- Description:	Devuelve una lista de los laboratorios de un proveedor
--				@Asignados:  (1 = Muestra los laboratorios que tiene el proveedor asignados,
--							  0 = Muestra los laboratrios que no estan asignados a proveedores)
-- =============================================

CREATE OR ALTER FUNCTION [dbo].[GetLaboratoriosProv] (@IdProveedor INT, @Asignados BIT)
RETURNS
	@Result TABLE (IdLaboratorio SMALLINT, 
	               Laboratorio NVARCHAR(30))
AS
BEGIN
    -- Insert statements for procedure here
	IF(@Asignados = 0)
	BEGIN
		INSERT @Result (IdLaboratorio, Laboratorio)
		SELECT mLaboratorios.IdLaboratorio, mLaboratorios.Laboratorio
		FROM Indigo.dbo.mLaboratorios
		WHERE mLaboratorios.IdLaboratorio NOT IN (SELECT mLaboratorios.IdLaboratorio FROM ExpoMR.dbo.mLaboratorios)
		ORDER BY mLaboratorios.Laboratorio
	END
	ELSE
	BEGIN
		INSERT @Result (IdLaboratorio, Laboratorio)
		SELECT mLaboratorios.IdLaboratorio, mLaboratorios.Laboratorio
		FROM ExpoMR.dbo.mLaboratorios
		WHERE mLaboratorios.Proveedor = @IdProveedor
		ORDER BY mLaboratorios.Laboratorio
	END

	RETURN
END