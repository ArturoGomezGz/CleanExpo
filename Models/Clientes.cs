using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models 
{
    public class Clientes
    {
        public int IdPersona { get; set; } 

        public string Nombre { get; set; }

        public bool selected { get; set; }
    }
}