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
    [Authorize(Roles = "Cliente,Proveedor")]
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
                    int isFechaExpo = ExpoMR.Clases.WebSite.ValidaDiaExpo();

                    if (isFechaExpo == 1) //1 = Dia Expo
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

        public JsonResult GetSearchGeneral(string Filtro, int IdUsuario)
        {
            ResponseModel rm = new();
            DB dB = new();

            string querySearhTerms = "";
            string[] busqueda = Filtro.Split(' ');
            int n = 1;
            string[] fields = { "vProductos.Nombre", "vProductos.Presentacion", "vProductos.Formula" };

            foreach (string w in busqueda)
            {
                querySearhTerms += string.IsNullOrEmpty(querySearhTerms) ? "(" : "And (";

                foreach (string f in fields)
                {
                    querySearhTerms += f + " like '%" + w + "%' Or ";
                }
                querySearhTerms = querySearhTerms.Substring(0, querySearhTerms.Length - 3) + ")";

                n++;
            }

            string query = "DECLARE @search TTCotizar "
                            + "INSERT @search(IdPersona, IdProducto) "
                            + "SELECT " + IdUsuario + ", vProductos.IdProducto "
                            + "FROM ExpoMR.dbo.vProductos "
                            + "WHERE "
                            + querySearhTerms
                            + "SELECT vProductos.IdProducto, vProductos.Nombre, vProductos.Presentacion, vProductos.Formula, mLaboratorios.Laboratorio, "
                            + "CAST(Precios.Precio - (CAST(Precios.Precio * vProductos.Descuento / 100.0 AS DECIMAL(9, 2)) + "
                            + "CAST(((Precios.Precio - CAST(Precios.Precio * vProductos.Descuento / 100.0 AS DECIMAL(9, 2))) * "
                            + "ISNULL(dProductosDescuento.Descuento,0)) / 100.0 AS DECIMAL(9,2))) AS DECIMAL(9,2)) AS PrecioFinal, "
                            + "mProdEmp.Status "
                            + "FROM ExpoMR.dbo.mLaboratorios INNER JOIN "
                            + "ExpoMR.dbo.vProductos ON mLaboratorios.IdLaboratorio = vProductos.IdLaboratorio INNER JOIN "
                            + "Indigo.dbo.GetMultiPrecioExpo(@search) AS Precios ON Precios.IdProducto = vProductos.IdProducto LEFT OUTER JOIN "
                            + "Indigo.dbo.mProdEmp ON Precios.IdProducto = mProdEmp.IdProducto LEFT OUTER JOIN "
                            + "ExpoMR.dbo.dProductosDescuento ON vProductos.IdProducto = dProductosDescuento.IdProducto AND dProductosDescuento.Cliente = " + IdUsuario + " "
                            + "ORDER BY vProductos.Nombre ASC";

            SqlParameter[] parameters = new SqlParameter[0];
            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, CommandType.Text, "IndigoCS");

            var jsonData = new
            {
                rows = (
                   from row in dr.Cast<System.Data.Common.DbDataRecord>()
                   select new
                   {
                       idProducto = row[0],
                       nombre = row[1],
                       presentacion = row[2],
                       formula = row[3] == DBNull.Value ? "" : row[3],
                       laboratorio = row[4],
                       precioFinal = row[5],
                       status = row[6]
                   }).ToArray()
            };

            rm.Response = true;
            rm.Message = "ok";
            rm.Result = jsonData.rows;

            dB.Close();
            return Json(rm.Result);
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