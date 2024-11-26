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
        private readonly IWebHostEnvironment webHostEnvironment;

        public CarritoController(UserManager<IdentityUser> userManager, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _configuration = configuration;
            this.webHostEnvironment = webHostEnvironment;
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
            int isFechaExpoVIP = ExpoMR.Clases.WebSite.ValidaExpoVIP();

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
            carrito.IdPorte = 0;
            carrito.Portes = WebSite.GetPortes(IdCliente);
            carrito.IdTipoVenta = 0;
            carrito.TiposVenta = WebSite.GetTiposVenta();
            carrito.ListaCarrito = GetCarrito(IdCliente, IdUsuario);

            if ((IsClienteVip == true && isFechaExpoVIP == 1) //1 = Expo VIP
                || (IsClienteVip == false && isFechaExpoVIP == 0)) // 0 = Expo Normal
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
			DB dB = new();

			try
            {
                string query = "SELECT CAST(ROW_NUMBER() OVER(PARTITION BY vProductos.IdLaboratorio ORDER BY mProveedores.Nombre, vProductos.Nombre ASC) AS int)AS Registro, " +
                                "mLaboratorios.Proveedor, mLaboratorios.Laboratorio, vProductos.IdProducto, vProductos.Nombre, vProductos.Presentacion, " +
                                "vCotizaciones.Cantidad, vCotizaciones.PrecioFinal, vCotizaciones.Total, vCotizaciones.PorConfirmar " +
                                "FROM vCotizaciones INNER JOIN  " +
                                "vProductos ON vCotizaciones.IdProducto = vProductos.IdProducto INNER JOIN  " +
                                "mLaboratorios ON vProductos.IdLaboratorio = mLaboratorios.IdLaboratorio INNER JOIN  " +
                                "mProveedores ON mLaboratorios.Proveedor = mProveedores.IdPersona  " +
                                "WHERE vCotizaciones.Cliente =  @IdCliente " +
                                "AND vCotizaciones.Usuario =  @IdUsuario " +
                                "AND vCotizaciones.Solicitado = 1 " +
                                "ORDER BY mProveedores.Nombre, mLaboratorios.Laboratorio, vProductos.Nombre";
               
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@IdCliente", IdCliente);
                parameters[1] = new SqlParameter("@IdUsuario", IdUsuario);

                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

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

            dB.Close();

            return listaCarrito;
        }
        public List<ddlPedidosCliente> GetddlClientes(int IdCliente)
        {
            List<ddlPedidosCliente> ddlPedidos = new List<ddlPedidosCliente>();
			DB dB = new();

			try
            {
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

            dB.Close();

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

            try {
                string usoCFDI = HttpContext.Session.GetString("ClienteSel");
                string query = "SELECT mPersonas.Persona, mPersonas.Nombre, mSatUsoCFDI.UsoCFDI " +
                    "FROM Indigo.dbo.mPersonas LEFT OUTER JOIN " +
                    "Indigo.dbo.mEmpresas ON mPersonas.Persona = mEmpresas.Persona LEFT OUTER JOIN " +
                    "Indigo.dbo.mSatRegimenFiscal ON mPersonas.IdRegimenFiscal = mSatRegimenFiscal.IdRegimenFiscal LEFT OUTER JOIN " +
                    "Indigo.dbo.mSatUsoCFDI ON mPersonas.IdUsoCFDI = mSatUsoCFDI.IdUsoCFDI " +
                    "where mPersonas.Persona = @IdPersona";

                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@IdPersona", HttpContext.Session.GetString("ClienteSel"));

                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

                while (dr.Read())
                {
                   usoCFDI = dr["UsoCFDI"] == DBNull.Value ? "": (string)dr["UsoCFDI"];
                }

                dB.Close();

                if (usoCFDI == "") {
                    rm.Message = "El usuario no tiene un uso de CFDI en Indigo";
                    return Json(rm);
                }
            }
            catch (Exception ex){
                rm.Message=ex.Message;
                return Json(rm);
            }

            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[3];
                parameters[0] = new SqlParameter("@cliente", HttpContext.Session.GetString("ClienteSel"));
                parameters[1] = new SqlParameter("@usuario", HttpContext.Session.GetString("ClienteSel"));
                parameters[2] = new SqlParameter() { ParameterName = "@Id_Inserted", Value = 0, Direction = ParameterDirection.Output };

                bool ret = Convert.ToBoolean(dB.ExecuteReturnValue("ExpoPedidoAgregar", ref parameters));

                rm.Response = ret;
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
                Articulo.IDdArticulos = (dr["IDdArticulos"] != DBNull.Value ? (int)dr["IDdArticulos"] : 0);
                Articulo.Foto = dr["Foto"] != DBNull.Value ? (byte[])dr["Foto"] : WebSite.GetImageDefault(webHostEnvironment, "noimage.png") ;
            }

			dB.Close();

			return Json(Articulo);
        }


        [HttpPost]
        public JsonResult GetTotalByPedido(int IdPedido)
        {
            ResumenPedido Pedido = new();

            string query = "SELECT mPedidos.IdPedido, COUNT(dbo.dPedidos.IdProducto) AS Productos, " +
                " CAST(SUM(dbo.dPedidos.Cantidad * (dbo.dPedidos.Unitario - dbo.dPedidos.Descuento)) AS DECIMAL(18, 2)) AS Subtotal, " +
                " CAST(SUM(CASE WHEN mProductos.Iva = 1 THEN dPedidos.Cantidad * (dPedidos.Unitario - dPedidos.Descuento) * Indigo.dbo.GetIva(0) ELSE 0 END) AS DECIMAL(18, 2)) AS IvaMonto," +
                " CAST(SUM(dbo.dPedidos.Cantidad * (dbo.dPedidos.Unitario - dbo.dPedidos.Descuento)) + SUM(CASE WHEN mProductos.Iva = 1 THEN dPedidos.Cantidad * (dPedidos.Unitario - dPedidos.Descuento) * Indigo.dbo.GetIva(0)  ELSE 0 END) AS DECIMAL(18, 2)) AS Total" +
                " FROM dbo.mPedidos " +
                " INNER JOIN dbo.dPedidos ON dbo.mPedidos.IdPedido = dbo.dPedidos.IdPedido " +
                " INNER JOIN dbo.mProductos ON dbo.dPedidos.IdProducto = dbo.mProductos.IdProducto" +
                " WHERE mPedidos.IdPedido = @IdPedido" +
                " GROUP BY mPedidos.IdPedido";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdPedido", IdPedido);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                Pedido.Pedido = (int)dr["IdPedido"];
                Pedido.Cantidad = (int)(dr["Productos"]);
                Pedido.Subtotal = Convert.ToDecimal(dr["Subtotal"]);
                Pedido.IVA = Convert.ToDecimal(dr["IvaMonto"]);
                Pedido.Total = Convert.ToDecimal(dr["Total"]);
            }

			dB.Close();

			return Json(Pedido);
        }

        [HttpPost]
        public JsonResult GetDatosFacturacion(int IdCliente)
        {
            ResponseModel rm = new();
            List<DatosFacturacion> Datos = new List<DatosFacturacion>();
			DB dB = new();

			try
            {
                string query = "DECLARE @idcotizacion INT " +
                                "EXEC @idcotizacion = dbo.CotizarGetCotizacion  @IdCliente,  @IdCliente " +
                                "SELECT vCotizacionDatos.IdCotizacion, vCotizacionDatos.Cliente, vCotizacionDatos.RFC, " +
                                "vCotizacionDatos.IdDocumento, vCotizacionDatos.Documento, " +
                                "ISNULL(vCotizacionDatos.IdUsoCFDI, 0) AS IdUsoCFDI, vCotizacionDatos.UsoCFDI " +
                                "FROM vCotizacionDatos " +
                                "WHERE vCotizacionDatos.IdCotizacion = @idcotizacion";

                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@IdCliente", IdCliente);
                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

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

            dB.Close();

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
			DB dB = new();

			try
            {
                string query = "DECLARE @idcotizacion INT " +
                                "EXEC @idcotizacion = dbo.CotizarGetCotizacion  @IdCliente,  @IdUsuario " +
                                "SELECT vCotizacionDatos.IdCotizacion, vCotizacionDatos.IdTipoVenta, vCotizacionDatos.TipoVenta, " +
                                "vCotizacionDatos.Entrega, vCotizacionDatos.DirEntrega, ISNULL(vCotizacionDatos.IdTransporte,0) AS IdTransporte, ISNULL(vCotizacionDatos.Transportista,0) AS Transportista, " +
                                "ISNULL(vCotizacionDatos.IdPorte,0) AS IdPorte, ISNULL(vCotizacionDatos.Porte,0) AS Porte, ISNULL(vCotizacionDatos.Obsv,'') AS Obsv " +
                                "FROM vCotizacionDatos " +
                                "WHERE vCotizacionDatos.IdCotizacion = @idcotizacion";

                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@IdCliente", IdCliente);
                parameters[1] = new SqlParameter("@IdUsuario", IdUsuario);

                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

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

            dB.Close();

            return Json(rm);
        }

        public static List<PedidosCarrito> GetPedidosCarrito(int IdCliente, int IdUsuario, int IdPedido = 0)
        {
            List<PedidosCarrito> pedidos = new List<PedidosCarrito>();

            string query = "SELECT ROW_NUMBER() OVER(PARTITION BY vPedidos.IdPedido,vProductos.IdLaboratorio " +
                            "ORDER BY mProveedores.Nombre, vProductos.Nombre ASC) AS RegistroProveedor, mLaboratorios.Proveedor, " +
                            "ROW_NUMBER() OVER(PARTITION BY vProductos.IdLaboratorio, vPedidos.IdPedido ORDER BY mProveedores.Nombre, " +
                            "vProductos.Nombre ASC) AS RegistroPedido, vPedidos.IdPedido, vProductos.IdProducto, vProductos.Nombre, " +
                            "vProductos.Presentacion, vPedidos.Cantidad, vPedidos.PrecioFinal, vPedidos.Total, mLaboratorios.Laboratorio " +
                            "FROM vPedidos INNER JOIN " +
                            "vProductos ON vPedidos.IdProducto = vProductos.IdProducto INNER JOIN " +
                            "mLaboratorios ON vProductos.IdLaboratorio = mLaboratorios.IdLaboratorio INNER JOIN " +
                            "mProveedores ON mLaboratorios.Proveedor = mProveedores.IdPersona " +
                            "WHERE vPedidos.Cliente = @IdCliente " +
                            "AND   vPedidos.Usuario = @IdUsuario " +
                            "AND ( vPedidos.IdPedido =  @IdPedido OR @IdPedido = 0) " +
                            "ORDER BY vPedidos.IdPedido, mProveedores.Nombre, mLaboratorios.Laboratorio, vProductos.Nombre";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[3];

            parameters[0] = new SqlParameter("@IdCliente", IdCliente);
            parameters[1] = new SqlParameter("@IdUsuario", IdUsuario);
            parameters[2] = new SqlParameter("@IdPedido", IdPedido);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

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

            dB.Close();

            return pedidos;
        }

        [HttpPost]
        public JsonResult GetPremios(int IdCliente)
        {
            ResponseModel rm = new();
			DB dB = new();

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

            dB.Close();

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
			DB dB = new();

			try
            {
                List<CotizacionDatos> Cotizaciones = new List<CotizacionDatos>();

                string query = "DECLARE @idcotizacion INT EXEC @idcotizacion = dbo.CotizarGetCotizacion @IdCliente, @IdUsuario " +
                " SELECT vCotizacionDatos.IdCotizacion, vCotizacionDatos.Cliente, vCotizacionDatos.RFC, vCotizacionDatos.Documento," +
                " vCotizacionDatos.UsoCFDI, vCotizacionDatos.DirEntrega, vCotizacionDatos.TipoVenta, vCotizacionDatos.Transportista," +
                " vCotizacionDatos.Porte, vCotizacionDatos.Obsv " +
                " FROM vCotizacionDatos " +
                " WHERE vCotizacionDatos.IdCotizacion = @idcotizacion";
                
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@IdCliente", IdCliente);
                parameters[1] = new SqlParameter("@IdUsuario", IdUsuario);

                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

                while (dr.Read())
                {
                    CotizacionDatos Cotizacion = new CotizacionDatos()
                    {
                        IdCotizacion = Convert.ToInt32(dr["IdCotizacion"]),
                        Cliente = dr["Cliente"].ToString(),
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

            dB.Close();

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

                rm.Result = 2;//s (int)dB.ExecuteScalar("ExpoPedidoValidar", ref parameters);
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
                parameters[8] = new SqlParameter("@Obsv", cotizacion.Observaciones == null ? "" : cotizacion.Observaciones);

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

            dB.Close();

            return totales;
        }

        public int GetCantidadCarrito(int IdPersona, int cliente)
        {
            int resultCantidad = 0;
			DB dB = new();

			try
            {
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

            dB.Close();

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

            dB.Close();

			return logo;
        }

        public JsonResult PorConfirmar(int IdProducto, int Cliente, int Confirmado) //Confirmado = 1 Marcar Todos // confirmado = 2 DesMarcar Todo
        {
            var rm = new ResponseModel();
            DB dB = new();
            try
            {
                SqlParameter[] parameters = new SqlParameter[4];

                parameters[0] = new SqlParameter("@IdProducto", IdProducto);
                parameters[1] = new SqlParameter("@cliente", Cliente);
                parameters[2] = new SqlParameter("@usuario", Cliente);
                parameters[3] = new SqlParameter("@Confirmado", Confirmado);
                dB.ExecuteNonQuery("ConfirmarProducto", ref parameters);
                dB.Close();
                rm.Response = true;
            } catch (Exception ex) {
                rm.Response = false;
                rm.Message = ex.Message;
                dB.Close();
            }

            return Json(rm);
        }
    }
}