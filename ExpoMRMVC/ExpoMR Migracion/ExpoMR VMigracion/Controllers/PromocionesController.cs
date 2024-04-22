using ExpoMR.Clases;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ExpoMR.Controllers
{
    [Authorize]
    public class PromocionesController : Controller
    {
        private static IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;

        public PromocionesController(UserManager<IdentityUser> userManager, IConfiguration config)
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

        private int getUsuario()
        {
            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            return GeneralesController.GetIdPersonas(username);
        }
        public IActionResult Promociones()
        {
            return View();
        }
        public IActionResult VerPromociones()
        {
            return View();
        }
        public IActionResult EditarPromociones(int idPromocion = 0)
        {
            return View(idPromocion);
        }

        [HttpPost]
        public JsonResult GetClientes()
        {
            int idProveedor = getProveedor();

            string query = "SELECT lVisita.Cliente AS idCliente, mClientes.Nombre AS cliente"
                           + " FROM lVisita INNER JOIN"
                           + " mClientes ON lVisita.Cliente = mClientes.IdPersona"
                           + " WHERE lVisita.Proveedor = " + idProveedor
                           + " GROUP BY lVisita.Cliente, mClientes.Nombre";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);


            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           idCliente = col[0],
                           cliente = col[1]
                       }).ToArray()
            };


            return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult GetProdcutos()
        {
            int idProveedor = getProveedor();

            string query = "Select vProductos.idProducto, vProductos.codigo,"
                           + " vProductos.Nombre + ' ' + vProductos.Presentacion + ' ' + vProductos.Unidades as producto"
                           + " From mProveedores inner join"
                           + " mLaboratorios ON mProveedores.IdPersona = mLaboratorios.Proveedor INNER JOIN"
                           + " Indigo.dbo.vProductos on mLaboratorios.IdLaboratorio = vProductos.IdLaboratorio INNER JOIN"
                           + " mProductos ON vProductos.IdProducto = mProductos.IdProducto"
                           + " Where mProveedores.IdPersona = " + idProveedor
                           + " And vProductos.Status in (0, 3, 4) -- status 0 = activo, 3 = nuevo, 4 = liquidacion";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);


            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           idProducto = col[0],
                           codigo = col[1],
                           producto = col[2]
                       }).ToArray()
            };


            return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult GetArticulosProveedor()
        {
            int idProveedor = getProveedor();

            string query = "SELECT mArticulos.IdArticulo, dbo.RemoveTildes(mArticulos.Nombre) AS Articulo,"
                           + " (SELECT TOP 1 dArticulos.Foto FROM dArticulos WHERE dArticulos.IdArticulo = mArticulos.IdArticulo ORDER BY dArticulos.NumFoto ASC) AS Foto"
                           + " FROM mArticulos"
                           + " WHERE mArticulos.Proveedor = " + idProveedor
                           + " GROUP BY mArticulos.IdArticulo, mArticulos.Nombre";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           idArticulo = col[0],
                           articulo = col[1],
                           foto = col[2]
                       }).ToArray()
            };


            return Json(jsonData.rows);
        }

        [HttpPost]
        public int AgregarPromocion(string promo, string tipo, int cantidad, int idCliente, int idProducto)
        {
            int idProveedor = getProveedor();
            int idUsuario = getUsuario();
            SqlConnection conexion = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand command = new SqlCommand("PromocionesInsertar", conexion);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Proveedor", idProveedor);
            command.Parameters.AddWithValue("@Usuario", idUsuario);
            command.Parameters.AddWithValue("@IdTipoPromocion", promo);
            command.Parameters.AddWithValue("@Cliente", idCliente);
            command.Parameters.AddWithValue("@Idproducto", idProducto);
            if (tipo == "Monto")
            {
                command.Parameters.AddWithValue("@Monto", cantidad);
                command.Parameters.AddWithValue("@Piezas", "");
            }
            else if (tipo == "Piezas")
            {
                command.Parameters.AddWithValue("@Monto", "");
                command.Parameters.AddWithValue("@Piezas", cantidad);
            }
            else
            {
                command.Parameters.AddWithValue("@Monto", "");
                command.Parameters.AddWithValue("@Piezas", "");
            }
            SqlParameter returnParameter = command.Parameters.Add("@IdPromocion", SqlDbType.Int);
            returnParameter.Direction = ParameterDirection.Output;

            conexion.Open();


            command.ExecuteNonQuery();
            conexion.Close();

            return (int)returnParameter.Value;
        }

        [HttpPost]
        public void AgregarArticulo(int idPromocion, int idArticulo, int cantidad)
        {
            SqlConnection conexion = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand command = new SqlCommand("PromocionesDetalleInsertar", conexion);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IdPromocion", idPromocion);
            command.Parameters.AddWithValue("@IdArticulo", idArticulo);
            command.Parameters.AddWithValue("@Cantidad", cantidad);

            conexion.Open();


            command.ExecuteNonQuery();
            conexion.Close();
        }

        [HttpPost]
        public JsonResult GetArticulosPromo(int idPromocion)
        {
            string query = "SELECT dPromociones.IdArticulo, mArticulos.Nombre AS Articulo,"
                           + " (SELECT TOP 1 dArticulos.Foto FROM dArticulos WHERE dArticulos.IdArticulo = mArticulos.IdArticulo ORDER BY dArticulos.NumFoto ASC) AS Foto, dPromociones.Cantidad"
                           + " FROM dPromociones INNER JOIN"
                           + " mArticulos ON dPromociones.IdArticulo = mArticulos.IdArticulo"
                           + " WHERE dPromociones.IdPromocion = " + idPromocion;

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           idArticulo = col[0],
                           articulo = col[1],
                           foto = col[2],
                           cantidad = col[3]
                       }).ToArray()
            };

            return Json(jsonData.rows);
        }

        [HttpPost]
        public int ActualizaPromocion(string promo, int idPromocion, string tipo, int cantidad, int idCliente, int idProducto)
        {
            int idProveedor = getProveedor();
            int idUsuario = getUsuario();
            SqlConnection conexion = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand command = new SqlCommand("PromocionesUpdate", conexion);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Proveedor", idProveedor);
            command.Parameters.AddWithValue("@Usuario", idUsuario);
            command.Parameters.AddWithValue("@IdPromocion", idPromocion);
            command.Parameters.AddWithValue("@IdTipoPromocion", promo);
            command.Parameters.AddWithValue("@Cliente", idCliente);
            command.Parameters.AddWithValue("@Idproducto", idProducto);
            if (tipo == "Monto")
            {
                command.Parameters.AddWithValue("@Monto", cantidad);
                command.Parameters.AddWithValue("@Piezas", "");
            }
            else if (tipo == "Piezas")
            {
                command.Parameters.AddWithValue("@Monto", "");
                command.Parameters.AddWithValue("@Piezas", cantidad);
            }
            else
            {
                command.Parameters.AddWithValue("@Monto", "");
                command.Parameters.AddWithValue("@Piezas", "");
            }
            SqlParameter returnParameter = command.Parameters.Add("@Updated", SqlDbType.Int);
            returnParameter.Direction = ParameterDirection.Output;

            conexion.Open();
            command.ExecuteNonQuery();
            conexion.Close();

            return (int)returnParameter.Value;
         }

        [HttpPost]
        public void ActivarPromocion(int idPromocion)
        {
            SqlConnection conexion = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand command = new SqlCommand("ActivarPromocion", conexion);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IdPromocion", idPromocion);

            conexion.Open();
            command.ExecuteNonQuery();
            conexion.Close();
        }

        [HttpPost]
        public int EliminaPromocion(int idPromocion)
        {
            SqlConnection conexion = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand command = new SqlCommand("PromocionesEliminar", conexion);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IdPromocion", idPromocion);

            SqlParameter returnParameter = command.Parameters.Add("@eliminado", SqlDbType.Int);
            returnParameter.Direction = ParameterDirection.Output;

            conexion.Open();
            command.ExecuteNonQuery();
            conexion.Close();

            return (int)returnParameter.Value;
        }

        [HttpPost]
        public void EliminaArticulo(int idPromocion, int idArticulo)
        {
            SqlConnection conexion = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand command = new SqlCommand("PromocionesDetalleEliminar", conexion);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IdPromocion", idPromocion);
            command.Parameters.AddWithValue("@IdArticulo", idArticulo);

            conexion.Open();
            command.ExecuteNonQuery();
            conexion.Close();
        }

        [HttpPost]
        public List<Promociones> GetPromo(int tipoPromo)
        {
            int idProveedor = getProveedor();
            string query = "SELECT mPromociones.IdPromocion, CONVERT(float, dPromoCondicion.Monto) as Monto, dPromoCondicion.Piezas, vProductos.Nombre AS Producto,"
                           + "  vPersonas.Nombre AS Cliente, mPromociones.Activa"
                           + " FROM mPromociones INNER JOIN"
                           + " dPromoCondicion ON mPromociones.IdPromocion = dPromoCondicion.IdPromocion LEFT OUTER JOIN"
                           + " vProductos ON dPromoCondicion.IdProducto = vProductos.IdProducto LEFT OUTER JOIN"
                           + " vPersonas ON mPromociones.Cliente = vPersonas.IdPersona"
                           + " WHERE mPromociones.IdTipoPromocion = " + tipoPromo
                           + " AND mPromociones.Proveedor = " + idProveedor
                           + " ORDER BY dPromoCondicion.Monto ASC, dPromoCondicion.Piezas, mPromociones.Cliente";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            List<Promociones> promociones = new List<Promociones>();
            foreach (DataRow row in dataTable.Rows)
            {
                Promociones promocion = new Promociones();
                promocion.idPromocion = row.Field<Int16>(0);
                promocion.monto = row.Field<double?>(1);
                promocion.piezas = row.Field<Int16?>(2);
                promocion.producto = row.Field<string>(3);
                promocion.cliente = row.Field<string>(4);
                promocion.activa = row.Field<bool>(5);
                promocion.tipoPromo = tipoPromo;
                promocion.articulos = getArticulos(promocion.idPromocion);
                promociones.Add(promocion);
            }

            return promociones;
        }

        private List<Articulos> getArticulos(int idPromocion)
        {
            string query = "SELECT dPromociones.Cantidad, mArticulos.Nombre"
                           + " FROM dPromociones LEFT OUTER JOIN"
                           + " mArticulos ON dPromociones.IdArticulo = mArticulos.IdArticulo"
                           + " WHERE dPromociones.IdPromocion = " + idPromocion;

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            List<Articulos> articulos = new List<Articulos>();
            foreach (DataRow row in dataTable.Rows)
            {
                Articulos articulo = new Articulos();
                articulo.cantidad = row.Field<Int16>(0);
                articulo.nombre = row.Field<string>(1);
                articulos.Add(articulo);
            }

            return articulos;
        }

        [HttpPost]
        public JsonResult GetArticulosRestantes()
        {
            int idProveedor = getProveedor();
            string query = "SELECT vArticulos.IdArticulo, vArticulos.Nombre AS Articulo, vArticulos.Descripcion,"
                           + " vArticulos.CantidadDisponible AS CantidadRestante"
                           + " FROM vArticulos"
                           + " WHERE vArticulos.Proveedor = " + idProveedor;

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           idArticulo = col[0],
                           articulo = col[1],
                           descripcion = col[2],
                           cantidadRestante = col[3]
                       }).ToArray()
            };

            return Json(jsonData.rows);
        }

        [HttpPost]
        public Promociones GetPromoById(int idPromocion)
        {
            string query = "SELECT mPromociones.IdPromocion, CONVERT(int, mPromociones.IdTipoPromocion) AS IdTipoPromocion, mPromociones.Cliente AS IdCliente,"
                           + " vPersonas.Nombre AS Cliente, mPromociones.Activa,"
                           + " dPromoCondicion.Monto, dPromoCondicion.Piezas, dPromoCondicion.IdProducto, vProductos.Producto"
                           + " FROM mPromociones INNER JOIN"
                           + " dPromoCondicion ON mPromociones.IdPromocion = dPromoCondicion.IdPromocion LEFT OUTER JOIN"
                           + " vProductos ON dPromoCondicion.IdProducto = vProductos.IdProducto LEFT OUTER JOIN"
                           + " vPersonas ON mPromociones.Cliente = vPersonas.IdPersona"
                           + " WHERE mPromociones.IdPromocion = " + idPromocion;

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            Promociones promocion = new Promociones();
            foreach (DataRow row in dataTable.Rows)
            {
                promocion.idPromocion = row.Field<Int16>(0);
                promocion.tipoPromo = row.Field<int>(1);
                promocion.idCliente = row.Field<int?>(2);
                promocion.cliente = row.Field<string>(3);
                promocion.activa = row.Field<bool>(4);
                promocion.monto = row.Field<float?>(5);
                promocion.piezas = row.Field<Int16?>(6);
                promocion.idProducto = row.Field<Int16?>(7);
                promocion.producto = row.Field<string>(8);
               
                promocion.articulos = getArticulos(promocion.idPromocion);
            }

            return promocion;
        }
    }
}
