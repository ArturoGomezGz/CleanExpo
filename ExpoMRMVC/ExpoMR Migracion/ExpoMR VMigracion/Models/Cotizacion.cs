using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace ExpoMR.Models
{
    public class Cotizacion
    {
        [Key]
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

    }

    public class TipoDocumento
    {
        [Key]
        public int IdDocumento { get; set; }
        public string Documentos { get; set; }
    }

    public class GetUsoCFDI
    {
        [Key]
        public int IdUsoCFDI { get; set; }

        public string UsoCFDI { get; set;}
    
    }

    public class GetTipoVenta 
    {
        [Key]
        public int IdTipoVenta { get; set;}

        public string TipoVenta {get; set;}
        
    }

    public class GetDireccionEntrega
    {
        [Key]
        public int Entrega { get; set; }
        public string DirEntrega { get; set; }

    }
}
