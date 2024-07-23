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

            string query = "SELECT Stand.Proveedor, Stand.Nombre, Stand.Descripcion, Stand.Template, Stand.Rank, Stand.Chat, Stand.Paleta, Stand.Logo, Stand.Background FROM GetStandProv(@IdProveedor) AS Stand";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProveedor", HttpContext.Session.GetString("ClienteSel"));

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                ConfiguracionStand.NombreStand = (string)dr["Nombre"];
                ConfiguracionStand.DescripcionStand = (string)dr["Descripcion"];
                ConfiguracionStand.Color = (byte)dr["Paleta"];
                ConfiguracionStand.Logo = (dr["Logo"] != DBNull.Value ? (byte[])dr["Logo"] : WebSite.GetImageDefault(webHostEnvironment, "noimage.png"));
                ConfiguracionStand.Fondo = (dr["Background"] != DBNull.Value ? (byte[])dr["Background"] : WebSite.GetImageDefault(webHostEnvironment, "noimage.png"));
                ConfiguracionStand.Fechas = GetFechas();
            }

            dB.Close();

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

            string query = "SELECT CONVERT(VARCHAR(20), mCalendario.Fecha, 111) AS Fecha, " +
                             "CASE WHEN VIP = 1 THEN (SELECT CAST(Fecha as NVARCHAR(MAX)) + ',' + CAST(MonedasVIP AS NVARCHAR(MAX)) + ',1' FROM mMotivos WHERE IdMotivo = 7) " +
                             "	  WHEN VIP = 0 THEN (SELECT CAST(Fecha as NVARCHAR(MAX)) + ',' + CAST(MonedasExpo AS NVARCHAR(MAX)) + ',0' FROM mMotivos WHERE IdMotivo = 7) " +
                             "END AS Monedas " +
                             "FROM mCalendario";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[0];

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                Fechas fecha = new Fechas();

                fecha.Fecha = (string)dr["Fecha"];
                fecha.Monedas = (string)dr["Monedas"];

                Fechas.Add(fecha);
            }

            dB.Close();

			return (Fechas);     
        }

        public JsonResult GetfechasMoneda()
        {
            string query = "DECLARE @factor INT, @factorvip INT " +
                            "SELECT @factor = MonedasExpo " +
                            "FROM mMotivos " +
                            "WHERE IdMotivo = 7 " +
                            "SELECT @factorvip = MonedasVIP " +
                            "FROM mMotivos " +
                            "WHERE IdMotivo = 7 " +
                            "SELECT @IdProveedor AS Proveedor, " +
                            "ISNULL(CASE WHEN mCalendario.VIP = 1 " +
                            "       THEN dStandsMax.Maximo /@factorvip " +
                            "       ELSE dStandsMax.Maximo /@factor " +
                            "	   END, 0) AS Visitas, " +
                            "ISNULL(dStandsMax.Maximo, 0) AS Maximo, " +
                            "mCalendario.Fecha, mCalendario.VIP " +
                            "FROM dStandsMax RIGHT OUTER JOIN " +
                            "mCalendario ON dStandsMax.Fecha = mCalendario.Fecha " +
                            "WHERE Proveedor = @IdProveedor " +
                            "OR Proveedor IS NULL";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProveedor", HttpContext.Session.GetString("ClienteSel"));

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

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

			dB.Close();

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
            string query = "SELECT dProductosMax.IdProducto, dProductosMax.Fecha, vProductos.Nombre + ' ' + vProductos.Presentacion as Nombre, " +
                            "CASE WHEN mCalendario.VIP = 0 THEN dProductosMax.Maximo / (SELECT MonedasExpo FROM mMotivos WHERE IdMotivo = 8) " +
                            "WHEN mCalendario.VIP = 1 THEN dProductosMax.Maximo / (SELECT MonedasVIP FROM mMotivos WHERE IdMotivo = 8) END AS Visitas, " +
                            "dProductosMax.Maximo " +
                            "FROM mLaboratorios INNER JOIN " +
                            "vProductos ON mLaboratorios.IdLaboratorio = vProductos.IdLaboratorio INNER JOIN " +
                            "dProductosMax ON vProductos.IdProducto = dProductosMax.IdProducto INNER JOIN " +
                            "mCalendario ON dProductosMax.Fecha = mCalendario.Fecha " +
                            "WHERE mLaboratorios.Proveedor = @IdProveedor " +
                            "ORDER BY Fecha ASC";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProveedor", HttpContext.Session.GetString("ClienteSel"));

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

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

			dB.Close();

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
