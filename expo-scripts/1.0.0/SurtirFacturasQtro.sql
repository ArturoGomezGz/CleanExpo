Use [ExpoMR]
GO
if exists(select * from sysobjects where name = 'SurtirFacturasQtro')
	drop procedure SurtirFacturasQtro
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
-- Create date: 24 Oct 2020
-- Description:	
-- =============================================
CREATE PROCEDURE SurtirFacturasQtro
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Declare @sucursal tinyint = 2
	Declare @InsertedFacturas Table (Idfactura int,
									IdPersona int,
									Fecha smalldatetime,
									Factura nvarchar(3))
	Declare @CreditoUsado Table (IdPersona int,
								CreditoInyectadoUsado real,
								CreditoExcedido real)

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
	BEGIN TRANSACTION;
		
		--Inserto el Maestro
		Insert QtroIndigo.dbo.mFacturas (IdCotizacion, IdPersona, IdDocumento, IdTipoVenta, IdTipoPago, Plazo, Fiscal, Entrega, IdRuta, IdVendedor,
		IdTransporte, IdPorte, Descuento, Vendedor, VentaEspecial, Obsv, Usuario, Fecha, FeAlta, Modifico, FeModifica, IdUsoCFDI)
		Output inserted.IdFactura, inserted.IdPersona, inserted.Fecha, inserted.Obsv
		Into @InsertedFacturas (IdFactura, IdPersona, Fecha, Factura)
		Select 0 As IdCotizacion, mSurtido.Cliente As IdPersona, mClientes.IdDocumento, mClientes.IdTipoVenta, ClienteIndigo.IdTipoPago, ClienteIndigo.Plazo, 
		mClientes.Fiscal, mClientes.Entrega, ClienteIndigo.IdRuta, mRutas.IdPersona As IdVendedor, mClientes.IdTransporte, mClientes.IdPorte, 
		SUM(mPedidos.Descuento * mSurtido.Surtir) as Descuento,
		0 As Vendedor, 0 As VentaEspecial, CAST(mSurtido.Factura as nvarchar(3)) as Obsv, 'ExpoMR' As Usuario, GETDATE() As Fecha, GETDATE() As FeAlta, 
		'ExpoMR' As Modifico, GETDATE() As FeModifica, mClientes.IdUsoCFDI
		From mSurtido Inner Join
		mPedidos on mSurtido.Cliente = mPedidos.Cliente And mSurtido.IdProducto = mPedidos.IdProducto Inner Join 
		mClientes On mSurtido.Cliente = mClientes.IdPersona Inner Join
		QtroIndigo.dbo.mClientes As ClienteIndigo ON mClientes.IdPersona = ClienteIndigo.IdPersona Left Outer Join
		QtroIndigo.dbo.mRutas On ClienteIndigo.IdRuta = mRutas.IdRuta
		Where mSurtido.Sucursal = @sucursal
		Group by mSurtido.Factura, mSurtido.Cliente, mClientes.IdDocumento, mClientes.IdTipoVenta, ClienteIndigo.IdTipoPago, 
		ClienteIndigo.Plazo, mClientes.Fiscal, mClientes.Entrega, ClienteIndigo.IdRuta, mRutas.IdPersona, mClientes.IdTransporte, 
		mClientes.IdPorte, mClientes.IdUsoCFDI
		Order by mSurtido.Cliente, mSurtido.Factura

		Insert mFacturas(Sucursal, IdFactura, IdPersona, Fecha)
		Select @sucursal as Sucursal, InsertedFacturas.Idfactura, InsertedFacturas.IdPersona, InsertedFacturas.Fecha 
		From @InsertedFacturas as InsertedFacturas


		--Inserto el detalle
		Insert QtroIndigo.dbo.dFacturas (IdFactura, IdProducto, IdCondicion, Unitario, Publico, Costo, SinCargo, Iva, Demanda, ConCargo)
		Select InsertedFacturas.IdFactura, Surtido.IdProducto, 1 As IdCondicion, mPedidos.Unitario, mProdEmp.Publico, mProductos.Costo,
		0 As SinCargo, CAST(mProductos.Iva * QtroIndigo.dbo.GetIva(1) As TinyInt) As Iva, Surtido.Demanda As Demanda,
		Case When Surtido.Inventario >= Surtido.Acumulado 
			Then Surtido.Demanda 
			Else Surtido.Inventario - (Surtido.Acumulado - Surtido.Demanda)  End as ConCargo
		From (	Select mSurtido.Cliente, mSurtido.IdProducto, mSurtido.Factura, 
				mSurtido.Surtir as Demanda, mProdVenta.Cantidad as Inventario,
					(Select SUM(SurtidoAcumulado.Surtir)
					From mSurtido as SurtidoAcumulado
					Where SurtidoAcumulado.IdProducto = mSurtido.IdProducto
				And SurtidoAcumulado.IdPedido <= mSurtido.IdPedido
				And SurtidoAcumulado.Sucursal = @sucursal) as Acumulado
				From mSurtido Inner Join
				QtroIndigo.dbo.mProdVenta on mSurtido.IdProducto = mProdVenta.IdProducto
				Where mSurtido.Sucursal = @sucursal
				And mProdVenta.IdCondicion = 1
			) as Surtido Inner Join
		@InsertedFacturas as InsertedFacturas on Surtido.Cliente = InsertedFacturas.IdPersona And Surtido.Factura = CAST(InsertedFacturas.Factura as smallint) Inner Join
		mPedidos on Surtido.Cliente = mPedidos.Cliente And Surtido.IdProducto = mPedidos.IdProducto Inner Join
		mProductos On Surtido.IdProducto = mProductos.IdProducto Inner Join
		QtroIndigo.dbo.mProdEmp On Surtido.IdProducto = mProdEmp.IdProducto 

		Insert dFacturas (Sucursal, IdFactura, IdProducto, Unitario, ConCargo)
		Select @sucursal as Sucursal, dFacturas.IdFactura, dFacturas.IdProducto, dFacturas.Unitario, dFacturas.ConCargo
		From @InsertedFacturas as InsertedFacturas Inner Join
		QtroIndigo.dbo.dFacturas on InsertedFacturas.IdFactura = dFacturas.IdFactura

		--actualizo el descuento
		Update QtroIndigo.dbo.mFacturas
		Set mFacturas.Descuento = Facturas.Descuento
		From (	Select mFacuturasBase.IdFactura, (100.0 * mFacuturasBase.Descuento ) / SUM(dFacturas.ConCargo * dFacturas.Unitario) as Descuento
				From @InsertedFacturas as InsertedFacturas Inner Join
				QtroIndigo.dbo.mFacturas as mFacuturasBase on InsertedFacturas.Idfactura = mFacuturasBase.IdFactura Inner Join
				QtroIndigo.dbo.dFacturas on mFacuturasBase.IdFactura = dFacturas.IdFactura
				Group by mFacuturasBase.IdFactura, mFacuturasBase.Descuento
			) as Facturas
		Where mFacturas.IdFactura = Facturas.IdFactura


		--Insertar el Kardex
		Insert QtroIndigo.dbo.mKardex (IdProducto, IdTipoMovimiento, Cantidad, Usuario, Fecha, Inventario, Referencia)
		Select ProductosFacturados.IdProducto, 6 as IdTipoMovimiento, ProductosFacturados.ConCargo as Cantidad, 
		'ExpoMR' as Usuario, GETDATE() as Fecha, SUM(ISNULL(mProdVenta.Cantidad, 0)) - ProductosFacturados.Acumulado as Inventario, 
		ProductosFacturados.IdFactura as Referencia
		From (	Select dFacturas.IdFactura, dFacturas.IdProducto, dFacturas.ConCargo,
				ISNULL((Select SUM(FacturaAcumulado.ConCargo)
						From QtroIndigo.dbo.dFacturas as FacturaAcumulado Inner Join
						@InsertedFacturas as InsertedFacturas on FacturaAcumulado.IdFactura = InsertedFacturas.IdFactura
						Where FacturaAcumulado.IdProducto = dFacturas.IdProducto
						And FacturaAcumulado.IdFactura < dFacturas.IdFactura), 0) as Acumulado
				From @InsertedFacturas as InsertedFacturas Inner Join
				QtroIndigo.dbo.dFacturas on InsertedFacturas.Idfactura = dFacturas.IdFactura
			) as ProductosFacturados left outer join
		QtroIndigo.dbo.mProdVenta on ProductosFacturados.IdProducto = mProdVenta.IdProducto
		Group by ProductosFacturados.IdFactura, ProductosFacturados.IdProducto, ProductosFacturados.ConCargo, ProductosFacturados.Acumulado 			


		--Eliminar el Inventario
		Update QtroIndigo.dbo.mProdVenta
		Set mProdVenta.Cantidad = mProdVenta.Cantidad - ProductosFacturados.Inventario 
		From (	Select dFacturas.IdProducto, dFacturas.IdCondicion, 
				SUM(dFacturas.ConCargo) as Inventario
				From QtroIndigo.dbo.dFacturas Inner Join
				@InsertedFacturas as InsertedFacturas on dFacturas.IdFactura = InsertedFacturas.Idfactura
				Group by dFacturas.IdProducto, dFacturas.IdCondicion) as ProductosFacturados
		Where mProdVenta.IdProducto = ProductosFacturados.IdProducto
		And mProdVenta.IdCondicion = ProductosFacturados.IdCondicion


		--Asigna el inventario
		Insert QtroIndigo.dbo.mProdAsignado (IdProducto, Cantidad, IdTipoAsignado, Identificador, IdCondicion, Fecha, Usuario)
		Select dFacturas.IdProducto, dFacturas.ConCargo as Cantidad, 1 as IdTipoAsignado, mFacturas.IdFactura as Identificador, 
		dFacturas.IdCondicion, GETDATE() as Fecha, mFacturas.Usuario     
		From QtroIndigo.dbo.mFacturas Inner Join
		@InsertedFacturas as InsertedFacturas on mFacturas.IdFactura = InsertedFacturas.Idfactura Inner Join
		QtroIndigo.dbo.dFacturas on mFacturas.IdFactura = dFacturas.IdFactura 

	COMMIT TRANSACTION;

	SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;
	BEGIN TRANSACTION;
		--descuenta los creditos utilizados
		--aparto los creditos usados en el proceso
		Insert @CreditoUsado (IdPersona, CreditoInyectadoUsado, CreditoExcedido)
		Select CreditoUsado.IdPersona, CreditoUsado.CreditoInyectadoUsado, CreditoUsado.CreditoExcedido
		From (	Select vSaldoFacturas.IdPersona, 
				Case When vCredito.CreditoNeto < 0 Then -vCredito.CreditoNeto Else 0 End as CreditoInyectadoUsado,
				vCredito.CreditoNeto + vCredito.CreditoInyectado - SUM(vSaldoFacturas.Total) as CreditoExcedido
				From @InsertedFacturas as InsertedFacturas Inner Join
				QtroIndigo.dbo.vSaldoFacturas on InsertedFacturas.IdFactura = vSaldoFacturas.IdFactura Inner Join
				QtroIndigo.dbo.vCredito on vSaldoFacturas.IdPersona = vCredito.IdPersona
				Group by vSaldoFacturas.IdPersona, vCredito.Credito, vCredito.CreditoNeto, vCredito.CreditoInyectado ) as CreditoUsado
		Where CreditoUsado.CreditoInyectadoUsado > 0

		--Inserto el credito extra que necesito
		Insert QtroIndigo.dbo.mCredito (IdPersona, Fecha, Monto, Vencimiento, Usuario)
		Select CreditoUsado.IdPersona, GETDATE() as Fecha, - CreditoUsado.CreditoExcedido as Monto, 
		DATEADD(d, 1, GETDATE()) as Vencimiento, 'Sistema' as Usuario
		From @CreditoUsado as CreditoUsado
		Where CreditoUsado.CreditoExcedido < 0

		--Inserto el remanente de los creditos que sobraron
		Insert QtroIndigo.dbo.mCredito (IdPersona, Fecha, Monto, Vencimiento, Usuario)
		Select Creditos.IdPersona, Creditos.Fecha, Creditos.Acumulado - Creditos.CreditoInyectadoUsado as Monto,
		Creditos.Vencimiento, Creditos.Usuario
		From (	Select mCredito.IdCredito, mCredito.IdPersona, mCredito.Fecha, mCredito.Monto, 
					mCredito.Vencimiento, mCredito.Usuario, CreditoUsado.CreditoInyectadoUsado,
					(Select SUM(CreditoAcumulado.Monto) 
					From QtroIndigo.dbo.mCredito as CreditoAcumulado
					Where CreditoAcumulado.IdCotizacion is null
					And CreditoAcumulado.Vencimiento >= GETDATE()
					And CreditoAcumulado.IdPersona = mCredito.IdPersona
					And (CreditoAcumulado.Fecha < mCredito.Fecha
						Or (CreditoAcumulado.Fecha = mCredito.Fecha 
							And CreditoAcumulado.IdCredito < mCredito.IdCredito))
					) as Acumulado
				From QtroIndigo.dbo.mCredito Inner Join
				@CreditoUsado as CreditoUsado on mCredito.IdPersona = CreditoUsado.IdPersona
				Where mCredito.IdCotizacion is null
				And mCredito.Vencimiento >= GETDATE()
			) as Creditos
		Where Creditos.Acumulado - Creditos.Monto <= Creditos.CreditoInyectadoUsado
		And Creditos.Acumulado - Creditos.CreditoInyectadoUsado > 0
	
		--cierro los creditos usados
		Update QtroIndigo.dbo.mCredito
		Set mCredito.Monto = Creditos.Monto - (Case	When Creditos.Acumulado - Creditos.CreditoInyectadoUsado > 0 
													Then Creditos.Acumulado - Creditos.CreditoInyectadoUsado  
													Else 0 End),
		mCredito.IdCotizacion = 0
		From (	Select mCredito.IdCredito, mCredito.IdPersona, mCredito.Fecha, mCredito.Monto, 
					mCredito.Vencimiento, mCredito.Usuario, CreditoUsado.CreditoInyectadoUsado,
					(Select SUM(CreditoAcumulado.Monto) 
					From QtroIndigo.dbo.mCredito as CreditoAcumulado
					Where CreditoAcumulado.IdCotizacion is null
					And CreditoAcumulado.Vencimiento >= GETDATE()
					And CreditoAcumulado.IdPersona = mCredito.IdPersona
					And (CreditoAcumulado.Fecha < mCredito.Fecha
						Or (CreditoAcumulado.Fecha = mCredito.Fecha 
							And CreditoAcumulado.IdCredito < mCredito.IdCredito))
					) as Acumulado
				From QtroIndigo.dbo.mCredito Inner Join
				@CreditoUsado as CreditoUsado on mCredito.IdPersona = CreditoUsado.IdPersona
				Where mCredito.IdCotizacion is null
				And mCredito.Vencimiento >= GETDATE()
			) as Creditos
		Where mCredito.IdCredito = Creditos.IdCredito 
		And Creditos.Acumulado - Creditos.Monto <= Creditos.CreditoInyectadoUsado

	COMMIT TRANSACTION;

	Delete @InsertedFacturas
	Delete @CreditoUsado

END
GO
