using ExpoMR.Clases;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Org.BouncyCastle.Asn1.X500;
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

        public ConfiguracionController(IConfiguration config, UserManager<IdentityUser> userManager)
        {
            _configuration = config;
            _userManager = userManager;

        }

        private int getProveedor()
        {
            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            return GeneralesController.GetIdProveedor(username);
        }

        public IActionResult Configuracion()
        {
            int idProveedor = getProveedor();
            ConfiguracionViewModel vwModel = new ConfiguracionViewModel();

            string query = "Select mProveedores.IdPersona, mProveedores.Nombre, mProveedores.Descripcion, "
                               + " mStands.Template, mStands.Paleta"
                               + " From mProveedores inner join"
                               + " mStands on mProveedores.IdPersona = mStands.Proveedor"
                               + " Where mProveedores.IdPersona = " + idProveedor;

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            vwModel.NombreStand = dataTable.Rows[0].Field<string>(1);
            vwModel.DescripcionStand = dataTable.Rows[0].Field<string>(2);
            vwModel.Color = Convert.ToInt16(dataTable.Rows[0].Field<byte>(4));
            vwModel.Logo = GetImg("logo", idProveedor);
            vwModel.Fondo = GetImg("background", idProveedor);
            vwModel.Fechas = GetFechas();

            return View(vwModel);
        }

        [HttpPost]
        public IActionResult GuardarConfiguracion(IFormFile logo, IFormFile fondo, string nombre, string descripcion, int color)
        {
            int idProveedor = getProveedor();
            byte[] logoArray = getImageFromView(logo);
            byte[] fondoArray = getImageFromView(fondo);

            string queryUpdate = "UPDATE mProveedores"
                                 + " SET Nombre = '" + nombre
                                 + "' , Descripcion = '" + descripcion
                                 + "' WHERE IdPersona = " + idProveedor;

            GeneralesController.sqlQuery("ExpoMRCS", queryUpdate);

            string connectionString = _configuration["ConnectionStrings:ExpoMRCS"];

            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand();

            command.Parameters.AddWithValue("@Color", color);
            queryUpdate = "update mStands set Paleta = @Color";

            if (logoArray.Length > 0)
            {
                command.Parameters.AddWithValue("@Logo", logoArray);
                queryUpdate += ", Logo = @Logo";
            }
                
            if (fondoArray.Length > 0)
            {
                command.Parameters.AddWithValue("@Fondo", fondoArray);
                queryUpdate += ", Background = @Fondo";
            }
                
            queryUpdate += " where Proveedor = " + idProveedor;

            command.CommandText = queryUpdate;
            command.Connection = connection;

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();

            return Json(new { success = true });
        }

        private byte[] getImageFromView(IFormFile imagen)
        {
            if (imagen != null && imagen.Length > 0)
            {
                var ms = new MemoryStream();

                Image image = Image.FromStream(imagen.OpenReadStream());
                image.Save(ms, ImageFormat.Jpeg);

                return ms.ToArray();
            }
            return new byte[0];
        }

        public byte[] GetImg(string type, int id)
        {
            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand cmd = new SqlCommand("ExpoGetImage", cnn);
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA;
            cnn.Open();

            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;

            //// 3. add parameter to command, which will be passed to the stored procedure
            cmd.Parameters.Add(new SqlParameter("@type", type));
            cmd.Parameters.Add(new SqlParameter("@id", id));

            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dataTable);
            cnn.Close();
            byte[] fotos = new byte[dataTable.Rows.Count];


            foreach (DataRow row in dataTable.Rows)
            {
                fotos = row.Field<byte[]>(0);
            }

            return fotos;

        }

        public List<Fechas> GetFechas()
        {
            List<Fechas> Fechas = new List<Fechas>();

            string query = "SELECT mCalendario.Fecha, " +
                            " CASE WHEN VIP = 1 THEN (SELECT CAST(Fecha as NVARCHAR(MAX)) + '," +
                            " ' + CAST(MonedasVIP AS NVARCHAR(MAX)) + ',1' " +
                            " FROM mMotivos WHERE IdMotivo = 7)" +
                            " WHEN VIP = 0 THEN(SELECT CAST(Fecha as NVARCHAR(MAX)) + ',' + CAST(MonedasExpo AS NVARCHAR(MAX)) + ',0'" +
                            " FROM mMotivos WHERE IdMotivo = 7) END AS Monedas  " +
                            " FROM mCalendario";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            foreach (DataRow row in dataTable.Rows)
            {
                Fechas fecha = new Fechas();
                fecha.valueFecha = row.Field<string>(1);
                fecha.Fecha = Convert.ToString(row.Field<DateTime>(0));
                Fechas.Add(fecha);
            }

            return Fechas;
        }

        public JsonResult GetfechasMoneda()
        {
            int idProveedor = getProveedor();

            string query = "DECLARE @factor int, @factorvip int"
                              + " SELECT @factor = MonedasExpo"
                              + " FROM mMotivos"
                              + " WHERE IdMotivo = 7" //--Monedas por visita
                              + " SELECT @factorvip = MonedasVIP"
                              + " FROM mMotivos"
                              + " WHERE IdMotivo = 7" //--Monedas por visita
                              + " SELECT dStandsMax.Proveedor, CASE WHEN mCalendario.VIP = 1"
                              + "                              THEN dStandsMax.Maximo / @factorvip"
                              + "                              ELSE dStandsMax.Maximo / @factor END AS Visitas,"
                              + " dStandsMax.Maximo,"
                              + " mCalendario.Fecha, mCalendario.VIP"
                              + " FROM dStandsMax RIGHT OUTER JOIN"
                              + " mCalendario ON dStandsMax.Fecha = mCalendario.Fecha"
                              + " WHERE Proveedor = " + idProveedor + " OR Proveedor IS NULL";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            var jsonData = new
            {
                rows = (
                      from c in dataTable.AsEnumerable()
                      select new
                      {
                          fecha = c[3],
                          mxVisitas = c[1],
                          monedas = c[2]
                      }).ToArray()
            };

            return Json(jsonData.rows);
        }
    }
}
