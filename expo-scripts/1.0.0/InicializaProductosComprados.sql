Use [ExpoMR]
GO
If exists (Select * From sysobjects Where name = 'InicializaProductosComprados')
Begin
	Drop procedure InicializaProductosComprados
End
--================================================
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
-- Create date: 3 sep 2020
-- Description:	Inicializa los datos de la tabla de productos comprados
-- =============================================
CREATE PROCEDURE InicializaProductosComprados 
	-- Add the parameters for the stored procedure here
	@cliente int = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Delete dProductosComprados
	Where dProductosComprados.Cliente = @cliente
	Or @cliente is null

	Insert dProductosComprados (IdProducto, Cliente)
	Select dFacturas.IdProducto, mFacturas.IdPersona 
	From mProductos inner join
	Indigo.dbo.dFacturas on mProductos.IdProducto = dFacturas.IdProducto inner join
	Indigo.dbo.mFacturas on dFacturas.IdFactura = mFacturas.IdFactura inner join
	mClientes on mFacturas.IdPersona = mClientes.IdPersona
	Where DATEADD(MONTH, 6, mFacturas.Fecha) > (Select MIN(mCalendario.Fecha) From mCalendario)
	And (mFacturas.IdPersona = @cliente
		Or @cliente is null)
	Group by dFacturas.IdProducto, mFacturas.IdPersona
	
	Insert dProductosComprados (IdProducto, Cliente)
	Select dFacturas.IdProducto, mFacturas.IdPersona 
	From mProductos inner join
	QtroIndigo.dbo.dFacturas on mProductos.IdProducto = dFacturas.IdProducto inner join
	QtroIndigo.dbo.mFacturas on dFacturas.IdFactura = mFacturas.IdFactura inner join
	mClientes on mFacturas.IdPersona = mClientes.IdPersona left outer join
	ExpoMR.dbo.dProductosComprados ON mFacturas.IdPersona = dProductosComprados.Cliente And dFacturas.IdProducto = dProductosComprados.IdProducto
	Where DATEADD(MONTH, 6, mFacturas.Fecha) > (Select MIN(mCalendario.Fecha) From mCalendario)
	And (mFacturas.IdPersona = @cliente
		Or @cliente is null)
	And dProductosComprados.Cliente is null
	Group by dFacturas.IdProducto, mFacturas.IdPersona
	
	Insert dProductosComprados (IdProducto, Cliente)
	Select dFacturas.IdProducto, mFacturas.IdPersona 
	From mProductos inner join
	EdomxIndigo.dbo.dFacturas on mProductos.IdProducto = dFacturas.IdProducto inner join
	EdomxIndigo.dbo.mFacturas on dFacturas.IdFactura = mFacturas.IdFactura inner join
	mClientes on mFacturas.IdPersona = mClientes.IdPersona left outer join
	ExpoMR.dbo.dProductosComprados ON mFacturas.IdPersona = dProductosComprados.Cliente And dFacturas.IdProducto = dProductosComprados.IdProducto
	Where DATEADD(MONTH, 6, mFacturas.Fecha) > (Select MIN(mCalendario.Fecha) From mCalendario)
	And (mFacturas.IdPersona = @cliente
		Or @cliente is null)
	And dProductosComprados.Cliente is null
	Group by dFacturas.IdProducto, mFacturas.IdPersona
END
GO
