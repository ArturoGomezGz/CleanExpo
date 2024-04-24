using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    [Keyless]
    public class StandVIewModel
    {
        public int Proveedor { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        [NotMapped]
        public string Template { get; set; }

        public byte Rank { get; set; }

        public string Chat { get; set; }

        public byte Paleta { get; set; }

        public byte[] Logo { get; set; }

        public byte[] Background { get; set; }


        [NotMapped]
        public string Activedhcp { get; set; }

        [NotMapped]
        public string Activestatic { get; set; }

        [NotMapped]
        public int IdUsuario { get; set; }

        [NotMapped]
        public string Sal { get; set; }
    }
}