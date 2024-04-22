using Microsoft.EntityFrameworkCore;

namespace ExpoMR.Models
{
    public class Monedas
    {
        public int idCliente { get; set; }
        public int idProveedor { get; set; }
        public string nombre { get; set; }
    }

    [Keyless]
    public class MonedasActuales
    {
        public int monedas { get; set; }
        public int monedasContacto { get; set; }
    }
}
