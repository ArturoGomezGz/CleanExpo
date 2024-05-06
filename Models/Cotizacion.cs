namespace ExpoMR.Models
{
    public class Cotizacion
    {
        public int IdCliente { get; set; }

        public int IdCotizacion { get; set; }

        public string Cliente { get; set; }

        public string RFC { get; set; }

        public int IdDocumento { get; set; }

        public string Documento { get; set;}

        public int IdUsoCFDI { get; set; }

        public string UsoCFDI { get; set;}

        public int Direccion { get; set; }

        public string DirEntrega { get; set; }

        public int IdTipoVenta { get; set; }

        public string TipoVenta { get; set; }

        public int IdTransporte { get; set; }

        public string Transportista { get; set; }

        public string Observaciones { get; set; }

        public int IdPorte { get; set; }

        public string Porte { get; set; } 

        public List<TipoDocumento> TipoDocumentos { get; set; }

        public List<GetUsoCFDI> CFDI { get; set; }

        public List<GetTipoVenta> Venta { get; set; }

        public List<GetDireccionEntrega> Direcciones { get; set; }

        public List<GetTransportista> transportistas { get; set; }

        public List<GetPorte> portes { get; set; }

    }

    public class TipoDocumento
    {
        public int IdDocumento { get; set; }

        public string Documentos { get; set; }
    }

    public class GetUsoCFDI
    {
        public int IdUsoCFDI { get; set; }

        public string UsoCFDI { get; set;}
    
    }

    public class GetTipoVenta 
    {
        public int IdTipoVenta { get; set;}

        public string TipoVenta {get; set;}
        
    }

    public class GetDireccionEntrega
    {
        public int Entrega { get; set; }
        public string DirEntrega { get; set; }
    }

    public class GetTransportista
    {
        public int IdTransporte { get; set; }

        public string Transportista { get; set; }
    }

    public class GetPorte
    {
        public int IdPorte { get; set; }
        public string Porte { get; set; }
    }
}
