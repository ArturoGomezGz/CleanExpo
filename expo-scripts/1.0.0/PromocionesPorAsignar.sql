USE [ExpoMR]
GO
If exists (select * From sysobjects Where name = 'PromocionesPorAsignar')
Begin
	drop procedure PromocionesPorAsignar
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
-- Author:		RodrigoCavero
-- Create date: 15 Sep 2020
-- Description:	Regresa todas las promociones que se podrian ganar
--				en base a los productos en su carrito
-- =============================================
CREATE PROCEDURE PromocionesPorAsignar 
	-- Add the parameters for the stored procedure here
	@cliente int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select vPromociones.IdPromocion, vPromociones.Proveedor, vPromociones.Restantes,
	Case When vPromociones.Cliente is null Then 0 Else 1 End as PromoCliente,
	mProveedores.Nombre,  
	ArticulosPromociones.Articulos
	From vPromociones inner join
	mProveedores on vPromociones.Proveedor = mProveedores.IdPersona inner join
	(Select dPromociones.IdPromocion, COUNT(dPromociones.IdPromocion) as Articulos  
	 From dPromociones 
	 Group by dPromociones.IdPromocion  
	) as ArticulosPromociones on vPromociones.IdPromocion = ArticulosPromociones.IdPromocion
	Where vPromociones.Restantes > 0 
	And (vPromociones.Cliente is null
		Or vPromociones.Cliente  = @cliente)  
	And vPromociones.Monto > 0 
	And vPromociones.Monto <= (	Select SUM(vCotizacionPedido.Total)
								From vCotizacionPedido inner join
								mLaboratorios on vCotizacionPedido.IdLaboratorio = mLaboratorios.IdLaboratorio
								Where vCotizacionPedido.Cliente = @cliente
								And mLaboratorios.Proveedor = vPromociones.Proveedor) 
	And vPromociones.IdPromocion not in (Select dPromoCliente.IdPromocion
										From dPromoCliente
										Where dPromoCliente.Cliente = @cliente)

END
GO
