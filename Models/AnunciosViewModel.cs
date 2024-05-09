using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    public class mAnuncios
    {
        public int IdAnuncio { get; set; }

        public int Proveedor { get; set; }

        public string Anuncio { get; set; }

        public DateTime FeInicio { get; set; }

        public DateTime FeTermino { get; set; }

        public byte Rank { get; set; }
    }

    public class Anuncios
    {
        public string Anuncio { get; set; }
    }
}
