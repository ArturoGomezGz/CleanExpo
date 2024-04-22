using System.ComponentModel.DataAnnotations;

namespace ExpoMR.Models
{
    public class AccesoViewModel
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La Contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Display(Name ="Recordar mis Datos")]
        public bool RememberMe { get; set; }
    }
}
