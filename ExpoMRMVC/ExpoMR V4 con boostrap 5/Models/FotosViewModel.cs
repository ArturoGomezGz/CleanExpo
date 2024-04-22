using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    [Table("mFotos")]
    public class FotosViewModel
    {
        [Key]
        public int IdFoto { get; set; }
        public int IdProducto { get; set; }
        public byte[] Foto { get; set; }
        public int Atributos { get; set; }
        public string Obvs { get; set; }
        public DateTime FeModifica {  get; set; }
    }
}
