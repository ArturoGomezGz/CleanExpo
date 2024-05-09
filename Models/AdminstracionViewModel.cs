using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace ExpoMR.Models
{
	public class AdminstracionViewModel
	{
        [Display(Name = "Tipo Usuario: ")]
        public int IdTipoUsuario { get; set; }

        public List<mTipoUsuario> TipoUsuario { get; set; }

        [Display(Name = "Ruta: ")]
        public int Ruta { get; set; }

		public List<RutasList> Rutas { get; set; }

        [Display(Name = "Sucursal: ")]
        public int IdSucrsal { get; set; }

		public List<SucursalUsuario> Sucursales { get; set; }
	}

    public class RutasList
	{
		public Int32 IdRuta { get; set; }

		public string Descripcion { get; set; }
	}
}