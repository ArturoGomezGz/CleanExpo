using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ExpoMR.Models
{
    public class MiCuentaViewModel
    {
        [Required(ErrorMessage = "La Contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Required(ErrorMessage = "La confirmación de Contraseña es obligatoria")]
        [Compare("Password", ErrorMessage = "La Contraseña y confirmación no coinciden")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmPassword { get; set; }

        public string RFC { get; set; }

        public string Nombre { get; set; }
        
        public string Direccion { get; set; }

        [Display(Name = "Tipo de Comprobante: ")]

        public int IdTipoDoc { get; set; }

        public List<TipoDocumentos> TipoDocumentos { get; set; }

        [Display(Name = "Uso del CFDI:")]
        public int IdUsoCFDI { get; set; }

        public List<UsosCFDI> UsoCFDI { get; set; }

        [Display(Name = "Tipo de Envío")]
        public int IdTipoVenta { get; set; }

        public List<TiposDeVenta> TiposVenta { get; set; }

        [Display(Name = "Dirección de Entrega: ")]
        public int Entrega { get; set; }

        public List<Entregas> Direccciones { get; set; }

        [Display(Name = "Transportista: ")]
        public int IdTransporte { get; set; }

        public List<Trasportes> Trasportes { get; set; }
        
        [Display(Name ="Porte")]
        public int IdPorte { get; set; }

        public List<Portes> Portes { get; set; }
    }

    public class TipoDocumentos
    { 
        public Int16 IdTipoDoc { get; set; }  

        public string TipoDoc { get; set; }
    }

    public class UsosCFDI
    {
        public Int16 IdUsoCFDI { get; set; }

        public string UsoCFDI { get; set; }
    }

    public class TiposDeVenta
    { 
        public int IdTipoVenta { get; set; }

        public string TipoVenta { get; set; }
    }

    public class Entregas
    {
        public int IdEntrega { get; set; }

        public string Entrega { get; set; }
    }

    public class Trasportes
    { 
        public int IdTrasporte { get; set; }

        public string Transporte { get; set; }
    }

    public class Portes
    { 
        public int Idporte { get; set; }

        public string Porte { get; set; }
    }
}