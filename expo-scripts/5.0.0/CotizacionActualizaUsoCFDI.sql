USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[CotizacionActualizaUsoCFDI]    Script Date: 15/04/2024 12:29:48 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =======================================================================
-- Author:		Jorge Carrazco
-- Create date: 15 Abril 2024
-- Description:	Se obtienen los datos de Transporte del cliente
-- =======================================================================
CREATE OR ALTER PROCEDURE [dbo].[CotizacionActualizaUsoCFDI]
	@IdCliente INT,
	@IdUsuario INT,
	@IdDocumento INT,
	@IdUsoCFDI INT,
	@IdTipoVenta TINYINT,
	@IdTransporte INT,
	@IdPorte TINYINT,
	@IdDireccion INT,
	@Obsv NVARCHAR(250)

AS
BEGIN	
	SET NOCOUNT ON;
	
	DECLARE @idcotizacion INT
	
	EXEC @idcotizacion = dbo.CotizarGetCotizacion  @IdCliente,  @IdUsuario
	
	IF(@IdUsoCFDI <> 0)
	BEGIN 
		UPDATE mCotizaciones 
		SET mCotizaciones.IdDocumento =  @IdDocumento,
			mCotizaciones.IdUsoCFDI = CASE WHEN @IdDocumento = 1 THEN  @IdUsoCFDI 
			                               ELSE 23 --23 S01 Sin Efectos Fiscales 
									  END  
		WHERE mCotizaciones.IdCotizacion = @idcotizacion
	END

	UPDATE mCotizaciones
    SET mCotizaciones.IdTipoVenta = CASE WHEN ISNULL(@IdTipoVenta, 0) = 0 THEN IdTipoVenta ELSE @IdTipoVenta END,
    mCotizaciones.IdTransporte = CASE WHEN ISNULL(@IdTransporte, 0) = 0 THEN IdTransporte ELSE @IdTransporte END,
    mCotizaciones.IdPorte = CASE WHEN ISNULL(@IdPorte, 0) = 0 THEN IdPorte ELSE @IdPorte END,
    mCotizaciones.Entrega = CASE WHEN ISNULL(@IdDireccion, 0) = 0 THEN Entrega ELSE @IdDireccion END,
    mCotizaciones.Obsv = @Obsv
    WHERE mCotizaciones.IdCotizacion = @idcotizacion

END