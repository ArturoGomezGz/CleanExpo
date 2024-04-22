using ExpoMR.Clases;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Org.BouncyCastle.Math.EC.ECCurve;
using System.Data;
using System.Diagnostics;

namespace ExpoMR.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _rolManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private static IConfiguration _configuration;

        //Variables para verificar La sesión Y el Usuario
        public HomeController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender, RoleManager<IdentityRole> rolManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _rolManager = rolManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _rolManager = rolManager;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var currentUsers = await _userManager.GetUserAsync(User);
                // Resto del código...
            }
            var currentUser = await _userManager.GetUserAsync(User);
            
            if (await _userManager.IsInRoleAsync(currentUser, "Administrador"))
            {
                return RedirectToAction("Usuarios", "Administracion"); //Accion, Controlador
            }
            if (await _userManager.IsInRoleAsync(currentUser, "Vendedor"))
            {
                return RedirectToAction("Cotizacion", "Empleados"); //Accion, Controlador
            }
            if (await _userManager.IsInRoleAsync(currentUser, "Proveedor"))
            {
                return RedirectToAction("Estadisticas", "Estadisticas"); //Accion, Controlador
            }
            if (await _userManager.IsInRoleAsync(currentUser, "Capturista"))
            {
                return RedirectToAction("Cotizacion", "Empleados"); //Accion, Controlador
            }            

            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "MiCuenta");
            }
            
            return View();
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
        public static List<Img> GetImage()
        {            
            string query = "Select mProveedores.IdPersona as Proveedor, mProveedores.Nombre, mStands.Logo"
                            + " From mStands inner join mProveedores on "
                            + " mStands.Proveedor = mProveedores.IdPersona Order by mStands.Rank asc";
            DataTable dataTable = sqlQueryDT("ExpoMRCS", query);

            List<Img> ModelStands = new List<Img>();

            foreach (DataRow row in dataTable.Rows)
            {
                Img stand = new Img();
                stand.Proveedor = row.Field<int>(0);
                stand.Nombre = row.Field<string>(1);
                stand.img = row.Field<byte[]>(2);
                ModelStands.Add(stand);
            }

            return ModelStands;
        }
        public static List<Img> GetImageById(int Id)
        {
            string query = "Select mProveedores.IdPersona as Proveedor, mProveedores.Nombre, mStands.Logo From mStands inner join mProveedores"
                            +" on mStands.Proveedor = mProveedores.IdPersona where mProveedores.IdPersona = "+Id+""
                            +" Order by mStands.Rank asc ";
            DataTable dataTable = sqlQueryDT("ExpoMRCS", query);          
            
            List<Img> ModelStands = new List<Img>();

            foreach (DataRow row in dataTable.Rows)
            {
                Img stand = new Img();
                stand.Proveedor = row.Field<int>(0);
                stand.Nombre = row.Field<string>(1);
                stand.img = row.Field<byte[]>(2);
                ModelStands.Add(stand);
            }


            return ModelStands;
        }
        public static List<Producto> GetImageProductByLab(int idProveedor, int IdUsuario)
        {
            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:IndigoCS"]);
            SqlCommand cmd = new SqlCommand("ExpoStandProductos", cnn);
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA; 
            cnn.Open();            

            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;

            //// 3. add parameter to command, which will be passed to the stored procedure
            cmd.Parameters.Add(new SqlParameter("@proveedor", idProveedor));
            cmd.Parameters.Add(new SqlParameter("@cliente", IdUsuario));

            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dataTable);
            cnn.Close();

            List<Producto> ProductStands = new List<Producto>();


            foreach (DataRow row in dataTable.Rows)
            {
                Producto producto = new Producto();
                producto.PrecioMR = Convert.ToDecimal(row.Field<Single>(0));
                producto.IdProducto =Convert.ToInt32(row.Field<Int16>(1));
                producto.Nombre = row.Field<string>(2);
                producto.Presentacion = row.Field<string>(3);
                producto.Formula = row.Field<string>(4);
                producto.imgProducto = producto.IdProducto; //GetFotos(producto.IdProducto);
                ProductStands.Add(producto);
            }

            return ProductStands;
        }
        public static List<PremioRuleta> GetImageRuleta(int idRuleta)
        {
            string query = "Select mPremios.IdPremio, mPremios.Premio, mPremios.Foto"
                               + " From mPremios"
                               + " Where mPremios.IdRuleta = " + idRuleta;
            DataTable dataTable = sqlQueryDT("ExpoMRCS", query);            

            List<PremioRuleta> PremioRuleta = new List<PremioRuleta>();
            foreach (DataRow row in dataTable.Rows)
            {
                PremioRuleta premio = new PremioRuleta();
                premio.IdPremio = row.Field<Int16>(0);
                premio.Premio = row.Field<string>(1);
                premio.imgPremio = row.Field<byte[]>(2);
                PremioRuleta.Add(premio);
            }
            return PremioRuleta;
        }
        public static byte[] GetFotos(int idProducto) {
            string query = "SELECT Foto FROM mFotos where IdProducto =" + idProducto;
            DataTable dataTable = sqlQueryDT("FotosCS", query);
            byte[] fotos = new byte[dataTable.Rows.Count];

            foreach (DataRow row in dataTable.Rows)
            {
                fotos = row.Field<byte[]>(0);
            }
            return fotos;
        }
        public static DataTable sqlQueryDT(string dataBase, string query) {
            DataTable dt = new DataTable();
            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:" + dataBase]);
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter sqlDA; cnn.Open();
            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dt);
            cnn.Close();
            return dt;
        }
        public static DataTable sqlStoreProcedure(string dataBase, string StoreProcedure, SqlParameter[] parameter) {
            DataTable dt = new DataTable();
            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:" + dataBase]);
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter sqlDA; cnn.Open();
            cmd.CommandText = StoreProcedure;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dt);
            cnn.Close();
            return dt;
        }
    }
}