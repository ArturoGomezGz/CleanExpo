using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    [Keyless]
    public class DatosFacturacion
    {
        public int IdCotizacion { get; set; }
        public string Cliente { get; set; }
        public string RFC { get; set; }
        public int IdDocumento { get; set; }
        public string Documento { get; set; }
        public int IdUsoCFDI { get; set; }
        public string UsoCFDI { get; set; }
    }
}
