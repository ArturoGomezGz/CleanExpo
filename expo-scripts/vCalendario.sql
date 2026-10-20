USE [ExpoMR]
GO

/****** Object:  View [dbo].[vCalendario]    Script Date: 17/10/2025 03:25:10 p. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[vCalendario] AS

SELECT DATEADD(HOUR, CAST(dbo.GetSysVar('HoraInicio') AS INT), CAST(MIN(mCalendario.Fecha) AS SMALLDATETIME)) AS Inicio,  
DATEADD(HOUR, CAST(dbo.GetSysVar('HoraFin') as int), CAST(MAX(mCalendario.Fecha) AS SMALLDATETIME)) AS Fin
FROM mCalendario
GO