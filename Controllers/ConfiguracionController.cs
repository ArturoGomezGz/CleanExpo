using ExpoMR.Clases;
using ExpoMR.Datos;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Proveedor")]
    public class ConfiguracionController : Controller
    {
        private static IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ConfiguracionController(IConfiguration config, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = config;
            _userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Configuracion()
        {
            ConfiguracionViewModel ConfiguracionStand = new();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProveedor", HttpContext.Session.GetString("ClienteSel"));

            SqlDataReader dr = dB.ExecuteReader("GetStandProv", ref parameters);

            while (dr.Read())
            {
                ConfiguracionStand.NombreStand = (string)dr["Nombre"];
                ConfiguracionStand.DescripcionStand = (string)dr["Descripcion"];
                ConfiguracionStand.Color = (byte)dr["Paleta"];
                ConfiguracionStand.Logo = (dr["Logo"] != DBNull.Value ? (byte[])dr["Logo"] : Array.Empty<byte>());
                ConfiguracionStand.Fondo = (dr["Background"] != DBNull.Value ? (byte[])dr["Background"] : Array.Empty<byte>());
                ConfiguracionStand.Fechas = GetFechas();
            }

            return View(ConfiguracionStand);
        }

        [HttpPost]
        public IActionResult GuardarConfiguracion(IFormFile logo, IFormFile fondo, string nombre, string descripcion, int color)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[6];
            parameters[0] = new SqlParameter("@IdProveedor", HttpContext.Session.GetString("ClienteSel"));
            parameters[1] = new SqlParameter("@Nombre", nombre);
            parameters[2] = new SqlParameter("@Descripcion", descripcion);
            parameters[3] = new SqlParameter("@Paleta", color);
            parameters[4] = new SqlParameter("@Logo",  WebSite.ConvertToByteArray(logo) );
            parameters[5] = new SqlParameter("@Background", WebSite.ConvertToByteArray(fondo));

            dB.ExecuteNonQuery("EditStandProv", ref parameters);

            return Json(new { success = true });
        }

        public List<Fechas> GetFechas()
        {
            List<Fechas> Fechas = new();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[0];

            SqlDataReader dr = dB.ExecuteReader("GetFechasMonedas", ref parameters);

            while (dr.Read())
            {
                Fechas fecha = new Fechas();

                fecha.Fecha = (string)dr["Fecha"];
                fecha.Monedas = (string)dr["Monedas"];

                Fechas.Add(fecha);
            }
            return (Fechas);     
        }

        public JsonResult GetfechasMoneda()
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProveedor", HttpContext.Session.GetString("ClienteSel"));

            SqlDataReader dr = dB.ExecuteReader("GetDetalleFechasProv", ref parameters);

            var jsonData = new
            {
                rows = (
                        from row in dr.Cast<System.Data.Common.DbDataRecord>()
                        select new
                        {
                            visitas = row["Visitas"],
                            monedas = row["Maximo"],
                            fecha = row["Fecha"].ToString().Split(' ')[0]
                        }).ToArray()
            };

            return Json(jsonData.rows);
        }

        [HttpPost]
        public void AgregarDia(int visitas, string fecha)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@proveedor", HttpContext.Session.GetString("ClienteSel"));
            parameters[1] = new SqlParameter("@fecha", fecha);
            parameters[2] = new SqlParameter("@visitas", visitas);

            dB.ExecuteNonQuery("StandMaximo", ref parameters);
        }

        public JsonResult GetProductosMax()
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProveedor", HttpContext.Session.GetString("ClienteSel"));

            SqlDataReader dr = dB.ExecuteReader("GetDetalleFechasProdProv", ref parameters);

            var jsonData = new
            {
                rows = (
                        from row in dr.Cast<System.Data.Common.DbDataRecord>()
                        select new
                        {
                        idProducto = row["IdProducto"],
                        fecha = row["Fecha"].ToString().Split(' ')[0],
                        nombre = row["Nombre"],
                        visitas = row["Visitas"],
                        maximo = row["Maximo"]
                        }).ToArray()
            };

            return Json(jsonData.rows);
        }

        [HttpPost]
        public void AgregarProductosMaximo(int visitas, string fecha, int idProducto)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[4];
            parameters[0] = new SqlParameter("@proveedor", HttpContext.Session.GetString("ClienteSel"));
            parameters[1] = new SqlParameter("@fecha", DateTime.Parse(fecha).ToString("yyyy-MM-dd"));
            parameters[2] = new SqlParameter("@visitas", visitas);
            parameters[3] = new SqlParameter("@IdProducto", idProducto);

            dB.ExecuteNonQuery("StandProductosMaximo", ref parameters);
        }
    }
}
