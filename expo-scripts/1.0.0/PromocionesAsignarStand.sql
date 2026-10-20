USE [ExpoMR]
GO
If exists (select * From sysobjects Where name = 'PromocionesAsignarStand')
Begin
	drop procedure PromocionesAsignarStand
End
-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Rodrigo Cavero
-- Create date: 11 Sep 2020
-- Description:	Asigna las promociones que se ganan al visitar el stand
-- =============================================
CREATE PROCEDURE PromocionesAsignarStand
	-- Add the parameters for the stored procedure here
	@proveedor int,
	@cliente int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Insert dPromoCliente(IdPromocion, Cliente)
	Select vPromociones.IdPromocion, @cliente
	From vPromociones
	Where vPromociones.Proveedor = @proveedor 
	And vPromociones.Monto = 0
	And vPromociones.Restantes > 0
	And (vPromociones.Cliente is null
		Or vPromociones.Cliente  = @cliente)  
	And vPromociones.IdPromocion not in (Select dPromoCliente.IdPromocion
										From dPromoCliente
										Where dPromoCliente.Cliente = @cliente)

END
GO
