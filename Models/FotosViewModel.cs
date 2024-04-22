using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    public class FotosViewModel
    {
        [Key]
        public int IdFoto { get; set; }

        public int IdProducto { get; set; }

        public byte[] Foto { get; set; }
    }
}