using System.ComponentModel.DataAnnotations;

namespace ExpoMR.Models;

public class ApiAdminPanelViewModel
{
    [Display(Name = "Entorno")]
    [Required]
    public string? Entorno { get; set; }

    // Se calcula automáticamente a partir del entorno + origen actual (no se captura en UI)
    public string? BaseUrl { get; set; }

    [Display(Name = "Usuario")]
    public string? Usuario { get; set; }

    [Display(Name = "Contraseña")]
    public string? Password { get; set; }

    public bool IsAuthenticated { get; set; }

    public string? Mensaje { get; set; }

    // Operación admin seleccionada
    [Display(Name = "Operación")]
    public string? Operacion { get; set; }

    // Campos genéricos para pruebas básicas
    [Display(Name = "Email objetivo")]
    public string? TargetEmail { get; set; }

    [Display(Name = "Rol")]
    public string? TargetRole { get; set; }

    [Display(Name = "Status")]
    public bool? TargetStatus { get; set; }

    [Display(Name = "Nueva contraseña")]
    public string? NewPassword { get; set; }
}
