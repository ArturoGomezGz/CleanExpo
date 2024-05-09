using Microsoft.EntityFrameworkCore;

namespace ExpoMR.Models
{
    public class Transporte
    {
        public int IdCotizacion { get; set; }

        public Int16 IdTipoVenta { get; set; } 
        
        public string TipoVenta { get; set; }

        public int Entrega { get; set; }

        public string DirEntrega { get; set; }

        public int? IdTransporte { get; set; }

        public string Transportista { get; set; }

        public Int16? IdPorte { get; set; }

        public string Porte { get; set; }

        public string Obsv { get; set; }
    }
}
