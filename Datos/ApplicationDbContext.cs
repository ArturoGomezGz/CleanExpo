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

        public DbSet<Busqueda> Busqueda { get; set; }

    }

    public class ApplicationDbContextExpo : DbContext
    {
        //public DbSet<StandVIewModel> Stands { get; set; }

        //public DbSet<mProveedores> mProveedores { get; set; }

        //public DbSet<PremioRuleta> PremioRuletas { get; set; }

        //public DbSet<mRuleta> mRuletas { get; set; }
 
        public DbSet<Promocion> Promociones { get; set; }

        public DbSet<mAnuncios> mAnuncios { get; set; }

        public DbSet<mTipoUsuario> TipoUsuarios { get; set; }

        public DbSet<Empleados> Empleados { get; set; }

        public DbSet<Roles> Roles { get; set; }

        public DbSet<Carrito> Carrito { get; set; }

        public DbSet<ListaCarrito> ListaCarrito { get; set; }

        public DbSet<PedidosCarrito> PedidosCarrito { get; set; }

        public DbSet<DatosFacturacion> DatosFacturacion { get; set; }

        public DbSet<Transporte> Transporte { get; set; }

        public DbSet<ResumenPedido> ResumenPedido { get; set; }

        public DbSet<Premios> Premios { get; set; }

        public DbSet<ddlPedidosCliente> ddlPedidosCliente { get; set; }

        public DbSet<InfoRuletas> InfoRuletas { get; set; }

        public DbSet<RuletaGiro> RuletaGiro { get; set; }

        public DbSet<Anuncios> Anuncios { get; set; }

        public DbSet<MonedasActuales> MonedasActuales { get; set; }

        public DbSet<Clientes> Clientes { get; set; }

        public DbSet<EditarProveedorViewModel> EditarProveedorViewModel { get; set; }

        public DbSet<EditarUsuarioViewModel> EditarUsuarioViewModel { get; set; }

        public DbSet<ReturnStore> ReturnStore { get; set; }

        public DbSet<IsFechaExpo> FechaExpo { get; set; }

        public DbSet<IsFechaExpoVIP> FechaExpoVip { get; set; }

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
        public DbSet<Producto> Producto { get; set; }

        public DbSet<CotizaProducto> CotizaProductos { get; set; }

        public DbSet<TipoDocumentos> TipoDocumentos { get; set; }

        public DbSet<UsosCFDI> UsoCFDI { get; set; }

        public DbSet<Entregas> Entregas { get; set; }

        public DbSet<Trasportes> Trasportes { get; set; }

        public DbSet<TiposDeVenta> TiposDeVenta { get; set; }

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