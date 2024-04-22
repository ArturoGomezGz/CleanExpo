using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ExpoMR.Models
{
    public class ConfiguracionViewModel
    {

        [Required(ErrorMessage = "Ingrese un nombre para mostrar en su stand")]
        [Display(Name = "¿Qué nombre mostrará tu Stand?")]
        public string NombreStand { get; set; }

        [Display(Name = "¿Qué descripción quieres que aparezca debajo de tu nombre?")]
        public string DescripcionStand { get; set; }

        [Display(Name = "¿Qué colores deseas en tus anuncios?")]

        public int Color { get; set; }

        public string Fecha { get; set; }
        public List<Fechas> Fechas { get; set; }

        [Display(Name = "¿Qué imagen usarás como tu Logo?")]
        public byte[] Logo { get; set; }
        
        [Display(Name = "¿Qué imagen usarás como fondo para mostrar en tu Stand?")]
        public byte[] Fondo { get; set; }
    }

    public class Colores
    {
        public int Valor { get; set; }
        public string Color { get; set; }
    }

    public class Fechas { 
        public string valueFecha { get; set; }
        public string Fecha { get; set; }
    }
}
