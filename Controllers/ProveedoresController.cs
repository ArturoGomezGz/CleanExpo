using ExpoMR.Clases;
using ExpoMR.Datos;
using ExpoMR.Models;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Org.BouncyCastle.Asn1.X500;
using System.Runtime.CompilerServices;
using Org.BouncyCastle.Ocsp;

namespace ExpoMR.Controllers
{
    //[Authorize(Roles = "Proveedor")]
    public class ProveedoresController : Controller
    {
        //Variables para obtener (la sesión, el Usuario, path de carpeta wwwroot)
        private static IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ProveedoresController(UserManager<IdentityUser> userManager, IConfiguration config, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = config;
            _userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult AddArticulo(string Nombre, string Descripcion, int CantidadArticulo, IFormFile Foto, int Orden)
        {
            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[6];
                parameters[0] = new SqlParameter("@Nombre", Nombre);
                parameters[1] = new SqlParameter("@Descripcion", Descripcion != null? Descripcion : "");
                parameters[2] = new SqlParameter("@Cantidad", CantidadArticulo);
                parameters[3] = new SqlParameter("@IdProveedor", HttpContext.Session.GetString("ClienteSel"));
                parameters[4] = new SqlParameter("@Orden", Orden);
                parameters[5] = new SqlParameter("@Foto", WebSite.ConvertToByteArray(Foto));

                dB.ExecuteNonQuery("AddArticuloProv", ref parameters);
            }
            catch (Exception)
            {
                throw;
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult ArticuloGuardarImagen(IFormFile file, int IdArticulo, int orden)
        {
            try
            {
                // Verifica si se recibió un archivo
                if (file != null && file.Length > 0)
                {
                    DB dB = new();
                    SqlParameter[] parameters = new SqlParameter[3];
                    parameters[0] = new SqlParameter("@IdArticulo", IdArticulo);
                    parameters[1] = new SqlParameter("@Orden", orden);
                    parameters[2] = new SqlParameter("@Foto", WebSite.ConvertToByteArray(file));

                    dB.ExecuteNonQuery("AddFotoArticuloProv", ref parameters);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return Json(new { success = true });
        }

        public IActionResult Articulos()
        {
            return View();
        }

        public IActionResult AsignaMonedas(int idCliente)
        {
            return View(idCliente);
        }

        public IActionResult Contacto()
        {
            return View();
        }

        public IActionResult DetalleArticulo(int IdArticulo)
        {
            return View(IdArticulo);
        }

        public IActionResult MiStand()
        {
            var parametros = new RouteValueDictionary
            {
                {"idProveedor", HttpContext.Session.GetString("ClienteSel")}
            };
            
            IActionResult stand = RedirectToAction("Editar", "Stand", parametros);

            return stand;
        }

        public IActionResult Precios(int idCliente = 0)
        {
            return View(idCliente);
        }

        public IActionResult DetallePrecios(int idCliente = 0)
        {
            return View(idCliente);
        }

        public IActionResult Preguntas()
        {
            return View();
        }

        public IActionResult Privacidad()
        {
            return View();
        }

        public ActionResult Anuncios()
        {
            return View();
        }

        public ActionResult Monedas()
        {
            return View();
        }

        [HttpPost]
        public JsonResult articuloEditar(int IdArticulo, string Nombre, string Descripcion, int Cantidad)
        {
            ResponseModel rm = new ResponseModel();

            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[4];
                parameters[0] = new SqlParameter("@IdArticulo", IdArticulo);
                parameters[1] = new SqlParameter("@Nombre", Nombre);
                parameters[2] = new SqlParameter("@Descripcion", Descripcion != null ? Descripcion : "");
                parameters[3] = new SqlParameter("@Cantidad", Cantidad);

                dB.ExecuteNonQuery("EditArticuloProv", ref parameters);

                rm.Response = true;
                rm.Message = "El articulo se modifico con exito";
            }
            catch (Exception e)
            {
                rm.Message = e.Message;
            }

            return Json(rm);
        }

        [HttpPost]
        public JsonResult ArticuloEliminaFoto(int IDdArticulos)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IDdArticulos", IDdArticulos);

            dB.ExecuteNonQuery("DeleteFotoArticuloProv", ref parameters);

            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult articuloEliminar(int IdArticulo)
        {
            ResponseModel rm = new ResponseModel();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@idarticulo", IdArticulo);

            int ret = dB.ExecuteReturnValue("ArticulosEliminar", ref parameters);

            return Json(ret);
        }

        [HttpPost]
        public JsonResult ArticuloSeleccionado(int IdArticulo)
        {
            ArticulosProveedores DetalleArticulo = new ArticulosProveedores();

            string query = "SELECT Articulo.IdArticulo, Articulo.Nombre, Articulo.Descripcion, Articulo.IDdArticulos, " +
                "Articulo.Foto, Articulo.Proveedor, Articulo.Cantidad, Articulo.NumFoto " +
                "FROM GetArticuloProv(@IdArticulo) AS Articulo";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdArticulo", IdArticulo);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                DetalleArticulo.IdArticulo = (Int16)dr["IdArticulo"];
                DetalleArticulo.Nombre = (string)dr["Nombre"];
                DetalleArticulo.Descripcion = (string)dr["Descripcion"];
                DetalleArticulo.IDdArticulos = (dr["IDdArticulos"] != DBNull.Value ? (int)dr["IDdArticulos"] : 0);
                DetalleArticulo.Foto = (dr["Foto"] != DBNull.Value ? (byte[])dr["Foto"] : Array.Empty<byte>());
                DetalleArticulo.Proveedor = (int)dr["Proveedor"];
                DetalleArticulo.Cantidad = (int)dr["Cantidad"];
            }

			dB.Close();

			return Json(DetalleArticulo);
        }

        [HttpPost]
        public JsonResult AsignarMonedas(int idCliente, int tipoAsignacion, int monedas)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[5];
            parameters[0] = new SqlParameter("@origen", HttpContext.Session.GetString("ClienteSel"));
            parameters[1] = new SqlParameter("@destino", idCliente);
            parameters[2] = new SqlParameter("@idmotivo", tipoAsignacion == 1? "12" : "6" );
            parameters[3] = new SqlParameter("@monedas", monedas);
            parameters[4] = new SqlParameter("@usuario", HttpContext.Session.GetString("ClienteSel"));

            int ret = dB.ExecuteReturnValue("MonedasAsignar", ref parameters);

            return Json(ret);
        }

        [HttpPost]
        public JsonResult ddlLisCliente()
        {
            string query = "SELECT Vitisas.IdPersona, Vitisas.Nombre " +
                "FROM GetVisitasStandProv(@IdProveedor, @Cliente) AS Vitisas";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@IdProveedor", HttpContext.Session.GetString("ClienteSel"));
            parameters[1] = new SqlParameter("@Cliente", "0");

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

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

			dB.Close();

			return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult eliminarAnuncio(int idAnuncio)
        {
            ResponseModel rm = new();

            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@IdAnuncio", idAnuncio);

                dB.ExecuteNonQuery("DeleteAnuncioProv", ref parameters);
                rm.Response = true;
            }
            catch (Exception ex)
            {
                rm.Response = false;
                rm.Message = ex.Message;
            }

            return Json(rm);
        }

        [HttpPost]
        public JsonResult GetAnuncio(int idAnuncio)
        {
            mAnuncios anuncio = new mAnuncios();

            string query = "SELECT mAnuncios.IdAnuncio, mAnuncios.Proveedor, mAnuncios.Anuncio, " +
                "mAnuncios.FeInicio, mAnuncios.FeTermino, mAnuncios.Rank " +
                "FROM mAnuncios " +
                "WHERE mAnuncios.IdAnuncio = @IdAnuncio";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdAnuncio", idAnuncio);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                anuncio.IdAnuncio = (int)dr["IdAnuncio"];
                anuncio.Proveedor = (int)dr["Proveedor"];
                anuncio.Anuncio = (string)dr["Anuncio"];
                anuncio.FeInicio = (DateTime)dr["FeInicio"];
                anuncio.FeTermino = (DateTime)dr["FeTermino"];
                anuncio.Rank = (byte)dr["Rank"];
            }

			dB.Close();

			return Json(anuncio);           
        }

        [HttpPost]
        public JsonResult GetAnuncios()
        {
            List<mAnuncios> Anuncios = new();

            string query = "SELECT mAnuncios.IdAnuncio, mAnuncios.Proveedor, mAnuncios.Anuncio, " +
                "mAnuncios.FeInicio, mAnuncios.FeTermino, mAnuncios.Rank " +
                "FROM mAnuncios " +
                "WHERE mAnuncios.Proveedor = @IdProveedor " +
                "ORDER BY IdAnuncio DESC";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProveedor", HttpContext.Session.GetString("ClienteSel"));

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                mAnuncios anuncio = new mAnuncios();

                anuncio.IdAnuncio = (int)dr["IdAnuncio"];
                anuncio.Proveedor = (int)dr["Proveedor"];
                anuncio.Anuncio = (string)dr["Anuncio"];
                anuncio.FeInicio = (DateTime)dr["FeInicio"];
                anuncio.FeTermino = (DateTime)dr["FeTermino"];
                anuncio.Rank = (byte)dr["Rank"];

                Anuncios.Add(anuncio);
            }

			dB.Close();

			return Json(Anuncios);
        }

        [HttpPost]
        public JsonResult GetArticulos()
        {
            List<ArticulosProveedores> Articulos = new();

            string query = "SELECT Articulos.IdArticulo, Articulos.Nombre, Articulos.Descripcion, Articulos.IDdArticulos, " +
                "Articulos.Foto, Articulos.NumFoto " +
                "FROM GetArticulosProv (@IdProveedor) AS Articulos";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProveedor", HttpContext.Session.GetString("ClienteSel"));

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                ArticulosProveedores DetalleArticulo = new ArticulosProveedores();

                DetalleArticulo.IdArticulo = (Int16)dr["IdArticulo"];
                DetalleArticulo.Nombre = (string)dr["Nombre"];
                DetalleArticulo.Descripcion = (string)dr["Descripcion"];
                DetalleArticulo.IDdArticulos = (dr["IDdArticulos"] != DBNull.Value ? (int)dr["IDdArticulos"] : 0);
                DetalleArticulo.Foto = (dr["Foto"] != DBNull.Value ? (byte[])dr["Foto"] : Array.Empty<byte>());
                DetalleArticulo.NumFoto = (dr["NumFoto"] != DBNull.Value ? (byte)dr["NumFoto"] : (byte)0);

                Articulos.Add(DetalleArticulo);
            }

			dB.Close();

			return Json(Articulos);
        }

        [HttpPost]
        public JsonResult getClienteAsignar(int idCliente)
        {
            Clientes Visita = new Clientes();

            string query = "SELECT Vitisas.IdPersona, Vitisas.Nombre " +
                "FROM GetVisitasStandProv(@IdProveedor, @Cliente) AS Vitisas";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@IdProveedor", HttpContext.Session.GetString("ClienteSel"));
            parameters[1] = new SqlParameter("@Cliente", idCliente);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                Visita.IdPersona = (int)dr["IdPersona"];
                Visita.Nombre = (string)dr["Nombre"];
            }

			dB.Close();

			return Json(Visita);
        }

        [HttpPost]
        public JsonResult GetClientes()
        {
            List<Clientes> Visitas = new();

            string query = "SELECT Visitas.IdPersona, Visitas.Nombre, " +
                "(SELECT ISNULL(SUM(monedas),0) " +
                "FROM mTransacciones " +
                "WHERE Usuario = @IdProveedor " +
                "AND IdMotivo IN (12,6) " +
                "AND Destino = Visitas.IdPersona) AS Monedas " +
                "FROM GetVisitasStandProv(@IdProveedor, @Cliente) AS Visitas";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@IdProveedor", HttpContext.Session.GetString("ClienteSel"));
            parameters[1] = new SqlParameter("@Cliente", "0");

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                Clientes Visita = new Clientes();

                Visita.IdPersona = (int)dr["IdPersona"];
                Visita.Nombre = (string)dr["Nombre"];
                Visita.Monedas = (int)dr["Monedas"];

                Visitas.Add(Visita);
            }

			dB.Close();

			return Json(Visitas);
        }

        [HttpPost]
        public JsonResult getMonedasContacto(int idCliente)
        {
            MonedasActuales MonedasActuales = new MonedasActuales();

            string query = "SELECT SUM(MonedasGanadas) AS MonedasContacto, SUM(Monedas) AS Monedas " +
                            "FROM ( " +
                            "		SELECT mTransacciones.Monedas AS MonedasGanadas, 0 AS Monedas " +
                            "		FROM mTransacciones " +
                            "		WHERE mTransacciones.Origen =  @IdProveedor " +
                            "		AND mTransacciones.Destino =  @Cliente " +
                            "		AND mTransacciones.IdMotivo = 6 " +
                            "		UNION " +
                            "		SELECT 0 AS MonedasGanadas, dbo.GetMonedas(@IdProveedor, @Cliente, 6, GETDATE(), NULL) AS Monedas " +
                            "	 ) AS T1";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@IdProveedor", HttpContext.Session.GetString("ClienteSel"));
            parameters[1] = new SqlParameter("@Cliente", idCliente);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                MonedasActuales.monedas = (int)dr["Monedas"];
                MonedasActuales.monedasContacto = (int)dr["MonedasContacto"];
            }

			dB.Close();

			return Json(MonedasActuales);
        }

        [HttpPost]
        public JsonResult getPreciosCliente(int cliente)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@cliente", cliente);
            parameters[1] = new SqlParameter("@proveedor", HttpContext.Session.GetString("ClienteSel"));
            parameters[2] = new()
            {
                ParameterName = "@itemscount",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            };

            SqlDataReader dr = dB.ExecuteReader("ExpoProveedorPrecios", ref parameters, commandType: CommandType.StoredProcedure, "IndigoCS");

            var jsonData = new
            {
                rows = (
                        from row in dr.Cast<System.Data.Common.DbDataRecord>()
                        select new
                        {
                            idproducto = row["IdProducto"],
                            codigo = row["Codigo"],
                            producto = row["Producto"],
                            costo = row["Costo"],
                            comprado = row["Comprado"],
                            carrito = row["Carrito"],
                            pedido = row["Pedido"],
                            preciocliente = row["PrecioCliente"],
                            descuentoexpo = row["DescuentoExpo"],
                            precioexpo = row["PrecioExpo"],
                            descuentoadicional = row["DescuentoAdicional"],
                            preciofinal = row["PrecioFinal"]                                                 
                        }).ToArray()
            };

			dB.Close();

			return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult GuardarAnuncio(int idAnuncio, string descripcion, string feInicio, string feTermino)
        {
            ResponseModel rm = new();

            try 
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[5];
                parameters[0] = new SqlParameter("@IdAnuncio", idAnuncio);
                parameters[1] = new SqlParameter("@Proveedor", HttpContext.Session.GetString("ClienteSel"));
                parameters[2] = new SqlParameter("@Anuncio", descripcion);
                parameters[3] = new SqlParameter("@FeInicio", Convert.ToDateTime(WebSite.FormatFecha(feInicio)));
                parameters[4] = new SqlParameter("@FeTermino", Convert.ToDateTime(WebSite.FormatFecha(feTermino)));

                dB.ExecuteNonQuery("AddAnuncioProv", ref parameters);

                rm.Response = true;
                rm.Message = "El anuncio se guardo correctamente";
            }
            catch (Exception ex)
            {
                rm.Response = false;
                rm.Message = ex.Message;
            }

            return Json(rm);
        }

        [HttpPost]
        public JsonResult imgArticulo(int IdArticulo)
        {
            List<ArticulosProveedores> Articulo = new();

            string query = "SELECT Articulo.IdArticulo, Articulo.Nombre, Articulo.Descripcion, Articulo.IDdArticulos, " +
                "Articulo.Foto, Articulo.Proveedor, Articulo.Cantidad, Articulo.NumFoto " +
                "FROM GetArticuloProv(@IdArticulo) AS Articulo";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdArticulo", IdArticulo);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType:CommandType.Text);

            while (dr.Read())
            {
                ArticulosProveedores DetalleArticulo = new ArticulosProveedores();

                DetalleArticulo.IdArticulo = (Int16)dr["IdArticulo"];
                DetalleArticulo.Nombre = (string)dr["Nombre"];
                DetalleArticulo.Descripcion = (string)dr["Descripcion"];
                DetalleArticulo.IDdArticulos = (dr["IDdArticulos"] != DBNull.Value ? (int)dr["IDdArticulos"] : 0);
                DetalleArticulo.Foto = (dr["Foto"] != DBNull.Value ? (byte[])dr["Foto"] : Array.Empty<byte>());
                DetalleArticulo.NumFoto = (dr["NumFoto"] != DBNull.Value ? (byte)dr["NumFoto"] : (byte)0);

                Articulo.Add(DetalleArticulo);
            }

			dB.Close();

			return Json(Articulo);
        }

        [HttpPost]
        public JsonResult OrdenFoto(int IdArticulo)
        {
            string query = "SELECT Ordenes.OrdenFoto " +
                            "FROM ( " +
                            "		SELECT 1 OrdenFoto " +
                            "		UNION " +
                            "		SELECT 2 OrdenFoto " +
                            "		UNION " +
                            "		SELECT 3 OrdenFoto " +
                            "	 ) AS Ordenes " +
                            "WHERE Ordenes.OrdenFoto NOT IN (SELECT dArticulos.NumFoto FROM dArticulos WHERE dArticulos.IdArticulo = @IdArticulo)";

            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdArticulo", IdArticulo);

            dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            var jsonData = new
            {
                rows = (
                from row in dr.Cast<System.Data.Common.DbDataRecord>()
                 select new
                 {
                     ordenfoto = row["OrdenFoto"]
                 }).ToArray()
            };

			dB.Close();

			return Json(jsonData.rows);
        }

        [HttpPost]
        public void ArticuloGuardarOrdenFoto(int NumFoto, int IDdArticulos)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@IDdArticulos", IDdArticulos);
            parameters[1] = new SqlParameter("@NumFoto", NumFoto);

            dB.ExecuteNonQuery("EditOrdenFotoArticuloProv", ref parameters);
        }

        [HttpPost]
        public JsonResult productoAgregarDescuento(int IdCliente, int IdProducto, decimal PorcentajeDescuento)
        {
            var rm = new ResponseModel();
            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[4];
                parameters[0] = new SqlParameter("@IdProveedor", HttpContext.Session.GetString("ClienteSel"));
                parameters[1] = new SqlParameter("@IdProducto", IdProducto);
                parameters[2] = new SqlParameter("@Descuento", PorcentajeDescuento);
                parameters[3] = new SqlParameter("@Cliente", IdCliente);

                dB.ExecuteNonQuery("AddDescuentoProv", ref parameters);
                rm.Result = true;
                rm.Message = "Descuento agregado con exito";
            }
            catch(Exception ex) 
            {
                rm.Message = ex.Message;
            }
            
            return Json(rm);
        }

        [HttpPost]
        public Int16 ValidaDescuento(decimal DescuentoNuevo, decimal Costo, decimal PrecioExpo)
        {
            Int16 res = 0; //El % de descuento debe ser mayor a 0

            if (DescuentoNuevo > 0)
               res = ValidaDescuentovsCosto(DescuentoNuevo, Costo, PrecioExpo);

            return res;
        }

        protected Int16 ValidaDescuentovsCosto(decimal DescuentoNuevo, decimal Costo, decimal PrecioExpo)
        {
            decimal MontoDescuento = PrecioExpo * DescuentoNuevo / 100;

            Int16 res = 1; //El descuento otorgado es valido
						   
			if ((PrecioExpo - MontoDescuento) <= Costo)
                res = 2; //El descuento otorgado es mayor al costo, mostrar aviso al proveedor

			return res;
        }
    }
}