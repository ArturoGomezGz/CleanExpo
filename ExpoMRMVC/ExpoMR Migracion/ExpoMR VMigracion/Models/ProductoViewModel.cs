namespace ExpoMR.Models
{
    public class Producto
    {
        public decimal PrecioMR { get; set; }
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Presentacion { get; set; }    
        public string Formula { get; set; }
        public int imgProducto { get; set; }
    }
}
