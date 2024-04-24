using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    public class mSales
    {
        [Key]
        public Int16 IdSal { get; set; }

        public string Sal { get; set; }
       
        public bool Habilitado { get; set; }
    }
}