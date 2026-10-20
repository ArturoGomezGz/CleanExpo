USE ExpoMR
GO
/****** Object:  StoredProcedure [dbo].[CotizarGetCotizacion]    Script Date: 06/21/2021 11:59:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Carlos Rivera
-- Create date: 21 Junio 2021
-- Description:	Obtiene el idcotizacion si no existe lo crea
-- =============================================
CREATE PROCEDURE [dbo].[CotizarGetCotizacion]
	@cliente int, 
	@usuario int
AS
BEGIN

	SET NOCOUNT ON;

	DECLARE @idcotizacion INT
	
	SET @idcotizacion = (SELECT mCotizaciones.IdCotizacion
						 FROM mCotizaciones 
						 WHERE mCotizaciones.Cliente = @cliente
						 AND mCotizaciones.Usuario = @usuario)
	
	IF(ISNULL(@idcotizacion, 0)) = 0
	BEGIN
		INSERT mCotizaciones (Cliente, IdDocumento, IdUsoCFDI, IdTipoVenta, IdTransporte, IdPorte, Fiscal, Entrega, Usuario, Fecha)		
		SELECT mClientes.IdPersona, mClientes.IdDocumento, mClientes.IdUsoCFDI, mClientes.IdTipoVenta, mClientes.IdTransporte, mClientes.IdPorte,
		mClientes.Fiscal, mClientes.Entrega, @usuario, GETDATE()
		FROM mClientes
		WHERE mClientes.IdPersona = @cliente
		
		SET @idcotizacion = SCOPE_IDENTITY()
	END

	RETURN @idcotizacion
END
