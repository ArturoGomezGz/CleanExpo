using System.ComponentModel.DataAnnotations;

namespace ExpoMR.Models 
{
    public class Busqueda
    {
        [Key]
        public int IdProducto { get; set; } 
        public string Nombre { get; set; }
    }
}
