using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ExpoMR.Models
{
    public class RecuperaPasswordViewModel
    {
        public string Email { get; set; }

        [Required(ErrorMessage = "Lacontraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Required(ErrorMessage = "La confirmacion de Contraseña es obligatoria")]
        [Compare("Password", ErrorMessage = "La contraseña y confirmacion no coinciden")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}
