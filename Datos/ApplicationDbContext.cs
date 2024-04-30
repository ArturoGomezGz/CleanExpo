#define TESTMODE
using ExpoMR.Controllers;
using ExpoMR.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace ExpoMR.Datos
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        //public DbSet<Busqueda> Busqueda { get; set; }

    }

    public class ApplicationDbContextExpo : DbContext
    {
        public DbSet<Carrito> Carrito { get; set; }

        public DbSet<ListaCarrito> ListaCarrito { get; set; }

        public DbSet<PedidosCarrito> PedidosCarrito { get; set; }

        public DbSet<DatosFacturacion> DatosFacturacion { get; set; }

        public DbSet<Transporte> Transporte { get; set; }

        public DbSet<ResumenPedido> ResumenPedido { get; set; }

        public DbSet<ddlPedidosCliente> ddlPedidosCliente { get; set; }

        public DbSet<ReturnStore> ReturnStore { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
#if (TESTMODE)
            options.UseSqlServer("Server=localhost; Database=ExpoMR; User ID=usrapp;Password=tangeri;Trusted_Connection=false;MultipleActiveResultSets=true");
#else

            options.UseSqlServer("Server=192.168.100.10; Database=ExpoMR; User ID=usrapp;Password=tangeri;Trusted_Connection=false;MultipleActiveResultSets=true");
#endif

        }
    }

    public class ApplicationDbContextIndigo : DbContext
    {
        //public DbSet<Producto> Producto { get; set; }

        public DbSet<CotizaProducto> CotizaProductos { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
#if (TESTMODE)
            options.UseSqlServer("Server=localhost; Database=Indigo; User ID=usrapp;Password=tangeri;Trusted_Connection=false;MultipleActiveResultSets=true");
#else

            options.UseSqlServer("Server=192.168.100.10; Database=Indigo; User ID=usrapp;Password=tangeri;Trusted_Connection=false;MultipleActiveResultSets=true");
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Producto>().HasNoKey();
        }
    }
}