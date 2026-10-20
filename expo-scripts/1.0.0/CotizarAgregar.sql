Use [ExpoMR]
GO
If exists (Select * From sysobjects Where name = 'CotizarAgregar')
Begin
	Drop procedure CotizarAgregar
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
-- Create date: 5 sept 2020
-- Description:	Agrega un producto a la cotizacion
-- =============================================
CREATE PROCEDURE CotizarAgregar 
	-- Add the parameters for the stored procedure here
	@cliente	int,
	@idproducto	smallint,
	@unitario	real,
	@cantidad	int,
	@usuario	int
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	If exists (	Select mCotizaciones.IdProducto  
				From mCotizaciones 
				Where mCotizaciones.Cliente = @cliente
				And mCotizaciones.IdProducto = @idproducto)
	Begin
		Update mCotizaciones
		Set mCotizaciones.Cantidad = @cantidad,
		mCotizaciones.Solicitado = 1,
		mCotizaciones.PorConfirmar = 1, 		
		mCotizaciones.Usuario = @usuario,
		mCotizaciones.Fecha = GETDATE()
		Where mCotizaciones.Cliente = @cliente
		And mCotizaciones.IdProducto = @idproducto  
	End
	Else
	Begin
		Insert mCotizaciones (Cliente, IdProducto, Unitario, Descuento, Cantidad, Solicitado, PorConfirmar, Usuario)
		Select @cliente, @idproducto, @unitario, ISNUlL(dProductosDescuento.Descuento, mProductos.Costo * mProductos.Descuento / 100.0), @cantidad, 1, 1, @usuario    
		From mProductos left outer join
		dProductosDescuento on mProductos.IdProducto = dProductosDescuento.IdProducto And dProductosDescuento.Cliente = @cliente 
		Where mProductos.IdProducto = @idproducto  
	End
	
END
GO
