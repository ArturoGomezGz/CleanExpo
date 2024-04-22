using Microsoft.EntityFrameworkCore;

namespace ExpoMR.Models
{
    [Keyless]
    public class ResumenPedido
    {
        public int IdCotizacion { get; set; }
        public string Cliente { get; set; }
        public string RFC { get; set; }
        public string Documento { get; set; }
        public string UsoCFDI { get; set; }
        public string DirEntrega { get; set; }
        public string TipoVenta { get; set; }
        public string Transportista { get; set; }
        public string Porte { get; set; }
        public string Obsv { get; set; }
    }
}
