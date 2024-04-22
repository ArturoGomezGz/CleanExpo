using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ExpoMR.Models
{
    public class ProductoExpoViewModel
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Presentacion { get; set; }
        public string Unidades { get; set; }
        public decimal Publico { get; set; }
        public decimal Neto { get; set; }
        public bool Iva { get; set; }
        public string Laboratorio { get; set; }
        public int Caja { get; set; }
        public string Codigo { get; set; }
        public bool Restringido { get; set; }
        public int Cantidad { get; set; }
        public string Formula { get; set; }
        public bool Controlado { get; set; }
        public decimal PrecioMR { get; set; }
        public decimal DescuentoExpo { get; set; }
        public bool DescuentoCliente { get; set; }
        public decimal ValorIva { get; set; }
        public decimal PrecioFinal { get; set; }
        public byte[] imgProductos { get; set; }
    }
}
