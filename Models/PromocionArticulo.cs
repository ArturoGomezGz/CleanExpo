namespace ExpoMR.Models
{
    public class PromocionArticulo
    {       
        public int IdArticulo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int IdArticulos { get; set; }
        public byte[] imgPromocion { get; set; }
    }
}
