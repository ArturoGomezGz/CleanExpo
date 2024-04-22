using ExpoMR.Clases;
using ExpoMR.Datos;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Cliente,ClienteVIP")]
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private static IConfiguration _configuration;

        //Variables para verificar La sesión Y el Usuario
        public HomeController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "MiCuenta");
            }
            else
            {
                bool isFechaExpoVIP = ExpoMR.Clases.WebSite.ValidaExpoVIP();

                if (ExpoMR.Clases.WebSite.ValidaRolUsuario(currentUser.Email, "ClienteVIP") == true)
                {
                    if (isFechaExpoVIP == true)
                    {
                        return View();
                    }
                    else
                    {
                        return RedirectToAction("Index", "MiCuenta");
                    }
                }
                else
                {
                    if (isFechaExpoVIP == true)
                    {
                        return RedirectToAction("Index", "MiCuenta");
                    }
                    else
                    {
                        return View();
                    }
                }
            }             
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public static List<StandVIewModel> GetImage()
        {
            List<StandVIewModel>LogosProveedores = new();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[0];
            SqlDataReader dr = dB.ExecuteReader("GetLogoProv", ref parameters);

            while (dr.Read())
            {
                StandVIewModel Logo = new StandVIewModel();

                Logo.Proveedor = (int)dr["Proveedor"];
                Logo.Nombre = (string)dr["Nombre"];
                //Logo.Logo = (byte[])dr["Logo"];

                LogosProveedores.Add(Logo);
            }

            return LogosProveedores;
        }

        public static List<StandVIewModel> GetImageById(int Id)
        {
            using (var dbContext = new ApplicationDbContextExpo())
            {
                return dbContext.Stands.Where(stand => stand.Proveedor.Equals(Id)).ToList();
            }
        }

        public static List<Producto> GetImageProductByLab(int idProveedor, int IdUsuario)
        {
            using (var dbContext = new ApplicationDbContextIndigo())
            {
                var parameters = new object[]
                {
                    idProveedor,
                    IdUsuario
                };

                List<Producto> productos = dbContext.Set<Producto>()
                    .FromSqlRaw("EXEC ExpoStandProductos @proveedor = {0}, @cliente = {1}", parameters)
                    .ToList();

                foreach (var p in productos)
                {
                    p.imgProducto = p.IdProducto;
                    p.imgProductos = GeneralesController.GetFoto(p.IdProducto);
                }
                return productos;
            }
        }

        public static List<PremioRuleta> GetImageRuleta(int idRuleta)
        {
            using (var dbContext = new ApplicationDbContextExpo())
            {
                return dbContext.PremioRuletas.Where(ruleta => ruleta.IdRuleta == idRuleta).ToList();
            }
        }

        public static byte[] GetFotos(int idProducto)
        {
            FotosViewModel FotoProducto = new FotosViewModel();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@idproducto", idProducto);

            SqlDataReader dr = dB.ExecuteReader("GetFotoProd", ref parameters);

            while (dr.Read())
            {
                FotoProducto.IdFoto = (int)dr["IdFoto"];
                FotoProducto.IdProducto = (int)dr["IdProducto"];
                FotoProducto.Foto = (byte[])dr["Foto"];
            }

            return (FotoProducto.Foto);
        }
    }
}