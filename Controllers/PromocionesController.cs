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

        public IActionResult EditarPromociones(int idPromocion = 0)
        {
            return View(idPromocion);
        }

        public IActionResult Promociones()
        {
            return View();
        }

        public IActionResult VerPromociones()
        {
            return View();
        }

        private List<Articulos> getArticulos(int idPromocion)
        {
            List<Articulos> articulos = new List<Articulos>();

            string query = "SELECT Promocion.IdArticulo, Promocion.Articulo, Promocion.Foto, Promocion.Cantidad FROM GetArticulosPromocionProv (@IdPromocion) AS Promocion";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdPromocion", idPromocion);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                Articulos articulo = new Articulos();

                articulo.nombre = (string)dr["Articulo"];
                articulo.cantidad = (Int16)dr["Cantidad"];

                articulos.Add(articulo);
            }

            return articulos;
        }

        [HttpPost]
        public List<Promociones> GetPromo(int tipoPromo)
        {
            List<Promociones> promociones = new List<Promociones>();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@IdProveedor", HttpContext.Session.GetString("ClienteSel"));
            parameters[1] = new SqlParameter("@IdTipoPromocion", tipoPromo);

            SqlDataReader dr = dB.ExecuteReader("GetPromocionesProv", ref parameters);

            while (dr.Read())
            {
                Promociones promocion = new Promociones();

                promocion.idPromocion = (Int16)dr["IdPromocion"];
                promocion.monto = (double)dr["Monto"];
                promocion.piezas = (Int16)dr["Piezas"];
                promocion.idProducto = (Int16)dr["IdProducto"];
                promocion.producto = (string)dr["Producto"];
                promocion.idCliente = (int)dr["IdCliente"];
                promocion.cliente = (string)dr["Cliente"];
                promocion.activa = (bool)dr["Activa"];
                promocion.tipoPromo = (byte)dr["TipoPromo"];
                promocion.articulos = getArticulos(promocion.idPromocion);

                promociones.Add(promocion);
            }

            return promociones;
        }

        [HttpPost]
        public Promociones GetPromoById(int idPromocion)
        {
            Promociones promocion = new Promociones();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdPromocion", idPromocion);

            SqlDataReader dr = dB.ExecuteReader("GetPromocionProv", ref parameters);

            while (dr.Read())
            {
                promocion.idPromocion = (Int16)dr["IdPromocion"];
                promocion.monto = (double)dr["Monto"];
                promocion.piezas = (Int16)dr["Piezas"];
                promocion.idProducto = (Int16)dr["IdProducto"];
                promocion.producto = (string)dr["Producto"];
                promocion.idCliente = (int)dr["IdCliente"];
                promocion.cliente = (string)dr["Cliente"];
                promocion.activa = (bool)dr["Activa"];
                promocion.tipoPromo = (byte)dr["TipoPromo"];
                promocion.articulos = getArticulos(promocion.idPromocion);
            }

            return promocion;
        }

        [HttpPost]
        public JsonResult GetArticulosPromo(int idPromocion)
        {
            string query = "SELECT Promocion.IdArticulo, Promocion.Articulo, Promocion.Foto, Promocion.Cantidad FROM GetArticulosPromocionProv (@IdPromocion) AS Promocion";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdPromocion", idPromocion);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            var jsonData = new
            {
                rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           idArticulo = row["IdArticulo"],
                           articulo = row["Articulo"],
                           foto = row["Foto"],
                           cantidad = row["Cantidad"]
                       }).ToArray()
            };

            return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult GetArticulosProveedor()
        {
            string query = "SELECT Articulos.IdArticulo, Articulos.Nombre, Articulos.Descripcion, Articulos.IDdArticulos, Articulos.Foto, Articulos.NumFoto FROM GetArticulosProv (@IdProveedor) AS Articulos";

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
                           idArticulo = row["IdArticulo"],
                           articulo = row["Nombre"],
                           foto = row["Foto"]
                       }).ToArray()
            };

            return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult GetArticulosRestantes()
        {
            string query = "SELECT vArticulos.IdArticulo, vArticulos.Nombre AS Articulo, vArticulos.Descripcion, vArticulos.CantidadDisponible AS CantidadRestante " +
                            "FROM vArticulos " +
                            "WHERE vArticulos.Proveedor = @IdProveedor " +
                            "ORDER BY vArticulos.Nombre";

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
                           idArticulo = row["IdArticulo"],
                           articulo = row["Articulo"],
                           descripcion = row["Descripcion"],
                           cantidadRestante = row["CantidadRestante"]
                       }).ToArray()
            };

            return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult GetClientes()
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProveedor", HttpContext.Session.GetString("ClienteSel"));

            SqlDataReader dr = dB.ExecuteReader("GetVisitasStandProv", ref parameters);

            var jsonData = new
            {
                rows = (
                        from row in dr.Cast<System.Data.Common.DbDataRecord>()
                        select new
                        {
                            cliente = row["IdPersona"],
                            nombre = row["Nombre"]
                        }).ToArray()
            };

            return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult GetProdcutos()
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProveedor", HttpContext.Session.GetString("ClienteSel"));

            SqlDataReader dr = dB.ExecuteReader("GetProductosPromocionesProv", ref parameters);

            var jsonData = new
            {
                rows = (
                        from row in dr.Cast<System.Data.Common.DbDataRecord>()
                        select new
                        {
                            idProducto = row["IdProducto"],
                            codigo = row["Codigo"],
                            producto = row["Producto"]
                        }).ToArray()
            };

            return Json(jsonData.rows);
        }

        [HttpPost]
        public void ActivarPromocion(int idPromocion)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdPromocion", idPromocion);

            dB.ExecuteNonQuery("ActivarPromocion", ref parameters);
        }

        [HttpPost]
        public bool ActualizaPromocion(string promo, int idPromocion, string tipo, int cantidad, int idCliente, int idProducto)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[9];
            parameters[0] = new SqlParameter("@Proveedor", HttpContext.Session.GetString("ClienteSel"));
            parameters[1] = new SqlParameter("@Usuario", HttpContext.Session.GetString("ClienteSel"));
            parameters[2] = new SqlParameter("@IdPromocion", idPromocion);
            parameters[3] = new SqlParameter("@IdTipoPromocion", promo);
            parameters[4] = new SqlParameter("@Cliente", idCliente);
            parameters[5] = new SqlParameter("@Idproducto", idProducto);

            if (tipo == "Monto")
            {
                parameters[6] = new SqlParameter("@Monto", cantidad);
                parameters[7] = new SqlParameter("@Piezas", "");
            }
            else if (tipo == "Piezas")
            {
                parameters[6] = new SqlParameter("@Monto", "");
                parameters[7] = new SqlParameter("@Piezas", cantidad);
            }
            else
            {
                parameters[6] = new SqlParameter("@Monto", "");
                parameters[7] = new SqlParameter("@Piezas", "");
            }

            parameters[8] = new()
            {
                ParameterName = "@Updated",
                DbType = DbType.Boolean,
                Direction = ParameterDirection.Output
            };

            SqlDataReader dr = dB.ExecuteReader("PromocionesUpdate", ref parameters);

            return (bool)parameters[8].Value;
        }

        [HttpPost]
        public void AgregarArticulo(int idPromocion, int idArticulo, int cantidad)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@IdPromocion", idPromocion);
            parameters[1] = new SqlParameter("@IdArticulo", idArticulo);
            parameters[2] = new SqlParameter("@Cantidad", cantidad);

            dB.ExecuteNonQuery("PromocionesDetalleInsertar", ref parameters);
        }

        [HttpPost]
        public Int16 AgregarPromocion(string promo, string tipo, int cantidad, int idCliente, int idProducto)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[8];
            parameters[0] = new SqlParameter("@Proveedor", HttpContext.Session.GetString("ClienteSel"));
            parameters[1] = new SqlParameter("@Usuario", HttpContext.Session.GetString("ClienteSel"));
            parameters[2] = new SqlParameter("@IdTipoPromocion", promo);
            parameters[3] = new SqlParameter("@Cliente", idCliente);
            parameters[4] = new SqlParameter("@Idproducto", idProducto);

            if (tipo == "Monto")
            {
                parameters[5] = new SqlParameter("@Monto", cantidad);
                parameters[6] = new SqlParameter("@Piezas", "");
            }
            else if (tipo == "Piezas")
            {
                parameters[5] = new SqlParameter("@Monto", "");
                parameters[6] = new SqlParameter("@Piezas", cantidad);
            }
            else
            {
                parameters[5] = new SqlParameter("@Monto", "");
                parameters[6] = new SqlParameter("@Piezas", "");
            }

            parameters[7] = new()
            {
                ParameterName = "@IdPromocion",
                DbType = DbType.Int16,
                Direction = ParameterDirection.Output
            };

            SqlDataReader dr = dB.ExecuteReader("PromocionesInsertar", ref parameters);

            return (Int16)parameters[7].Value;
        }

        [HttpPost]
        public void EliminaArticulo(int idPromocion, int idArticulo)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@IdPromocion", idPromocion);
            parameters[1] = new SqlParameter("@IdArticulo", idArticulo);

            dB.ExecuteNonQuery("PromocionesDetalleEliminar", ref parameters);
        }

        [HttpPost]
        public bool EliminaPromocion(int idPromocion)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@IdPromocion", idPromocion);
            parameters[1] = new()
            {
                ParameterName = "@eliminado",
                DbType = DbType.Boolean,
                Direction = ParameterDirection.Output
            };

            dB.ExecuteNonQuery("PromocionesEliminar", ref parameters);

            return (bool)parameters[1].Value;
        }
    }
}