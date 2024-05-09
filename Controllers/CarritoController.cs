using ExpoMR.Clases;
using ExpoMR.Datos;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Nancy.Conventions;
using Org.BouncyCastle.Utilities;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Cliente,ClienteVIP")]
    public class CarritoController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private static IConfiguration _configuration;

        public CarritoController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "MiCuenta");
            }

            int IdCliente = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));
            int IdUsuario = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));
            bool IsClienteVip = WebSite.ValidaRolUsuario(currentUser.Email, "ClienteVIP");
            bool isFechaExpoVIP = ExpoMR.Clases.WebSite.ValidaExpoVIP();

            Carrito carrito = new Carrito(); 
            carrito.CantidadCarrito = GetCantidadCarrito(IdUsuario, IdCliente);
            carrito.IdUsuario = IdUsuario;
            carrito.IdCliente = IdCliente;
            carrito.TotalesCotizacion = GetTotales(IdCliente, IdUsuario, false); //se inicializan los totales de la cotizacion
            carrito.TotalesPedidos = GetTotales(IdCliente, IdUsuario, true); //se inicializan los totales de los pedidos
            carrito.IdPedido = 0;
            carrito.PedidosCliente = GetddlClientes(IdCliente);
            carrito.IdTipoDoc = 0;
            carrito.TipoDocumentos = WebSite.GetTiposDocumentos(IdCliente);
            carrito.IdUsoCFDI = 0;
            carrito.UsoCFDI = WebSite.GetUsosCFDI(IdCliente);
            carrito.Entrega = 0;
            carrito.Direccciones = WebSite.GetDireccionesEntrega(IdCliente);
            carrito.IdTransporte = 0;
            carrito.Trasportes = WebSite.GetTransportes(IdCliente);
            carrito.IdTipoVenta = 0;
            carrito.TiposVenta = WebSite.GetTiposVenta();

            if ((IsClienteVip == true && isFechaExpoVIP == true)
                || (IsClienteVip == false && isFechaExpoVIP == false))
            {
                return View(carrito);
            }
            else
            {
                return RedirectToAction("Index", "MiCuenta");
            }
        }

        public static List<ListaCarrito> GetCarrito(int IdCliente, int IdUsuario)
        {
            List<ListaCarrito> listaCarrito = new List<ListaCarrito>();

            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@IdCliente", IdCliente);
                parameters[1] = new SqlParameter("@IdUsuario", IdUsuario);

                SqlDataReader dr = dB.ExecuteReader("GetDetailCarrito", ref parameters);

                while (dr.Read())
                {
                    ListaCarrito lcarrito = new ListaCarrito();
                    
                    lcarrito.Registro = Convert.ToInt32(dr["Registro"]);
                    lcarrito.Proveedor = Convert.ToInt32(dr["Proveedor"]);
                    lcarrito.Laboratorio = dr["Laboratorio"].ToString();
                    lcarrito.IdProducto = Convert.ToInt32(dr["IdProducto"]);
                    lcarrito.Nombre = dr["Nombre"].ToString();
                    lcarrito.Presentacion = dr["Presentacion"].ToString();
                    lcarrito.cantidad = Convert.ToInt32(dr["Cantidad"]);
                    lcarrito.PrecioFinal = Convert.ToDecimal(dr["PrecioFinal"]);
                    lcarrito.total = Convert.ToDecimal(dr["Total"]);
                    lcarrito.PorConfirmar = Convert.ToBoolean(dr["PorConfirmar"]);

                    listaCarrito.Add(lcarrito);
                }
            }
            catch (Exception ex)
            {
                listaCarrito = new List<ListaCarrito>();
            }

            return listaCarrito;
        }

        public List<ddlPedidosCliente> GetddlClientes(int IdCliente)
        {
            List<ddlPedidosCliente> ddlPedidos = new List<ddlPedidosCliente>();

            try
            {

                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@IdCliente", IdCliente);

                string query = "SELECT 0 AS IdPedido, 'TODOS' AS Pedido " +
                               "UNION " +
                               "SELECT vPedidos.IdPedido, CAST(vPedidos.IdPedido AS NVARCHAR) AS Pedido " +
                               "FROM vPedidos " +
                               "WHERE vPedidos.Cliente = @IdCliente " +
                               "AND vPedidos.Usuario = @IdCliente " +
                               "GROUP BY vPedidos.IdPedido";

                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

                while (dr.Read())
                {
                    ddlPedidosCliente ddlPedidosCliente = new ddlPedidosCliente()
                    {
                        IdPedido = Convert.ToInt32(dr["IdPedido"]),
                        Pedido = dr["Pedido"].ToString()
                    };

                    ddlPedidos.Add(ddlPedidosCliente);
                }
            }
            catch (Exception e)
            {
                return new List<ddlPedidosCliente>();
            }

            return ddlPedidos;
        }

        [HttpPost]
        public JsonResult AddCotizacion(int cliente, int idProducto, decimal unitario, int cantidad)
        {
            ResponseModel rm = new();

            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[5];

                parameters[0] = new SqlParameter("@cliente", cliente);
                parameters[1] = new SqlParameter("@idproducto", idProducto);
                parameters[2] = new SqlParameter("@unitario", unitario);
                parameters[3] = new SqlParameter("@cantidad", cantidad);
                parameters[4] = new SqlParameter("@usuario", cliente);

                dB.ExecuteNonQuery("CotizarAgregar", ref parameters);

                rm.Response = true;
                rm.Message = "El producto se agrego al carrito";
                rm.Result = GetCantidadCarrito(cliente, cliente);
            }
            catch (Exception e)
            {
                rm.Message = e.Message;
            }

            return Json(rm);
        }

        [HttpPost]
        public JsonResult ExpoPedidoAgregar()
        {
            ResponseModel rm = new();

            try
            {
                string query = "EXEC ExpoPedidoAgregar @cliente = @cliente, @usuario = @usuario, @Id_Inserted = @Id_Inserted OUTPUT";
                DB dB = new();

                SqlParameter[] parameters = new SqlParameter[3];
                parameters[0] = new SqlParameter("@cliente", HttpContext.Session.GetString("ClienteSel"));
                parameters[1] = new SqlParameter("@usuario", HttpContext.Session.GetString("ClienteSel"));
                parameters[2] = new SqlParameter() { ParameterName = "@Id_Inserted", Value = 0, Direction = ParameterDirection.Output };

                dB.ExecuteNonQuery(query, ref parameters, commandType: CommandType.Text);

                rm.Response = true;
                rm.Message = "El pedido ha sido colocado correctamente.";
                rm.Result = parameters[2].Value;
            }
            catch (Exception e)
            {
                rm.Message = e.Message;
                rm.Response = false;
            }

            return Json(rm);
        }

        [HttpPost]
        public JsonResult GetArticuloGanado(int IdArticulo)
        {
            ArticulosProveedores Articulo = new ArticulosProveedores();

            string query = "SELECT Articulo.IdArticulo, Articulo.Nombre, Articulo.Descripcion, Articulo.IDdArticulos, Articulo.Foto, Articulo.Proveedor, Articulo.Cantidad, Articulo.NumFoto FROM GetArticuloProv(@IdArticulo) AS Articulo";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdArticulo", IdArticulo);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                Articulo.IdArticulo = (Int16)dr["IdArticulo"];
                Articulo.Nombre = (string)dr["Nombre"];
                Articulo.Descripcion = (string)dr["Descripcion"];
                Articulo.IDdArticulos = (int)dr["IDdArticulos"];
                Articulo.Foto = (byte[])dr["Foto"];
            }

            return Json(Articulo);
        }

        [HttpPost]
        public JsonResult GetDatosFacturacion(int IdCliente)
        {
            ResponseModel rm = new();
            List<DatosFacturacion> Datos = new List<DatosFacturacion>();

            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[1];

                parameters[0] = new SqlParameter("@IdCliente", IdCliente);
                SqlDataReader dr = dB.ExecuteReader("GetDatosFacturacion", ref parameters);

                while (dr.Read())
                {
                    DatosFacturacion Dato = new DatosFacturacion()
                    {
                        IdCotizacion = Convert.ToInt32(dr["IdCotizacion"]),
                        Cliente = dr["Cliente"].ToString(),
                        RFC = dr["RFC"].ToString(),
                        IdDocumento = Convert.ToInt32(dr["IdDocumento"]),
                        Documento = dr["Documento"].ToString(),
                        IdUsoCFDI = Convert.ToInt32(dr["IdUsoCFDI"]),
                        UsoCFDI = dr["UsoCFDI"].ToString()
                    };

                    Datos.Add(Dato);
                }

                rm.Result = Datos;
                rm.Response = Datos.Count > 0;
                rm.Message = Datos.Count > 0 ? "" : "No Hay Datos Disponibles";
            }
            catch (Exception e)
            {
                rm.Message = e.Message;
                rm.Response = false;
            }

            return Json(rm);
        }

        [HttpPost]
        public JsonResult GetDatosTransporte(int IdCliente, int IdUsuario, int IdDocumento, int IdUsoCFDI)
        {
            Cotizacion UpdCot = new Cotizacion()
            {
                IdCliente = IdCliente,
                IdDocumento = IdDocumento,
                IdUsoCFDI = IdUsoCFDI,
                IdTipoVenta = 0,
                IdTransporte = 0,
                IdPorte = 0,
                Direccion = 0,
                Observaciones = ""
            };

            ActualizaUsoCFDI(UpdCot, IdUsuario);

            ResponseModel rm = new();
            List<Transporte> transportes = new List<Transporte>();

            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@IdCliente", IdCliente);
                parameters[1] = new SqlParameter("@IdUsuario", IdUsuario);

                SqlDataReader dr = dB.ExecuteReader("GetDatosTransporte", ref parameters);

                while (dr.Read())
                {
                    Transporte transporte = new Transporte()
                    {
                        IdCotizacion = Convert.ToInt32(dr["IdCotizacion"]),
                        IdTipoVenta = Convert.ToInt16(dr["IdTipoVenta"]),
                        TipoVenta = dr["TipoVenta"].ToString(),
                        Entrega = Convert.ToInt32(dr["Entrega"]),
                        DirEntrega = dr["DirEntrega"].ToString(),
                        IdTransporte = Convert.ToInt32(dr["IdTransporte"]),
                        Transportista = dr["Transportista"].ToString(),
                        IdPorte = Convert.ToInt16(dr["IdPorte"]),
                        Porte = dr["Porte"].ToString(),
                        Obsv = dr["Obsv"].ToString()
                    };

                    transportes.Add(transporte);
                }

                if (transportes.Count > 0)
                {
                    rm.Result = transportes;
                    rm.Response = true;
                }
                else
                {
                    rm.Response = false;
                    rm.Message = "No hay Promociones Ganadas.";
                }
            }
            catch (Exception e)
            {
                rm.Response = false;
                rm.Message = e.Message;
            }

            return Json(rm);
        }

        public static List<PedidosCarrito> GetPedidosCarrito(int IdCliente, int IdUsuario, int IdPedido = 0)
        {
            List<PedidosCarrito> pedidos = new List<PedidosCarrito>();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[3];

            parameters[0] = new SqlParameter("@IdCliente", IdCliente);
            parameters[1] = new SqlParameter("@IdUsuario", IdUsuario);
            parameters[2] = new SqlParameter("@IdPedido", IdPedido);

            SqlDataReader dr = dB.ExecuteReader("GetPedidosCarrito", ref parameters);

            while (dr.Read())
            {
                PedidosCarrito pedidoCarrito = new PedidosCarrito()
                {
                    RegistroProveedor = Convert.ToInt32(dr["RegistroProveedor"]),
                    Proveedor = Convert.ToInt32(dr["Proveedor"]),
                    RegistroPedido = Convert.ToInt32(dr["RegistroPedido"]),
                    IdPedido = Convert.ToInt32(dr["IdPedido"]),
                    IdProducto = Convert.ToInt32(dr["IdProducto"]),
                    Nombre = dr["Nombre"].ToString(),
                    Presentacion = dr["Presentacion"].ToString(),
                    Cantidad = Convert.ToInt32(dr["Cantidad"]),
                    PrecioFinal = Convert.ToDecimal(dr["PrecioFinal"]),
                    Total = Convert.ToDecimal(dr["Total"]),
                    Laboratorio = dr["Laboratorio"].ToString()
                };

                pedidos.Add(pedidoCarrito);
            }

            return pedidos;
        }

        [HttpPost]
        public JsonResult GetPremios(int IdCliente)
        {
            ResponseModel rm = new();

            try
            {
                List<Premios> premios = new List<Premios>();

                string query = "SELECT CAST(dPromociones.IdArticulo AS INT) as IdArticulo, mProveedores.Nombre AS Proveedor, "
                             + " SUM(dPromociones.Cantidad * dPromoCliente.Cantidad) As Cantidad, mArticulos.Nombre"
                             + " FROM dPromoCliente INNER JOIN"
                             + " dPromociones ON dPromoCliente.IdPromocion = dPromociones.IdPromocion INNER JOIN"
                             + " mPromociones ON dPromoCliente.IdPromocion = mPromociones.IdPromocion INNER JOIN"
                             + " mArticulos ON dPromociones.IdArticulo = mArticulos.IdArticulo INNER JOIN"
                             + " mProveedores ON mArticulos.Proveedor = mProveedores.IdPersona"
                             + " WHERE dPromoCliente.Cliente = @IdCliente"
                             + " GROUP BY dPromociones.IdArticulo, mProveedores.Nombre,mArticulos.Nombre";

                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@IdCliente", IdCliente);
                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

                while (dr.Read())
                {
                    Premios Premio = new Premios()
                    {
                        idArticulo = Convert.ToInt32(dr["IdArticulo"]),
                        proveedor = dr["Proveedor"].ToString(),
                        cantidad = Convert.ToInt32(dr["Cantidad"]),
                        Nombre = dr["Nombre"].ToString()
                    };

                    premios.Add(Premio);
                }
                if (premios.Count > 0)
                {
                    rm.Result = premios;
                    rm.Response = true;
                }
                else
                {
                    rm.Response = false;
                    rm.Message = "No hay Promociones Ganadas.";
                }
            }
            catch (Exception e)
            {
                rm.Message = e.Message;
                rm.Response = false;
            }

            return Json(rm);
        }

        [HttpPost]
        public JsonResult GetSumary(int IdCliente, int IdUsuario, int IdTipoVenta, int IdTransporte, int IdPorte, int IdDireccion, string Obsv)
        {
            Cotizacion UpdCot = new Cotizacion()
            {
                IdCliente = IdCliente,
                IdDocumento = 0,
                IdUsoCFDI = 0,
                IdTipoVenta = IdTipoVenta,
                IdTransporte = IdTransporte,
                IdPorte = IdPorte,
                Direccion = IdDireccion,
                Observaciones = Obsv
            };

            ActualizaUsoCFDI(UpdCot, IdUsuario);

            ResponseModel rm = new();

            try
            {
                List<CotizacionDatos> Cotizaciones = new List<CotizacionDatos>();

                string query = "DECLARE @idcotizacion INT EXEC @idcotizacion = dbo.CotizarGetCotizacion @IdCliente, @IdUsuario " +
                " SELECT vCotizacionDatos.IdCotizacion, vCotizacionDatos.Cliente, vCotizacionDatos.RFC, vCotizacionDatos.Documento," +
                " vCotizacionDatos.UsoCFDI, vCotizacionDatos.DirEntrega, vCotizacionDatos.TipoVenta, vCotizacionDatos.Transportista," +
                " vCotizacionDatos.Porte, vCotizacionDatos.Obsv " +
                " FROM vCotizacionDatos " +
                " WHERE vCotizacionDatos.IdCotizacion = @idcotizacion";
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@IdCliente", IdCliente);
                parameters[1] = new SqlParameter("@IdUsuario", IdUsuario);

                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

                while (dr.Read())
                {
                    CotizacionDatos Cotizacion = new CotizacionDatos()
                    {
                        IdCotizacion = Convert.ToInt32(dr["IdCotizacion"]),
                        Cliente = dr["IdCotizacion"].ToString(),
                        RFC = dr["RFC"].ToString(),
                        Documento = dr["Documento"].ToString(),
                        UsoCFDI = dr["UsoCFDI"].ToString(),
                        DirEntrega = dr["DirEntrega"].ToString(),
                        TipoVenta = dr["TipoVenta"].ToString(),
                        Transportista = dr["Transportista"].ToString(),
                        Porte = dr["Porte"].ToString(),
                        Obsv = dr["Obsv"].ToString()
                    };

                    Cotizaciones.Add(Cotizacion);
                }

                if (Cotizaciones.Count > 0)
                {
                    rm.Result = Cotizaciones;
                    rm.Response = true;
                }
                else
                {
                    rm.Response = false;
                    rm.Message = "No hay Cotización aun.";
                }
            }
            catch (Exception e)
            {
                rm.Message = e.Message;
                rm.Response = false;
            }

            return Json(rm);
        }

        [HttpPost]
        public JsonResult ProductoDelete(int IdProducto, int Cliente)
        {
			ResponseModel rm = new();

			try
			{
				DB dB = new();
				SqlParameter[] parameters = new SqlParameter[3];
			
				parameters[0] = new SqlParameter("@cliente", Cliente);
				parameters[1] = new SqlParameter("@usuario", Cliente);
				parameters[2] = new SqlParameter("@IdProducto", IdProducto);

				dB.ExecuteNonQuery("ExpoPedidoQuitar", ref parameters);

				rm.Response = true;
				rm.Message = "producto eliminado";
			}
			catch (Exception e)
			{
				rm.Message = e.Message;
			}

            return Json(rm);
        }

        [HttpPost]
        public JsonResult Validarcolocar(int IdCliente, int IdUsuario)
        {
            ResponseModel rm = new();

            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@cliente", IdCliente);
                parameters[1] = new SqlParameter("@usuario", IdUsuario);

                rm.Result = dB.ExecuteReturnValue("ExpoPedidoValidar", ref parameters); ;
                rm.Response = true;
            }
            catch (Exception e)
            {
                rm.Message = e.Message;
                rm.Response = false;
            }

            return Json(rm);
        }

        public bool ActualizaUsoCFDI(Cotizacion cotizacion, int IdUsuario)
        {
            bool result;

            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[9];

                parameters[0] = new SqlParameter("@IdCliente", cotizacion.IdCliente);
                parameters[1] = new SqlParameter("@IdUsuario", IdUsuario);
                parameters[2] = new SqlParameter("@IdDocumento", cotizacion.IdDocumento);
                parameters[3] = new SqlParameter("@IdUsoCFDI", cotizacion.IdUsoCFDI);
                parameters[4] = new SqlParameter("@IdTipoVenta", cotizacion.IdTipoVenta);
                parameters[5] = new SqlParameter("@IdTransporte", cotizacion.IdTransporte);
                parameters[6] = new SqlParameter("@IdPorte", cotizacion.IdPorte);
                parameters[7] = new SqlParameter("@IdDireccion", cotizacion.Direccion);
                parameters[8] = new SqlParameter("@Obsv", cotizacion.Observaciones);


                dB.ExecuteNonQuery("CotizarActualizarDatos", ref parameters);

                result = true;
            }
            catch (Exception e)
            {
                result = false;
            }

            return result;
        }

        public static Totales GetTotales(int IdCliente, int IdUsuario, bool Origen)
        {
            Totales totales = new Totales();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@cliente", IdCliente);
            parameters[1] = new SqlParameter("@IdPersona", IdUsuario);
            parameters[2] = new SqlParameter("@Origen", Origen);

            SqlDataReader dr = dB.ExecuteReader("GetTotales", ref parameters);

            if (dr.Read())
            {
                totales.Productos = (dr["Productos"] != DBNull.Value ? (Int32)dr["Productos"] : 0);
                totales.Subtotal = (dr["Subtotal"] != DBNull.Value ? (decimal)dr["Subtotal"] : 0);
                totales.IVA = (dr["Iva"] != DBNull.Value ? (decimal)dr["Iva"] : 0);
                totales.Total = (dr["Total"] != DBNull.Value ? (decimal)dr["Total"] : 0);
                totales.Credito = (dr["Credito"] != DBNull.Value ? (decimal)dr["Credito"] : 0);
                totales.HasProduct = (dr["Productos"] != DBNull.Value ? true : false);
            }

            return totales;
        }

        public int GetCantidadCarrito(int IdPersona, int cliente)
        {
            int resultCantidad = 0;

            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@cliente", cliente);
                parameters[1] = new SqlParameter("@usuario", IdPersona);

                SqlDataReader dr = dB.ExecuteReader("ExpoCountProducts", ref parameters);

                if (dr.Read())
                {
                    resultCantidad = Convert.ToInt32(dr["Productos"]);
                }
            }
            catch
            {
                resultCantidad = 0;
            }

            return resultCantidad;
        }

        public static byte[] GetLogoProveedor(string Tipo, int Id)
        {
			DB dB = new();
            byte[] logo = new byte[0];

			SqlParameter[] parameters = new SqlParameter[2];
			parameters[0] = new SqlParameter("@Type", Tipo);
			parameters[1] = new SqlParameter("@Id", Id);

			SqlDataReader dr = dB.ExecuteReader("ExpoGetImage", ref parameters, CommandType.StoredProcedure);

			if (dr.Read())
            {
                logo = (dr["Logo"] != DBNull.Value ? (byte[])dr["Logo"] : Array.Empty<byte>());
            }
			
            return logo;
        }

        public void PorConfirmar(int IdProducto, int Cliente, int Confirmado)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[4];

            parameters[0] = new SqlParameter("@IdProducto", IdProducto);
            parameters[1] = new SqlParameter("@cliente", Cliente);
            parameters[2] = new SqlParameter("@usuario", Cliente);
            parameters[3] = new SqlParameter("@Confirmado", Confirmado);

            dB.ExecuteNonQuery("ConfirmarProducto", ref parameters);
        }
    }
}