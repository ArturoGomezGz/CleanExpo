Use [ExpoMR]
GO

If exists(select * from sysobjects where name = 'vCalendario')
	drop view vCalendario
GO

CREATE VIEW vCalendario AS

Select DATEADD(HOUR, CAST(dbo.GetSysVar('HoraInicio') as int), CAST(MIN(mCalendario.Fecha) as smalldatetime)) as Inicio,
DATEADD(HOUR, CAST(dbo.GetSysVar('HoraFin') as int), CAST(MAX(mCalendario.Fecha) as smalldatetime)) as Fin, 
mCalendario.VIP 
From mCalendario
Group by mCalendario.VIP