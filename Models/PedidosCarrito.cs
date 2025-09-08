using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    public class PedidosCarrito
    {
        public Int64 RegistroProveedor { get; set; }

        public int Proveedor { get; set; }

        public Int64 RegistroPedido { get; set; }

        public int IdPedido { get; set; }

        public int IdProducto { get; set; }

        public string Nombre { get; set; }

        public string Presentacion { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioFinal { get; set; }

        public decimal Total { get; set; }

        public string Laboratorio { get; set; }

        public bool Iva { get; set; }
    }
}