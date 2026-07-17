using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    public class Pedidos
    {
        public int IdEmail { get; set; }

        public int IdCliente { get; set; }

        public TotalesPedidos TotalesPedidos { get; set; }

        [Display(Name = "Seleccione un pedido: ")]

        public int IdPedido { get; set; }

        public List<ddlPedidos> PedidosCliente { get; set; }
    }

    public class ddlPedidos
    {
        public int IdPedido { get; set; }

        public string Pedido { get; set; }
    }

    public class TotalesPedidos
    {
        public decimal Subtotal { get; set; }

        public decimal IVA { get; set; }

        public decimal Total { get; set; }

        public decimal Credito { get; set; }

        public int Productos { get; set; }

        public bool HasProduct { get; set; }
    }
}