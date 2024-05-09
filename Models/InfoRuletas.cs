using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    public class InfoRuletas
    {
        public byte IdRuleta { get; set; }

        public string Ruleta { get; set; }

        public int Monedas { get; set; }

        public int Activa { get; set; }

        public string RuletaNormalized { get; set; }
    }
}
