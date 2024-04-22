using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace ExpoMR.Controllers
{
    [Authorize]
    public class AyudaController : Controller
    {
        private static IConfiguration _configuration;

        public AyudaController(IConfiguration config)
        {
            _configuration = config;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Contacto() { 
            return View();
        }       
        public IActionResult Preguntas() {
            return View();
        }
        public IActionResult Privacidad() {
            return View();
        }
        public IActionResult CondicionesRuleta() {
            return View();
        }
        public IActionResult TerminosyCondiciones() {
            return View();
        }

        [HttpPost]
        public JsonResult GetSales(string letra, string palabra) {

            //SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:IndigoCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "Select mSales.IdSal, mSales.Sal, " +
            //    " CAST(Case When COUNT(mProductos.IdProducto) = 0 Then 0 Else 1 End as bit) as Habilitado " +
            //    " From mSales inner join " +
            //    " dProdSal on mSales.IdSal = dProdSal.IdSal left outer join " +
            //    " ExpoMR.dbo.mProductos on dProdSal.IdProducto = mProductos.IdProducto " +
            //    " Where UPPER(LEFT(mSales.Sal, 1)) = UPPER( CASE WHEN '"+ letra + "' = '' THEN NULL ELSE '"+letra+"' END) " +
            //    " Or mSales.Sal Like Case When '"+ palabra + "' = '' Then Null Else '%"+ palabra +"%' End" +
            //    " Group by mSales.IdSal, mSales.Sal Order by mSales.Sal";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();

            string query = "Select mSales.IdSal, mSales.Sal, " +
                " CAST(Case When COUNT(mProductos.IdProducto) = 0 Then 0 Else 1 End as bit) as Habilitado " +
                " From mSales inner join " +
                " dProdSal on mSales.IdSal = dProdSal.IdSal left outer join " +
                " ExpoMR.dbo.mProductos on dProdSal.IdProducto = mProductos.IdProducto " +
                " Where UPPER(LEFT(mSales.Sal, 1)) = UPPER( CASE WHEN '" + letra + "' = '' THEN NULL ELSE '" + letra + "' END) " +
                " Or mSales.Sal Like Case When '" + palabra + "' = '' Then Null Else '%" + palabra + "%' End" +
                " Group by mSales.IdSal, mSales.Sal Order by mSales.Sal";

            DataTable dataTable = GeneralesController.sqlQueryDT("IndigoCS", query);
            var jsonData = new
            {
                rows = (
                       from c in dataTable.AsEnumerable()
                       select new
                       {
                           IdSal = c[0],
                           Sal = c[1],
                           Habilitado = c[2]
                       }).ToArray()
            };


            return Json(jsonData.rows);
        }
        
        [HttpPost]
        public JsonResult GetLaboratorioBySal(int Sal) {

            //string file = "C:\\Users\\mvalenzuela\\Downloads\\Image20230223113835.png"; // Lee la imagen y conviértela en un byte array
            //byte[] imageBytes;
            //using (Image image = Image.FromFile(file))
            //{
            //    using (MemoryStream ms = new MemoryStream())
            //    {
            //        image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            //        imageBytes = ms.ToArray();
            //    }
            //}

            //string connectionString = _configuration["ConnectionStrings:ExpoMRCS"]; // Reemplaza con tu cadena de conexión
            //string insertQuery = "UPDATE dArticulos SET Foto =@Imagen WHERE IDdArticulos = 233;";

            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    SqlCommand command = new SqlCommand(insertQuery, connection);
            //    command.Parameters.AddWithValue("@Imagen", imageBytes);

            //    connection.Open();
            //    command.ExecuteNonQuery();
            //}

            //SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "Select mProveedores.IdPersona as Proveedor, mProveedores.Nombre," +
            //    " COUNT(mProductos.IdProducto) As NumProd, mStands.Logo" +
            //    " From mProveedores inner join" +
            //    " mLaboratorios on mProveedores.IdPersona = mLaboratorios.Proveedor inner join" +
            //    " mProductos on mLaboratorios.IdLaboratorio = mProductos.IdLaboratorio inner join" +
            //    " mStands on mStands.Proveedor = mProveedores.IdPersona inner join" +
            //    " Indigo.dbo.dProdsal on mProductos.IdProducto = dProdSal.IdProducto inner join" +
            //    " Indigo.dbo.mSales on dProdSal.IdSal = mSales.IdSal" +
            //    " Where mSales.IdSal = " + Sal + "" +
            //    " Group by mProveedores.IdPersona, mProveedores.Nombre, mStands.Logo" +
            //    " Order by mProveedores.Nombre";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();

            string query = "Select mProveedores.IdPersona as Proveedor, mProveedores.Nombre," +
                " COUNT(mProductos.IdProducto) As NumProd, mStands.Logo" +
                " From mProveedores inner join" +
                " mLaboratorios on mProveedores.IdPersona = mLaboratorios.Proveedor inner join" +
                " mProductos on mLaboratorios.IdLaboratorio = mProductos.IdLaboratorio inner join" +
                " mStands on mStands.Proveedor = mProveedores.IdPersona inner join" +
                " Indigo.dbo.dProdsal on mProductos.IdProducto = dProdSal.IdProducto inner join" +
                " Indigo.dbo.mSales on dProdSal.IdSal = mSales.IdSal" +
                " Where mSales.IdSal = " + Sal + "" +
                " Group by mProveedores.IdPersona, mProveedores.Nombre, mStands.Logo" +
                " Order by mProveedores.Nombre";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);
            var jsonData = new
            {
                rows = (
                       from c in dataTable.AsEnumerable()
                       select new
                       {
                           Proveedor = c[0],
                           Nombre = c[1],
                           NumProd = c[2],
                           img = c[3]
                       }).ToArray()
            };


            return Json(jsonData.rows);
        }
    }
}
