using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ExpoMR.Models
{
	public class EditarEmpleadoViewModel
	{
		public int IdPersona { get; set; }
        [Display(Name = "Nombre: ")]
        public string Empleado { get; set; }
        public string IdRol { get; set; }
		[Display(Name = "Rol Empleado: ")]
        public string Rol { get; set; }
        [Display(Name = "Email: ")]
        public string Email { get; set; }
		public List<RoleIdPersona> Roles { get; set; }
	}

	public class RoleIdPersona { 
		public string IdRol { get; set; }
		public string Role { get; set; }
	}
}
