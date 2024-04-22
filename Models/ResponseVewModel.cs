using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ExpoMR.Models
{
    [Keyless]
    public class Saldos
    {
        public int Saldo { get; set; }
    }

    [Keyless]
    public class ReturnStore
    {
        public int Productos { get; set; }
    }
}
