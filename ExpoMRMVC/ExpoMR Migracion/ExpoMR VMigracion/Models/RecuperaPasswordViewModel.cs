using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ExpoMR.Models
{
    public class RecuperaPasswordViewModel
    {
        public string Email { get; set; }

        [Required(ErrorMessage = "La Contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Required(ErrorMessage = "La confirmación de Contraseña es obligatoria")]
        [Compare("Password", ErrorMessage = "La Contraseña y la confirmación no coinciden")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}
