using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Administrador,Proveedor")]
    public class EstadisticasController : Controller
    {
        private static IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;

        public EstadisticasController(UserManager<IdentityUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _configuration = config;
        }

        public IActionResult Estadisticas()
        {
            return View();
        }

        private int getProveedor()
        {
            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            return GeneralesController.GetIdProveedor(username);
        }

        [HttpPost]
        public List<Graficas> GetDatosGraficaVisitas()
        {
            int idProveedor = getProveedor();

            List<Graficas> listaGrafica = new List<Graficas>();

            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand cmd = new SqlCommand("GraficaVisitas", cnn);
            SqlDataReader dr;
            cnn.Open();

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@proveedor", idProveedor));
            dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                Graficas graficas = new Graficas
                {
                    fecha = dr.GetDateTime(0).ToString(),
                    stand = dr.GetInt32(1),
                    producto = dr.GetInt32(2)
                };
                listaGrafica.Add(graficas);
            }
            cnn.Close();


            return listaGrafica;
        }

        [HttpPost]
        public List<Graficas> GetDatosGraficaMonedas()
        {
            int idProveedor = getProveedor();

            List<Graficas> listaGrafica = new List<Graficas>();

            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand cmd = new SqlCommand("GraficaMonedas", cnn);
            SqlDataReader dr;
            cnn.Open();

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@proveedor", idProveedor));
            dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                Graficas graficas = new Graficas
                {
                    fecha = dr.GetDateTime(0).ToShortDateString(),
                    monedas = dr.GetInt32(1)
                };
                listaGrafica.Add(graficas);
            }
            cnn.Close();


            return listaGrafica;
        }

        [HttpPost]
        public List<Graficas> GetDatosGraficaVenta()
        {
            int idProveedor = getProveedor();

            List<Graficas> listaGrafica = new List<Graficas>();

            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand cmd = new SqlCommand("GraficaVenta", cnn);
            SqlDataReader dr;
            cnn.Open();

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@proveedor", idProveedor));

            SqlParameter carrito = new SqlParameter(), confirmado = new SqlParameter(), ventaprov = new SqlParameter();
            carrito.ParameterName = "@carrito";
            confirmado.ParameterName = "@confirmado";
            ventaprov.ParameterName = "@ventaprov";

            carrito.DbType = DbType.Int32; 
            confirmado.DbType = DbType.Int32; 
            ventaprov.DbType = DbType.Double;

            carrito.Direction = ParameterDirection.Output;
            confirmado.Direction = ParameterDirection.Output;
            ventaprov.Direction = ParameterDirection.Output;

            cmd.Parameters.Add(carrito);
            cmd.Parameters.Add(confirmado);
            cmd.Parameters.Add(ventaprov);

            dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                Graficas graficas = new Graficas
                {
                    fecha = dr.GetDateTime(0).ToShortDateString(),
                    piezasCarrito = dr.GetInt32(1),
                    piezasConfirmado = dr.GetInt32(2),
                    ventaProv = dr.GetDecimal(3)
                };
                listaGrafica.Add(graficas);
            }
            cnn.Close();


            return listaGrafica;
        }

        [HttpPost]
        public JsonResult GetRegistroVisitas() 
        {
            int idProveedor = getProveedor();

            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand cmd = new SqlCommand("EstadisticaVisitaStand", cnn);
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA;
            cnn.Open();

            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;

            //// 3. add parameter to command, which will be passed to the stored procedure
            cmd.Parameters.Add(new SqlParameter("@proveedor", idProveedor));
            SqlParameter pvTotal = new SqlParameter();
            pvTotal.ParameterName = "@total";
            pvTotal.DbType = DbType.Int32;
            pvTotal.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(pvTotal);
            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dataTable);
            cnn.Close();

            var jsonData = new
            {
                rows = (
                       from c in dataTable.AsEnumerable()
                       select new
                       {
                           idPersona = c[0],
                           cliente = c[1],
                           fecha = c[2]
                       }).ToArray()
            };


            return Json(jsonData.rows);
        }
        [HttpPost]
        public JsonResult GetProductosVisitados() 
        {
            int idProveedor = getProveedor();

            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand cmd = new SqlCommand("EstadisticaVisitaProd", cnn);
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA;
            cnn.Open();

            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;

            //// 3. add parameter to command, which will be passed to the stored procedure
            cmd.Parameters.Add(new SqlParameter("@proveedor", idProveedor));
            SqlParameter pvTotal = new SqlParameter();
            pvTotal.ParameterName = "@total";
            pvTotal.DbType = DbType.Int32;
            pvTotal.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(pvTotal);
            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dataTable);
            cnn.Close();

            var jsonData = new
            {
                rows = (
                       from c in dataTable.AsEnumerable()
                       select new
                       {
                           idPersona = c[0],
                           cliente = c[1],
                           visitas = c[2]
                       }).ToArray()
            };


            return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult GetProductosInfo() 
        {
            int idProveedor = getProveedor();

            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand cmd = new SqlCommand("EstadisticaProductos", cnn);
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA;
            cnn.Open();

            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;

            //// 3. add parameter to command, which will be passed to the stored procedure
            cmd.Parameters.Add(new SqlParameter("@proveedor", idProveedor));

            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dataTable);
            cnn.Close();

            var jsonData = new
            {
                rows = (
                       from c in dataTable.AsEnumerable()
                       select new
                       {
                           idProducto = c[0],
                           producto = c[1],
                           visitas = c[2],
                           carrito = c[3],
                           confirmado = c[4],
                           VentaProv = c[5]
                       }).ToArray()
            };


            return Json(jsonData.rows);
        }
        [HttpPost]
        public JsonResult GetDetalleProducto(int IdProducto = 0) {

            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand cmd = new SqlCommand("EstadisticaProdCliente", cnn);
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA;
            cnn.Open();

            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;

            //// 3. add parameter to command, which will be passed to the stored procedure
            cmd.Parameters.Add(new SqlParameter("@idproducto", IdProducto));

            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dataTable);
            cnn.Close();

            var jsonData = new
            {
                rows = (
                       from c in dataTable.AsEnumerable()
                       select new
                       {
                           nombre = c[1],
                           visitas = c[2],
                           carrito = c[3],
                           confirmado = c[4],
                           ventaProv = c[4]
                       }).ToArray()
            };

            return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult GetMonedasAsignadas() 
        {
            int idProveedor = getProveedor();

            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand cmd = new SqlCommand("EstadisticaMonedas", cnn);
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA;
            cnn.Open();

            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;

            //// 3. add parameter to command, which will be passed to the stored procedure
            cmd.Parameters.Add(new SqlParameter("@proveedor", idProveedor));

            ////4. add output parameter
            /// 1er
            SqlParameter monedasadquiridas = new SqlParameter();
            monedasadquiridas.ParameterName = "@monedasadquiridas";
            monedasadquiridas.DbType = DbType.Int32;
            monedasadquiridas.Direction = ParameterDirection.Output;
            ////2do
            SqlParameter monedasrepartidas = new SqlParameter();
            monedasrepartidas.ParameterName = "@monedasrepartidas";
            monedasrepartidas.DbType = DbType.Int32;
            monedasrepartidas.Direction = ParameterDirection.Output;
            ////3er
            SqlParameter saldo = new SqlParameter();
            saldo.ParameterName = "@saldo";
            saldo.DbType = DbType.Int32;
            saldo.Direction = ParameterDirection.Output;
            ////4to
            SqlParameter monedasvisitastand = new SqlParameter();
            monedasvisitastand.ParameterName = "@monedasvisitastand";
            monedasvisitastand.DbType = DbType.Int32;
            monedasvisitastand.Direction = ParameterDirection.Output;
            ////5to
            SqlParameter monedasvisitaprod = new SqlParameter();
            monedasvisitaprod.ParameterName = "@monedasvisitaprod";
            monedasvisitaprod.DbType = DbType.Int32;
            monedasvisitaprod.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(monedasadquiridas);
            cmd.Parameters.Add(monedasrepartidas);
            cmd.Parameters.Add(saldo);
            cmd.Parameters.Add(monedasvisitastand);
            cmd.Parameters.Add(monedasvisitaprod);

            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dataTable);
            cnn.Close();

            var jsonData = new
            {
                rows = (
                       from c in dataTable.AsEnumerable()
                       select new
                       {
                           cliente = c[0],
                           monedas = c[1],
                           fecha = c[2],
                           motivo = c[3]
                       }).ToArray()
            };


            return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult DetailProducto(int IdProducto = 0) {

            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand cmd = new SqlCommand("EstadisticaProdDetalle", cnn);
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA;
            cnn.Open();

            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;

            //// 3. add parameter to command, which will be passed to the stored procedure
            cmd.Parameters.Add(new SqlParameter("@producto", IdProducto));

            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dataTable);
            cnn.Close();

            var jsonData = new
            {
                rows = (
                       from c in dataTable.AsEnumerable()
                       select new
                       {
                           imgProducto = HomeController.GetFotos(Convert.ToInt32(c[0])),
                           producto = c[1],
                           codigo = c[2],
                           formula = c[3],
                           visitas = c[4],
                           visitasCliente = c[5],
                           carrito = c[6],
                           carritoCliente = c[7],
                           confirmado = c[8],
                           confirmadoCliente = c[9],
                           ventaProv = c[10]
                       }).ToArray()
            };


            return Json(jsonData.rows);
        }
    }
}
