using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    public class ArticulosProveedores
    {
        public Int16 IdArticulo { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public int IDdArticulos { get; set; }

        public byte[] Foto { get; set; }

        [NotMapped]
        public int Proveedor { get; set; }

        [NotMapped]
        public Int16 Cantidad { get; set; }

        [NotMapped]
        public byte NumFoto { get; set; }
    }
}