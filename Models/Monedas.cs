using Microsoft.EntityFrameworkCore;

namespace ExpoMR.Models
{
    public class Monedas
    {
        public int idCliente { get; set; }
        public int idProveedor { get; set; }
        public string nombre { get; set; }
    }

    public class MonedasActuales
    {
        public int monedas { get; set; }
        public int monedasContacto { get; set; }
    }

    public class SaldoMonedas {

        public int monedasAdquiridas { get; set; }
        public int monedasRepartidas { get; set; }
        public int saldo { get; set; }
        public int monedasVisitaStand { get; set; }
        public int monedasVisitaProd { get; set; }
        public  List<MonedasAsignadas> MonedasAsignadas { get; set; }

    }

    public class MonedasAsignadas { 
    
        public string cliente { get; set; }
        public int monedas { get; set; }
        public DateTime fecha { get; set; }
        public string motivo { get; set; }  
    
    
    }
}
