USE [ExpoMR]
GO
/****** Object:  StoredProcedure [dbo].[EditCliente]    Script Date: 22/03/2024 02:22:10 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:	Josue Pulido, Carlos Rivera
-- Create date: 19 Abril 2024
-- Description:	Actualiza los datos del cliente para su facturacion
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[EditCliente] 
	@IdPersona INT,
	@TipoDoc TINYINT,
	@IdUsoCFDI TINYINT,
	@IdTipoVenta TINYINT,
	@IdDirEntrega INT,
	@IdTransporte INT,
	@IdPorte TINYINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;    
	
	UPDATE mClientes
	SET IdDocumento = CASE WHEN @TipoDoc <> '' THEN @TipoDoc ELSE mClientes.IdDocumento END,
		IdUsoCFDI = CASE WHEN @IdUsoCFDI <> '' THEN @IdUsoCFDI ELSE mClientes.IdUsoCFDI END,
		IdTipoVenta = CASE WHEN @IdTipoVenta <> '' THEN @IdTipoVenta ELSE mClientes.IdTipoVenta END,
		Entrega = CASE WHEN @IdDirEntrega <> '' THEN @IdDirEntrega ELSE mClientes.Entrega END,
		IdTransporte = CASE WHEN @IdTransporte <> '' THEN @IdTransporte ELSE mClientes.IdTransporte END,
		IdPorte = CASE WHEN @IdPorte <> '' THEN @IdPorte ELSE mClientes.IdPorte END
	WHERE mClientes.IdPersona = @IdPersona
END