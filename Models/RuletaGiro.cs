using Microsoft.EntityFrameworkCore;

namespace ExpoMR.Models
{
    public class RuletaGiro
    {
        public decimal GradosGiro { get; set; }

        public Int16 IdPremio { get; set; }

        public string Premio { get; set; }

        public string Descripcion { get; set; }

        public byte[] foto { get; set; }
    }
}
