using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ExpoMR.Models
{
    [Keyless]
    public class ReturnStore
    {
        public int Productos { get; set; }
    }
}
