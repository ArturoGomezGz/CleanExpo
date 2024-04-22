using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    public class Promocion
    {
        [Key]
        public Int16 IdPromocion { get; set; }

        public float? Monto { get; set; }
        
        public Int16? Piezas { get; set; }
        
        public string Producto { get; set; }
        
        public int Disponibilidad { get; set; }
        
        public bool Obtenida { get; set; }
        
        public byte IdTipoPromocion { get; set; }
        
        public string Premios { get; set; }
        
        [NotMapped]
        public string ImgDisponibilidad { get; set;}
    }
}
