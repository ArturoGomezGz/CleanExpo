namespace ExpoMR.Models
{
    public class ResumenCompraCliente
    {
        public string Proveedor { get; set; }
        public string Producto { get; set; }
        public int NumPedidos { get; set; }
        public int TotalProds { get; set; }
        public int TotalPiezas { get; set; }
        public decimal TotalCompra { get; set; }
    }
}
