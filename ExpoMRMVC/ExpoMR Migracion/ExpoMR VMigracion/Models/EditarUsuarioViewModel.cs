using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ExpoMR.Models
{
    public class EditarUsuarioViewModel
    {
        public int IdPersona { get; set; }
        [Display(Name = "Nombre: ")]
        public string Usuario { get; set; }
        
        [Display(Name = "Email: ")]
        public string Email { get; set; }
        [Display(Name ="Estado")]
        public string Estado { get; set; }
        public string VIP { get; set; }
        public string Lock { get; set; }

        [Display(Name ="Tipo")]
        public int IdTipoUsuario { get; set; }      
        public List<TipoUsuario> TipoUsuario{ get; set; }


        [Display(Name ="Sucursal")]
        public int IdSucursal { get; set; }
        public List<SucursalUsuario> Sucursales { get; set; }

        [Display(Name ="Invitación")]
        public string Invitacion { get; set; }
    }

    public class SucursalUsuario
    {
        public int IdSucrsal { get; set; }
        public string Sucursal { get; set; }
    }

    public class TipoUsuario
    { 
        public int IdTipoUsuario { get; set; }
        public string TipoUsuarios { get; set; }

    }
}
