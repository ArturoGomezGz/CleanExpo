namespace ExpoMR.Models
{
    public class ResumenPedido
    {
        public int Pedido { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        public decimal IVA { get; set; }
        public decimal Total { get; set; }
    }
}
