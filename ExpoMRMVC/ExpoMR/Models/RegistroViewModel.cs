using System.ComponentModel.DataAnnotations;

namespace ExpoMR.Models
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage ="El email es requerido")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage ="La contraseña es obligatoria")]
        [StringLength(50, ErrorMessage ="El {0} debe estar entre al menos {2} caracteres de longitud", MinimumLength = 5)]
        [DataType(DataType.Password)]
        [Display(Name ="Contraseña")]
        public string Password { get; set; }

        [Required(ErrorMessage = "La confirmacion de Contraseña es obligatoria")]
        [Compare("Password", ErrorMessage ="La contraseña y confirmacion no coinciden")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage ="El nombre Es obligatorio")]
        public string Nombre { get; set; }        
        public string Telefono { get; set; }
        public string Ciudad { get; set; }
        [Required(ErrorMessage ="Fecha de nacimiento es requerida")]
        [Display(Name ="Fecha de nacimiento")]
        public bool Estado { get; set; }
    }
}
