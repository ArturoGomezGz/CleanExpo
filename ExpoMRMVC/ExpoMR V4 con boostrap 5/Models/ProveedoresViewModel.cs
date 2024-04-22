using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    public class mProveedores
    {
        [Key]
        public int IdPersona { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public bool Alta { get; set; }

        public StandVIewModel Stand { get; set; }

    }
}