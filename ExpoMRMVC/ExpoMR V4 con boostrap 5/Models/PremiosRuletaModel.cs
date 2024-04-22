using Microsoft.EntityFrameworkCore;

namespace ExpoMR.Models
{
    [Keyless]
    public class Premios
    {
        public int idArticulo { get; set; }
        public string proveedor { get; set; }
        public int cantidad { get; set; }
        public string Nombre { get; set; }
    }
}
