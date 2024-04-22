using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    [Keyless]
    public class Carrito
    {
        [NotMapped]
        public int CantidadCarrito { get; set; }
        [NotMapped]
        public int IdUsuario { get; set; }
        [NotMapped]
        public int IdCliente { get; set; }
        public int Productos { get; set; }
        [NotMapped]
        public bool HasProduct { get; set; }
        public decimal Subtotal { get; set; }
        public decimal IVA { get; set; }
        public decimal Total { get; set; }
        public decimal? Credito { get; set; }
        [NotMapped]
        [Display(Name = "Seleccione un pedido: ")]
        public int IdPedido { get; set; }
        [NotMapped]
        public List<ddlPedidosCliente> PedidosCliente { get; set; }
        [NotMapped]
        [Display(Name = "Tipo de Comprobante: ")]
        public int IdTipoDoc { get; set; }
        public List<TipoDocumentos> TipoDocumentos { get; set; }
        [NotMapped]
        [Display(Name = "Uso del CFDI:")]
        public int IdUsoCFDI { get; set; }
        [NotMapped]
        public List<UsosCFDI> UsoCFDI { get; set; }
        [NotMapped]
        [Display(Name = "Dirección de Entrega: ")]
        public int Entrega { get; set; }
        [NotMapped]
        public List<Entregas> Direccciones { get; set; }
        [NotMapped]
        [Display(Name = "Transportista: ")]
        public int IdTransporte { get; set; }
        [NotMapped]
        public List<Trasportes> Trasportes { get; set; }
        [NotMapped]
        [Display(Name = "Tipo de Envío")]
        public int IdTipoVenta { get; set; }
        [NotMapped]
        public List<TiposDeVenta> TiposVenta { get; set; }
    }

    [Keyless]
    public class ddlPedidosCliente { 
        public int IdPedido { get; set; }
        public string Pedido { get; set; }
    }

    
}
