USE ExpoMR

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author: Josue Pulido, Carlos Rivera
-- Create date: 14/02/2024
-- Description:	Valida si la fecha en la que se quiere ingresar al sistema esta en el rango de fechas del inicio y fin de la ExpoMR,
-- de ser asi regresa si es expo vip o expo normal

-- Valores de @Result:
--0 = el dia es de expo normal
--1 = El dia es de expo vip
--2 = El dia no es de expo
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[ValidaExpoVIP]
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Result TINYINT

	SELECT TOP 1 @Result = Result.VIP
	FROM (
			SELECT FechasExpo.FeIni, FechasExpo.FeFin, FechasExpo.VIP
			FROM (
					SELECT DATEADD(HOUR, CAST(dbo.GetSysVar(N'HoraInicio') AS int), CAST(Fecha AS smalldatetime)) AS FeIni,
					DATEADD(HOUR, CAST(dbo.GetSysVar(N'HoraFin') AS int), CAST(Fecha AS smalldatetime)) AS FeFin,
					mCalendario.VIP
					FROM mCalendario
				 ) AS FechasExpo
			WHERE GETDATE() BETWEEN FechasExpo.FeIni AND FechasExpo.FeFin
			UNION
			SELECT GETDATE() AS FeIni, GETDATE() AS FeFin, 2 AS VIP
		 ) AS Result
	ORDER BY Result.VIP ASC

	RETURN @Result
END