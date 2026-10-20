Use [ExpoMR] 
GO
If exists(Select * From sysobjects Where name = 'PromocionesActivasCliente')
drop procedure PromocionesActivasCliente
GO
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
-- Create date: 10 Sept 2020
-- Description:	Regresa las promociones activas para el cliente
-- =============================================
CREATE PROCEDURE PromocionesActivasCliente
	-- Add the parameters for the stored procedure here
	@proveedor int, 
	@Cliente int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select mPromociones.IdPromocion,  mPromociones.Monto, 
	CAST(Case When mPromociones.Cliente is null Then 0 Else 1 End as bit) as PromoCliente,
	ArticulosPromociones.Articulos,
	mPromociones.NumPromos - ISNULL(NumPromociones.Otorgadas, 0) as NumPromos,
	CAST(Case When dPromoCliente.Cliente is null Then 0 Else 1 End as bit) as Obtenida
	From mPromociones inner join
	(Select dPromociones.IdPromocion, COUNT(dPromociones.IdPromocion) as Articulos  
	 From dPromociones 
	 Group by dPromociones.IdPromocion  
	) as ArticulosPromociones on mPromociones.IdPromocion = ArticulosPromociones.IdPromocion left outer join
	(Select dPromoCliente.IdPromocion, COUNT(*) as Otorgadas
	 From dPromoCliente
	 Group by dPromoCliente.IdPromocion
	 ) as NumPromociones on mPromociones.IdPromocion = NumPromociones.IdPromocion left outer join
	dPromoCliente on mPromociones.IdPromocion = dPromoCliente.IdPromocion And dPromoCliente.Cliente = @cliente  
	Where mPromociones.Activa = 1
	And mPromociones.Proveedor = @proveedor 
	And (	mPromociones.Cliente is null 
		Or	mPromociones.Cliente = @cliente)
	Order by PromoCliente desc, NumPromos desc 
END
GO
