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
    [Authorize(Roles = "Cliente,ClienteVIP,Proveedor")]
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
                if (ExpoMR.Clases.WebSite.ValidaRolUsuario(currentUser.Email, "Proveedor") == true)
                {
                    return View();
                }
                else
                {
                    int isFechaExpoVIP = ExpoMR.Clases.WebSite.ValidaExpoVIP();

                    if (ExpoMR.Clases.WebSite.ValidaRolUsuario(currentUser.Email, "ClienteVIP") == true)
                    {
                        if (isFechaExpoVIP == 1) //1 = Expo VIP
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
                        if (isFechaExpoVIP == 0) //1 = Expo Normal
                        {
                            return View();
                        }
                        else
                        {
                            return RedirectToAction("Index", "MiCuenta");

                        }
                    }
                }
            }             
        }

        public JsonResult GetSearchGeneral(string Filtro, int IdUsuario)
        {
            List<Producto> productos = new List<Producto>();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@Filtro", Filtro);
            parameters[1] = new SqlParameter("@Cliente", IdUsuario);

            SqlDataReader dr = dB.ExecuteReader("ExpoStandProductosGeneral", ref parameters, CommandType.StoredProcedure, "IndigoCS");

            while (dr.Read())
            {
                Producto producto = new Producto();

                producto.PrecioMR = (decimal)dr["PrecioMR"];
                producto.IdProducto = (Int16)dr["IdProducto"];
                producto.Nombre = (string)dr["Nombre"];
                producto.Presentacion = (string)dr["Presentacion"];
                producto.Formula = (dr["Formula"] != DBNull.Value ? (string)dr["Formula"] : "");
                producto.Descuento = (decimal)dr["Descuento"];
                producto.DescuentoCLiente = (int)(dr["DescuentoCLiente"]);
                producto.PrecioFinal = (decimal)(dr["PrecioFinal"]);

                productos.Add(producto);
            }

            dB.Close();

            return Json(productos);
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

            string query = "SELECT Proveedor.Proveedor, Proveedor.Nombre, Proveedor.Logo FROM GetLogoProv(@IdProveedor) AS Proveedor";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProveedor", "0");
            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                StandVIewModel Logo = new StandVIewModel();

                Logo.Proveedor = (int)dr["Proveedor"];
                Logo.Nombre = (string)dr["Nombre"];
                Logo.Logo = (dr["Logo"] != DBNull.Value ? (byte[])dr["Logo"] : Array.Empty<byte>());

                LogosProveedores.Add(Logo);
            }

			dB.Close();

			return LogosProveedores;
        }
    }
}