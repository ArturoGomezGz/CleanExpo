using System.ComponentModel.DataAnnotations;

namespace ExpoMR.Models
{
    public class Promociones
    {
        public int idPromocion { get; set; }
        public double? monto { get; set; }
        public int? piezas { get; set; }
        public int? idProducto { get; set; }
        public string producto { get; set; }
        public int? idCliente { get; set; }
        public string cliente { get; set; }
        public bool activa { get; set; }
        public int tipoPromo { get; set; }
        public List<Articulos> articulos { get; set; }
    }

public class Articulos
{
    public string nombre { get; set; }
    public int cantidad { get; set; }
}
}
