using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ExpoMR.Models
{
    public class EditarProveedorViewModel
    {
        public int IdPersona { get; set; }
        [Display(Name = "Nombre: ")]
        public string Proveedor { get; set; }        
        [Display(Name = "Descripción: ")]
        public string Descripcion { get; set; }
        [Display(Name ="Monedas")]
        public int Monedas { get; set; }
    }
}
