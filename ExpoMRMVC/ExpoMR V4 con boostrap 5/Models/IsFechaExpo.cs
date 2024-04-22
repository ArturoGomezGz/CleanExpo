using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ExpoMR.Models
{
    [Keyless]
    public class IsFechaExpo
    {
        public bool FechaExpo { get; set; }
    }

    [Keyless]
    public class IsFechaExpoVIP 
    {
        public bool FechaVip { get; set; }
    }
}
