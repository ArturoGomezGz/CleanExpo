using System.ComponentModel.DataAnnotations;

namespace ExpoMR.Models
{
    public class AccesoViewModel
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Display(Name ="Recordar Datos?")]
        public bool RememberMe { get; set; }
    }
}
