using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace ExpoMR.Models
{
    public class EditarUsuarioViewModel
    {
        public int IdPersona { get; set; }

        [Display(Name = "Nombre: ")]
        public string Nombre { get; set; }

		public int IdEmail { get; set; }

		[Display(Name = "Email: ")]
        public string Email { get; set; }
        
        [Display(Name ="Estado: ")]
        public string Estado { get; set; }
        
        public bool VIP { get; set; }
        
        public bool VIPvalue { get; set; }

        public bool Lock { get; set; }

        public string isLock { get; set; }

        [Display(Name ="Tipo")]
        public int IdTipoUsuario { get; set; }

        [Display(Name ="Tipo Usuario: ")]
        public string TipoUsuario{ get; set; }

        public byte IdSucursal { get; set; }

        [Display(Name ="Sucursal: ")]
        public string Sucursal { get; set; }

        public List<SucursalUsuario> Sucursales { get; set; }

        [Display(Name ="Invitación: ")]
        public string Invitacion { get; set; }


        [Required(ErrorMessage = "La Contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]

        public string Password { get; set; }

        [Required(ErrorMessage = "La confirmación de Contraseña es obligatoria")]
        [Compare("Password", ErrorMessage = "La Contraseña y confirmación no coinciden")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmPassword { get; set; }

        public bool Usuario { get; set; }
    }

    public class SucursalUsuario
    {
        public int IdSucursal { get; set; }

        public string Sucursal { get; set; }
    }

    public class mTipoUsuario
    {
        public byte IdTipoUsuario { get; set; }

        public string TipoUsuario { get; set; }
    }
}