using System.ComponentModel.DataAnnotations;

namespace ExpoMR.Models
{
    public class OlvidoPasswordViewModel
    {
        [Required(ErrorMessage = "El email es Obligatorio")]
        [EmailAddress]
        public string Email { get; set; }

    }
}
