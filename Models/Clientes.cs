using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models 
{
    public class Clientes
    {
        [Key]
        public int IdPersona { get; set; } 
        public string Nombre { get; set; }

        [NotMapped]
        public bool selected { get; set; }
    }

}
