using ExpoMR.Controllers;
using ExpoMR.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpoMR.Datos
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { 
        
        }

        //se agregan los Modelos necesarios
        public DbSet<Cotizacion> Cotizacion { get; set; }
        public DbSet<AppUsuario> AppUsuario { get; set; }
        public DbSet<Busqueda> Busqueda { get; set; }

    }
}
