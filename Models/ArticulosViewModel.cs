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

        public int Proveedor { get; set; }

        public Int16 Cantidad { get; set; }

        public byte NumFoto { get; set; }
    }
}