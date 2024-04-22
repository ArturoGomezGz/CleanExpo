namespace ExpoMR.Models
{
    public class Promocion
    {
        public int Condicion{ get; set; }
        public string Premio { get; set; }
        public string ImgDisponibilidad { get; set; }
        public int IdTipoPromocion { get; set; }
        public int? Monto { get; set; }
        public string Producto { get; set; }
        public int? Piezas { get; set;}
    }
}
