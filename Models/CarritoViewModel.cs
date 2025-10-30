using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    public class Carrito
    {
        public int CantidadCarrito { get; set; }

        public int IdEmail { get; set; }

        public int IdCliente { get; set; }
        
        public Totales TotalesCotizacion { get; set; }

        public Totales TotalesPedidos { get; set; }

        //[Display(Name = "Seleccione un pedido para conocer el detalle: ")]
        
        //public int IdPedido { get; set; }
           
        //public List<ddlPedidosCliente> PedidosCliente { get; set; }
        
        [Display(Name = "Tipo de Comprobante: ")]
        public int IdTipoDoc { get; set; }
        
        public List<TipoDocumentos> TipoDocumentos { get; set; }
        
        [Display(Name = "Uso del CFDI:")]
        public int IdUsoCFDI { get; set; }
        
        public List<UsosCFDI> UsoCFDI { get; set; }
        
        [Display(Name = "Dirección de Entrega: ")]
        
        public int Entrega { get; set; }
        
        public List<Entregas> Direccciones { get; set; }
        
        [Display(Name = "Transportista: ")]
        public int IdTransporte { get; set; }
        
        public List<Trasportes> Trasportes { get; set; }

        [Display(Name = "Porte: ")]
        public int IdPorte { get; set; }

        public List<Portes> Portes { get; set; }

        [Display(Name = "Tipo de Envío")]
        public int IdTipoVenta { get; set; }

        
        public List<TiposDeVenta> TiposVenta { get; set; }

        public List<ListaCarrito> ListaCarrito { get; set; }
    }

   //public class ddlPedidosCliente 
   // { 
   //     public int IdPedido { get; set; }
    
   //     public string Pedido { get; set; }
   // }

    public class Totales 
    {
        public decimal Subtotal { get; set; }

        public decimal IVA { get; set; }

        public decimal Total { get; set; }

        public decimal Credito { get; set; }

        public int Productos { get; set; }

        public bool HasProduct { get; set; }
    }
}