using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    public class Producto
    {
        [Key]
        public decimal PrecioMR { get; set; }

        public Int16 IdProducto { get; set; }
        
        public string Nombre { get; set; }
        
        public string Presentacion { get; set; }    
        
        public string Formula { get; set; }
        
        public decimal Descuento { get; set; } 
        
        public int DescuentoCLiente { get; set; }
        
        public decimal PrecioFinal { get; set; }
        
        [NotMapped]
        public Int16 imgProducto { get; set; }
        
        [NotMapped]
        public byte[] imgProductos { get; set; }
    }

    [Keyless]
    public class CotizaProducto
    {
        public Int16 IdProducto { get; set; }

        public string Nombre { get; set; }

        public string Presentacion { get; set; }

        public string Unidades { get; set; }

        public decimal Publico { get; set; }

        public decimal Neto { get; set; }

        public bool Iva { get; set; }

        public string Laboratorio { get; set; }

        public Int16 Caja { get; set; }

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
        
        [NotMapped]
        public int IdProveedor { get; set; }

        [NotMapped]
        public byte[] imgProductos { get; set; }
    }
}