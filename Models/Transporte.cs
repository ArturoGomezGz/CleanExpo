using Microsoft.EntityFrameworkCore;

namespace ExpoMR.Models
{
    [Keyless]
    public class Transporte
    {
        public int IdCotizacion { get; set; }
        public byte IdTipoVenta { get; set; }    
        public string TipoVenta { get; set; }
        public int Entrega { get; set; }
        public string DirEntrega { get; set; }
        public int? IdTransporte { get; set; }
        public string Transportista { get; set; }
        public byte? IdPorte { get; set; }
        public string Porte { get; set; }
        public string Obsv { get; set; }
    }
}
