using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    public class PremioRuleta
    {
        public Int16 IdPremio { get; set; }

        public string Premio { get; set; }

        public string Descripcion { get; set; }

        public byte IdRuleta { get; set; }

        public byte Seccion { get; set; }

        public byte[] Foto { get; set; }

        public Int16 Cantidad { get; set; }
    }

    public class mRuleta 
    {
        public byte IdRuleta { get; set; }

        public string Ruleta { get; set; }

        public int Monedas { get; set; }
    }
}