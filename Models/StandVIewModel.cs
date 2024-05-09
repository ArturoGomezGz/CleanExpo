using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    public class StandVIewModel
    {
        public int Proveedor { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public string Template { get; set; }

        public byte Rank { get; set; }

        public string Chat { get; set; }

        public byte Paleta { get; set; }

        public byte[] Logo { get; set; }

        public byte[] Background { get; set; }

        public string Activedhcp { get; set; }

        public string Activestatic { get; set; }

        public int IdUsuario { get; set; }

        public string Sal { get; set; }
    }
}