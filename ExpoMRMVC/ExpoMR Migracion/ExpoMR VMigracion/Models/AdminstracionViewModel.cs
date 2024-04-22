using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ExpoMR.Models
{
	public class AdminstracionViewModel
	{
        [Display(Name = "Tipo Usuario: ")]
        public int IdTipoUsuario { get; set; }
        public List<TipoUsuario> TipoUsuario { get; set; }
        [Display(Name = "Ruta: ")]
        public int Ruta { get; set; }
		public List<RutasList> Rutas { get; set; }
        [Display(Name = "Sucursal: ")]
        public int IdSucrsal { get; set; }
		public List<SucursalUsuario> Sucursales { get; set; }

	}
	public class RutasList { 
		public int IdRuta { get; set; }
		public string Ruta { get; set; }
	}
	


}
