using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpoMR.Models
{
    public class ListaCarrito
    {
        public int Registro { get; set; }

        public int Proveedor { get; set; }

        public string Laboratorio { get; set; }

        public int IdProducto { get; set; }

        public string Nombre { get; set; }

        public string Presentacion { get; set; }

        public int cantidad { get; set; }

        public decimal PrecioFinal { get; set; }

        public decimal total { get; set; }

        public bool PorConfirmar { get; set; }

		public bool Iva { get; set; }

        public byte Status { get; set; }
    }

}
