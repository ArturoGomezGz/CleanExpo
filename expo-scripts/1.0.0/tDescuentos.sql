-- ================================================
-- Template generated from Template Explorer using:
-- Create Trigger (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- See additional Create Trigger templates for more
-- examples of different Trigger statements.
--
-- This block of comments will not be included in
-- the definition of the function.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Rodrigo Cavero
-- Create date: 30 ago 2020
-- Description:	replica los descuentos a las cotizaciones y pedidos ya en curso
-- =============================================
CREATE TRIGGER dbo.tDescuentos 
   ON  dbo.dProductosDescuento 
   AFTER INSERT, UPDATE, DELETE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here

	Update mCotizaciones
	Set mCotizaciones.Descuento = mProductos.Descuento
	From deleted inner join 
	mProductos on deleted.IdProducto = mProductos.IdProducto 
	Where mCotizaciones.Cliente = deleted.Cliente
	And mCotizaciones.IdProducto = deleted.IdProducto

	Update mPedidos
	Set mPedidos.Descuento = mProductos.Descuento
	From deleted inner join 
	mProductos on deleted.IdProducto = mProductos.IdProducto 
	Where mPedidos.Cliente = deleted.Cliente
	And mPedidos.IdProducto = deleted.IdProducto

	Update mCotizaciones
	Set mCotizaciones.Descuento = inserted.Descuento
	From inserted
	Where mCotizaciones.Cliente = inserted.Cliente
	And mCotizaciones.IdProducto = inserted.IdProducto
	
	Update mPedidos
	Set mPedidos.Descuento = inserted.Descuento
	From inserted
	Where mPedidos.Cliente = inserted.Cliente
	And mPedidos.IdProducto = inserted.IdProducto

END
GO
