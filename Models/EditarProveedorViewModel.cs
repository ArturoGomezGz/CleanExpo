using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace ExpoMR.Models
{
    public class EditarProveedorViewModel
    {
        public int IdPersona { get; set; }
        [Display(Name = "Nombre: ")]

        public string Nombre { get; set; }        
        [Display(Name = "Descripción: ")]

        public string Descripcion { get; set; }
        [Display(Name ="Monedas")]

        public int Saldo { get; set; }

        [Display(Name ="Asignar Monedas")]
        public int AsignarMonedas { get; set; }

        public int NumLabAsignados { get; set; }
    }
}