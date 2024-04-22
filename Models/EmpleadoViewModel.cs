using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    [Keyless]
	public class Empleados
	{
		public int IdPersona { get; set; }

        [Display(Name = "Nombre: ")]
        public string Nombre { get; set; }

        [Display(Name = "Email: ")]
        public string Email { get; set; }

        public string IdRol { get; set; }

		[Display(Name = "Rol Empleado: ")]
        public string Rol { get; set; }

		[NotMapped]
		public List<Roles> Roles { get; set; }

        [NotMapped]
        public bool IsUser { get; set; }
    }

    [Keyless]
    public class Roles { 
		public string IdRol { get; set; }

		public string Role { get; set; }
	}
}