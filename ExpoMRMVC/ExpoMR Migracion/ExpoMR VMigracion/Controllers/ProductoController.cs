using ExpoMR.Clases;
using ExpoMR.Controllers;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using System.Data;
using System.Diagnostics;


namespace ExpoMR.Controllers
{
    [Authorize]
    public class ProductoController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _rolManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private static IConfiguration _configuration;

        //Variables para verificar La sesión Y el Usuario
        public ProductoController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender, RoleManager<IdentityRole> rolManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _rolManager = rolManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _configuration = configuration;
        }
        public async Task<List<string>> GetRoles()
        {
            var userId = _userManager.GetUserId(User); // Obtiene el ID del usuario actualmente autenticado
            var user = await _userManager.FindByIdAsync(userId); // Encuentra el usuario por su ID

            var roles = await _userManager.GetRolesAsync(user); // Obtiene los roles del usuario


            return roles.ToList();
        }

        public async Task<IActionResult> Index(int IdProducto = 0, int Idproveedor = 0)
        {

            var currentUser = await _userManager.GetUserAsync(User);
            List<string> roles = new List<string>();

            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "MiCuenta");
            }

            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            int cliente = GeneralesController.GetClientes(username);

            roles = await GetRoles();
            foreach (string role in roles)
            {
                if (role == "Cliente")
                Log.SaveVisita(cliente, Idproveedor, IdProducto);
            }

            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:IndigoCS"]);
            SqlCommand cmd = new SqlCommand("ExpoCotizarVerProducto", cnn);
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA;
            cnn.Open();
            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;
            //// 3. add parameter to command, which will be passed to the stored procedure
            cmd.Parameters.Add(new SqlParameter("@cliente", cliente));
            cmd.Parameters.Add(new SqlParameter("@idproducto", IdProducto));
            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dataTable);
            cnn.Close();

            ProductoExpoViewModel Producto = new ProductoExpoViewModel();

            foreach (DataRow row in dataTable.Rows)
            {
                Producto.IdProducto = row.Field<Int16>(0);
                Producto.Nombre = row.Field<string>(1);
                Producto.Presentacion = row.Field<string>(2);
                Producto.Unidades = row.Field<string>(3);
                Producto.Publico = row.Field<decimal>(4);
                Producto.Neto = row.Field<decimal>(5);
                Producto.Iva = row.Field<bool>(6);
                Producto.Laboratorio = row.Field<string>(7);
                Producto.Caja = row.Field<Int16>(8);
                Producto.Codigo = row.Field<string>(9);
                Producto.Restringido = row.Field<bool>(10);
                Producto.Cantidad = row.Field<int>(11);
                Producto.Formula = row.Field<string>(12);
                Producto.Controlado = row.Field<bool>(13);
                Producto.PrecioMR = row.Field<decimal>(14);
                Producto.DescuentoExpo = row.Field<decimal>(15);
                Producto.DescuentoCliente = row.Field<bool>(16);
                Producto.ValorIva = row.Field<decimal>(17);
                Producto.PrecioFinal = row.Field<decimal>(18);
                Producto.imgProductos = HomeController.GetFotos(IdProducto);
                
                
            }

            return View(Producto);
        }

        [HttpPost]
        public JsonResult AddCarrito(int idProducto, int cantidad, decimal unitario) {
            ResponseModel rm = new ResponseModel();
            try {
                var user = _userManager.GetUserAsync(User);
                string username = user.Result.UserName.ToString();
                int IdPersona = GeneralesController.GetIdPersonas(username);
                int cliente = GeneralesController.GetClientes(username);

                SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
                SqlCommand cmd = new SqlCommand("CotizarAgregar", cnn);
                cnn.Open();
                // 2. set the command object so it knows to execute a stored procedure
                cmd.CommandType = CommandType.StoredProcedure;
                //// 3. add parameter to command, which will be passed to the stored procedure
                cmd.Parameters.Add(new SqlParameter("@cliente", cliente));
                cmd.Parameters.Add(new SqlParameter("@idproducto", idProducto));
                cmd.Parameters.Add(new SqlParameter("@unitario", unitario));
                cmd.Parameters.Add(new SqlParameter("@cantidad", cantidad));
                cmd.Parameters.Add(new SqlParameter("@usuario", IdPersona));
                cmd.Connection = cnn;
                SqlDataReader dr = cmd.ExecuteReader();
                cnn.Close();

                rm.response = true;
                rm.message = "El producto se agrego al carrito";


            } catch (Exception e) {
                rm.message = e.Message;
            }
            return Json(rm);
        }
    }
}
