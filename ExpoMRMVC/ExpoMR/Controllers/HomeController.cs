using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ExpoMR.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailSender _emailSender;


        public HomeController(ILogger<HomeController> logger, IEmailSender emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
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

            SqlConnection cnn = new SqlConnection("Server=localhost\\MSSQLSERVER01;Database=ExpoMR;Integrated Security=true; MultipleActiveResultSets=true");
            SqlCommand cmd = new SqlCommand();
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA; cnn.Open();
            cmd.CommandText = "Select mProveedores.IdPersona as Proveedor, mProveedores.Nombre, mStands.Logo  From mStands inner join mProveedores on mStands.Proveedor = mProveedores.IdPersona Order by mStands.Rank asc";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dataTable);
            cnn.Close();

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

            SqlConnection cnn = new SqlConnection("Server=localhost\\MSSQLSERVER01;Database=ExpoMR;Integrated Security=true; MultipleActiveResultSets=true");
            SqlCommand cmd = new SqlCommand();
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA; cnn.Open();
            cmd.CommandText = "Select mProveedores.IdPersona as Proveedor, mProveedores.Nombre, mStands.Logo  From mStands inner join mProveedores on mStands.Proveedor = mProveedores.IdPersona where mProveedores.IdPersona ="+Id+" Order by mStands.Rank asc ";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dataTable);
            cnn.Close();

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

        public static List<Producto> GetImageProductByLab(int idProveedor)
        {

            SqlConnection cnn = new SqlConnection("Server=192.168.1.57; Database=Indigo; User ID=usrapp;Password=tangeri;Trusted_Connection=false;MultipleActiveResultSets=true");
            SqlCommand cmd = new SqlCommand("ExpoStandProductos", cnn);
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA; 
            cnn.Open();            

            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;

            //// 3. add parameter to command, which will be passed to the stored procedure
            cmd.Parameters.Add(new SqlParameter("@proveedor", idProveedor));
            cmd.Parameters.Add(new SqlParameter("@cliente", 3456));


            //cmd.CommandText = "Select mProveedores.IdPersona as Proveedor, mProveedores.Nombre, mStands.Logo  From mStands inner join mProveedores on mStands.Proveedor = mProveedores.IdPersona where mProveedores.IdPersona =" + Id + " Order by mStands.Rank asc ";
            //cmd.CommandType = CommandType.Text;
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
                producto.imgProducto = GetFotos(producto.IdProducto);
                ProductStands.Add(producto);
            }


            return ProductStands;
        }

        public static byte[] GetFotos(int idProducto) {
            SqlConnection cnn = new SqlConnection("Server=192.168.1.57; Database=Fotos; User ID=usrapp;Password=tangeri;Trusted_Connection=false;MultipleActiveResultSets=true");
            SqlCommand cmd = new SqlCommand();
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA; cnn.Open();
            cmd.CommandText = "SELECT Foto FROM mFotos where IdProducto ="+idProducto;
            cmd.CommandType = CommandType.Text;
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
    }
}