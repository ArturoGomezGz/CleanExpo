using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ExpoMR.Models
{
    public class IsFechaExpo
    {
        public bool FechaExpo { get; set; }
    }

    public class IsFechaExpoVIP 
    {
        public bool FechaVip { get; set; }
    }
}
