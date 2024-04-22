using System.ComponentModel.DataAnnotations;

namespace ExpoMR.Models
{
    public class Promociones
    {
        public Int16 idPromocion { get; set; }

        public double? monto { get; set; }

        public Int16? piezas { get; set; }

        public Int16? idProducto { get; set; }

        public string producto { get; set; }

        public int? idCliente { get; set; }

        public string cliente { get; set; }

        public bool activa { get; set; }

        public byte tipoPromo { get; set; }

        public List<Articulos> articulos { get; set; }
    }

    public class Articulos
    {
        public string nombre { get; set; }

        public Int16 cantidad { get; set; }
    }
}
