using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    public class Usuarios
    {
        [Key]
        public int IdUsuario { get; set; }

        public int IdTipoUsuario { get; set; }

        public int IdEmail { get; set; }

        public string Email { get; set; }

        public int IdPersona { get; set; }

        public string Nombre { get; set; }

        [NotMapped]
        public bool selected { get; set; }

    }
}