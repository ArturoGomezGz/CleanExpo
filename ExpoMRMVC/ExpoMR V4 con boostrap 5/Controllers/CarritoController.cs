using ExpoMR.Clases;
using ExpoMR.Datos;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

            string username = currentUser.UserName.ToString();
            int IdPersona = GeneralesController.GetIdPersonas(username);
            int.TryParse(HttpContext.Session.GetString("ClienteSel"), out int cliente);
            bool isFechaExpoVIP = WebSite.ValidaExpoVIP();
            bool isFechaExpo = WebSite.ValidaExpoVIP();
            bool IsClienteVip = WebSite.ValidaRolUsuario(currentUser.Email, "ClienteVIP");
            Carrito carrito = GetCarritoUsuario(cliente, IdPersona);

            if (carrito != null)
            {
                carrito.CantidadCarrito = GetCantidadCarrito(IdPersona, cliente);
                carrito.IdUsuario = IdPersona;
                carrito.IdCliente = cliente;
                carrito.HasProduct = true;
            }
            else
            {
                carrito = new Carrito();
                carrito.HasProduct = false;
                carrito.Productos = 0;
                carrito.Subtotal = 0;
                carrito.IVA = 0;
                carrito.Total = 0;
                carrito.Credito = 0;
            }

            carrito.IdPedido = 0;
            carrito.PedidosCliente = GetddlClientes(cliente);
            carrito.IdTipoDoc = 0;
            carrito.TipoDocumentos = GetTipoDocumentos(cliente);
            carrito.IdUsoCFDI = 0;
            carrito.UsoCFDI = GetUsoCFDIs(cliente);
            carrito.Entrega = 0;
            carrito.Direccciones = GetDirecciones(cliente);
            carrito.IdTransporte = 0;
            carrito.Trasportes = GetTrasportes(cliente);
            carrito.IdTipoVenta = 0;
            carrito.TiposVenta = GetTiposVenta();

            if ((IsClienteVip == true && isFechaExpoVIP)
                || (IsClienteVip == false && isFechaExpo))
            {
                return View(carrito);
            }
            else
            {
                return RedirectToAction("Index", "MiCuenta");
            }
        }

        public static Carrito GetCarritoUsuario(int IdCliente, int IdUsuario)
        {
            Carrito carrito = new Carrito();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@cliente", IdCliente);
            parameters[1] = new SqlParameter("@IdPersona", IdUsuario);

            SqlDataReader dr = dB.ExecuteReader("GetCarrito", ref parameters);

            if (dr.Read())
            {
                carrito.Productos = Convert.ToInt32(dr["Productos"]);
                carrito.Subtotal = Convert.ToDecimal(dr["Subtotal"]);
                carrito.IVA = Convert.ToDecimal(dr["Iva"]);
                carrito.Total = Convert.ToDecimal(dr["Total"]);
                carrito.Credito = Convert.ToDecimal(dr["Credito"]);
            }

            return carrito;
        }

        public static List<ListaCarrito> GetCarrito(int IdCliente, int IdUsuario)
        {
            List<ListaCarrito> listaCarrito = new List<ListaCarrito>();

            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@cliente", IdCliente);
                parameters[1] = new SqlParameter("@IdPersona", IdUsuario);

                SqlDataReader dr = dB.ExecuteReader("GetCarrito", ref parameters);

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

                    lcarrito.imgProducto = GeneralesController.GetFoto(lcarrito.IdProducto);
                    listaCarrito.Add(lcarrito);
                }
            }
            catch (Exception ex)
            {
                listaCarrito = new List<ListaCarrito>();
            }

            return listaCarrito;
        }

        public int GetCantidadCarrito(int IdPersona, int cliente)
        {
            var dbExpo = new ApplicationDbContextExpo();
            List<SqlParameter> parms = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "@cliente", Value = cliente },
                new SqlParameter { ParameterName = "@usuario", Value = IdPersona }
            };
            try
            {
                var returnStore = dbExpo.Set<ReturnStore>().FromSqlRaw($"EXEC ExpoCountProducts @cliente = {parms[0].Value}, @usuario = {parms[1].Value}").ToList();
                return returnStore[0].Productos;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return 0;
        }

        [HttpPost]
        public JsonResult ProductoDelete(int IdProducto, int Cliente)
        {
            var dbExpo = new ApplicationDbContextExpo();
            ResponseModel rm = new();

            try
            {
                var user = _userManager.GetUserAsync(User);
                string username = user.Result.UserName.ToString();
                int Usuario = GeneralesController.GetIdPersonas(username);

                dbExpo.Database.ExecuteSqlRaw("DECLARE @idcotizacion INT " +
                                "EXEC @idcotizacion = dbo.CotizarGetCotizacion " + Cliente + "," + Usuario +
                                "UPDATE dCotizaciones " +
                                "SET Solicitado = 0, " +
                                "PorConfirmar = 0 " +
                                "WHERE dCotizaciones.IdCotizacion = @idcotizacion " +
                                "AND dCotizaciones.IdProducto = " + IdProducto);
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
        public JsonResult AddCotizacion(int cliente, int idProducto, decimal unitario, int cantidad)
        {
            var dbExpo = new ApplicationDbContextExpo();
            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            int IdPersona = GeneralesController.GetIdPersonas(username);
            var parameters = new object[]
                {
                    cliente,
                    idProducto,
                    unitario,
                    cantidad,
                    IdPersona
                };
            ResponseModel rm = new();
            try
            {
                dbExpo.Database.ExecuteSqlRaw("EXEC CotizarAgregar @cliente = {0}, @idproducto = {1}, @unitario = {2}, @cantidad = {3}, @usuario = {4}", parameters);

                rm.Response = true;
                rm.Message = "El producto se agrego al carrito";
                rm.Result = GetCantidadCarrito(IdPersona, cliente);
            }
            catch (Exception e)
            {
                rm.Message = e.Message;
            }
            return Json(rm);
        }

        [HttpPost]
        public JsonResult GetPedidosCarrito(int IdCliente, int IdUsuario, int IdPedido = 0)
        {
            var dbExpo = new ApplicationDbContextExpo();
            ResponseModel rm = new();
            try
            {
                List<PedidosCarrito> pedidos = dbExpo.Set<PedidosCarrito>()
                    .FromSqlRaw("SELECT ROW_NUMBER() OVER(PARTITION BY vPedidos.IdPedido,vProductos.IdLaboratorio " +
                               "ORDER BY mProveedores.Nombre, vProductos.Nombre ASC) AS RegistroProveedor, mLaboratorios.Proveedor, " +
                               "ROW_NUMBER() OVER(PARTITION BY vProductos.IdLaboratorio, vPedidos.IdPedido ORDER BY mProveedores.Nombre, " +
                               "vProductos.Nombre ASC) AS RegistroPedido, vPedidos.IdPedido, vProductos.IdProducto, vProductos.Nombre, " +
                               "vProductos.Presentacion, vPedidos.Cantidad, vPedidos.PrecioFinal, vPedidos.Total, mLaboratorios.Laboratorio " +
                               "FROM vPedidos INNER JOIN " +
                               "vProductos ON vPedidos.IdProducto = vProductos.IdProducto INNER JOIN " +
                               "mLaboratorios ON vProductos.IdLaboratorio = mLaboratorios.IdLaboratorio INNER JOIN " +
                               "mProveedores ON mLaboratorios.Proveedor = mProveedores.IdPersona " +
                               " WHERE vPedidos.Cliente = " + IdCliente +
                               " AND vPedidos.Usuario = " + IdUsuario +
                               " AND (vPedidos.IdPedido = " + IdPedido + " OR " + IdPedido + " = 0) " +
                               "ORDER BY vPedidos.IdPedido, mProveedores.Nombre, mLaboratorios.Laboratorio, vProductos.Nombre")
                    .ToList();

                if (pedidos.Count > 0)
                {
                    rm.Result = pedidos;
                    rm.Response = true;
                }
                else
                {
                    rm.Response = false;
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
        public JsonResult GetDatosFacturacion(int IdCliente)
        {
            var dbExpo = new ApplicationDbContextExpo();
            ResponseModel rm = new();
            try
            {
                List<DatosFacturacion> datos = dbExpo.Set<DatosFacturacion>().FromSqlRaw("DECLARE @idcotizacion INT" +
                    " EXEC @idcotizacion = dbo.CotizarGetCotizacion " + IdCliente + ", " + IdCliente + " " +
                    "SELECT vCotizacionDatos.IdCotizacion, vCotizacionDatos.Cliente, vCotizacionDatos.RFC, vCotizacionDatos.IdDocumento, " +
                    "vCotizacionDatos.Documento, ISNULL(vCotizacionDatos.IdUsoCFDI, 0) AS IdUsoCFDI, vCotizacionDatos.UsoCFDI " +
                    "FROM vCotizacionDatos WHERE vCotizacionDatos.IdCotizacion = @idcotizacion").ToList();

                if (datos.Count > 0)
                {
                    rm.Result = datos;
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
        public JsonResult GetDatosTransporte(int IdCliente, int IdUsuario, int IdDocumento, int IdUsoCFDI)
        {
            var dbExpo = new ApplicationDbContextExpo();
            try
            {
                dbExpo.Database.ExecuteSqlRaw("DECLARE @idcotizacion INT" +
                    " EXEC @idcotizacion = dbo.CotizarGetCotizacion " + IdCliente + ", " + IdUsuario + " " +
                    " UPDATE mCotizaciones " +
                    " SET mCotizaciones.IdDocumento = " + IdDocumento + "," +
                    " mCotizaciones.IdUsoCFDI = CASE WHEN " + IdDocumento + " = 1 " +
                    " THEN " + IdUsoCFDI + " ELSE 23 END " + // 23 S01 Sin Efectos Fiscales 
                    " WHERE mCotizaciones.IdCotizacion = @idcotizacion");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            ResponseModel rm = new();
            try
            {
                List<Transporte> transportes = dbExpo.Set<Transporte>()
                    .FromSqlRaw("DECLARE @idcotizacion INT EXEC @idcotizacion = dbo.CotizarGetCotizacion " + IdCliente + ", " + IdUsuario + " " +
                    " SELECT vCotizacionDatos.IdCotizacion, vCotizacionDatos.IdTipoVenta, vCotizacionDatos.TipoVenta," +
                    " vCotizacionDatos.Entrega, vCotizacionDatos.DirEntrega, vCotizacionDatos.IdTransporte, vCotizacionDatos.Transportista," +
                    " vCotizacionDatos.IdPorte, vCotizacionDatos.Porte, vCotizacionDatos.Obsv " +
                    " FROM vCotizacionDatos" +
                    " WHERE vCotizacionDatos.IdCotizacion = @idcotizacion")
                    .ToList();

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
                rm.Message = e.Message;
                rm.Response = false;
            }
            return Json(rm);
        }

        [HttpPost]
        public JsonResult GetSumary(int IdCliente, int IdUsuario, int IdTipoVenta, int IdTransporte, int IdPorte, int IdDireccion, string Obsv)
        {
            var dbExpo = new ApplicationDbContextExpo();
            try
            {
                dbExpo.Database.ExecuteSqlRaw("DECLARE @idcotizacion INT" +
                    " EXEC @idcotizacion = dbo.CotizarGetCotizacion " + IdCliente + ", " + IdUsuario + " " +
                    " UPDATE mCotizaciones" +
                    " SET mCotizaciones.IdTipoVenta = " + IdTipoVenta + "," +
                    " mCotizaciones.IdTransporte = CASE WHEN " + IdTransporte + " = 0 THEN NULL ELSE " + IdTransporte + " END," +
                    " mCotizaciones.IdPorte = CASE WHEN " + IdPorte + " = 0 THEN NULL ELSE " + IdPorte + " END," +
                    " mCotizaciones.Entrega = " + IdDireccion + "," +
                    " mCotizaciones.Obsv = '" + Obsv + "'" +
                    " WHERE mCotizaciones.IdCotizacion = @idcotizacion");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            ResponseModel rm = new();
            try
            {
                List<ResumenPedido> resumen = dbExpo.Set<ResumenPedido>()
                    .FromSqlRaw("DECLARE @idcotizacion INT EXEC @idcotizacion = dbo.CotizarGetCotizacion " + IdCliente + ", " + IdUsuario + " " +
                    " SELECT vCotizacionDatos.IdCotizacion, vCotizacionDatos.Cliente, vCotizacionDatos.RFC, vCotizacionDatos.Documento," +
                    " vCotizacionDatos.UsoCFDI, vCotizacionDatos.DirEntrega, vCotizacionDatos.TipoVenta, vCotizacionDatos.Transportista," +
                    " vCotizacionDatos.Porte, vCotizacionDatos.Obsv " +
                    " FROM vCotizacionDatos" +
                    " WHERE vCotizacionDatos.IdCotizacion = @idcotizacion").ToList();

                if (resumen.Count > 0)
                {
                    rm.Result = resumen;
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
        public JsonResult ExpoPedidoAgregar()
        {
            var dbExpo = new ApplicationDbContextExpo();
            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            int IdPersona = GeneralesController.GetIdPersonas(username);
            int cliente = GeneralesController.GetClientes(username);
            List<SqlParameter> parms = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "@cliente", Value = cliente },
                new SqlParameter { ParameterName = "@usuario", Value = IdPersona },
                new SqlParameter { ParameterName = "@Id_Inserted",  SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Output }
            };
            ResponseModel rm = new();
            try
            {
                string query = "EXEC ExpoPedidoAgregar @cliente = @cliente, @usuario = @usuario, @Id_Inserted = @Id_Inserted OUTPUT";

                // Pasa los parámetros a través de la función ExecuteSqlRaw
                dbExpo.Database.ExecuteSqlRaw(query, parms.ToArray());

                rm.Response = true;
                rm.Message = "El pedido ha sido colocado correctamente.";
                rm.Result = parms[2].Value;
            }
            catch (Exception e)
            {
                rm.Message = e.Message;
                rm.Response = false;
            }
            return Json(rm);
        }

        [HttpPost]
        public JsonResult GetPremios(int IdCliente)
        {
            var dbExpo = new ApplicationDbContextExpo();
            ResponseModel rm = new();
            try
            {
                List<Premios> premios = dbExpo.Set<Premios>().FromSqlRaw("SELECT CAST(dPromociones.IdArticulo AS INT) as IdArticulo, mProveedores.Nombre AS Proveedor, "
                                + " SUM(dPromociones.Cantidad * dPromoCliente.Cantidad) As Cantidad, mArticulos.Nombre"
                                + " FROM dPromoCliente INNER JOIN"
                                + " dPromociones ON dPromoCliente.IdPromocion = dPromociones.IdPromocion INNER JOIN"
                                + " mPromociones ON dPromoCliente.IdPromocion = mPromociones.IdPromocion INNER JOIN"
                                + " mArticulos ON dPromociones.IdArticulo = mArticulos.IdArticulo INNER JOIN"
                                + " mProveedores ON mArticulos.Proveedor = mProveedores.IdPersona"
                                + " WHERE dPromoCliente.Cliente =" + IdCliente
                                + " GROUP BY dPromociones.IdArticulo, mProveedores.Nombre,mArticulos.Nombre").ToList();

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
        public JsonResult Validarcolocar(int IdCliente, int IdUsuario)
        {
            var dbExpo = new ApplicationDbContextExpo();
            List<SqlParameter> parms = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "@cliente", Value = IdCliente },
                new SqlParameter { ParameterName = "@usuario", Value = IdUsuario }
            };
            var resultadoParameter = new SqlParameter("@success", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            ResponseModel rm = new();
            try
            {
                dbExpo.Database.ExecuteSqlRaw($"EXEC @success = ExpoPedidoValidar @cliente = {parms[0].Value}, @usuario = {parms[1].Value}", resultadoParameter);

                rm.Result = Convert.ToInt32(resultadoParameter.Value);
                rm.Response = true;
            }
            catch (Exception e)
            {
                rm.Message = e.Message;
                rm.Response = false;
            }
            return Json(rm);
        }

        [HttpPost]
        public JsonResult GetDetailArticulo(int IdArticulo)
        {
            var dbExpo = new ApplicationDbContextExpo();
            ResponseModel rm = new();
            try
            {
                //List<dArticulos> articulos = dbExpo.dArticulos
                //    .Include(articulo => articulo.mArticulos)
                //    .Where(articulo => articulo.IdArticulo == IdArticulo)
                //    .ToList();

                //if (articulos.Count > 0)
                //{
                //    rm.Result = articulos;
                //    rm.Response = true;
                //}
                //else
                //{
                //    rm.Response = false;
                //    rm.Message = "No hay Promociones Ganadas.";
                //}
            }
            catch (Exception e)
            {
                rm.Message = e.Message;
                rm.Response = false;
            }
            return Json(rm);
        }

        public List<ddlPedidosCliente> GetddlClientes(int IdCliente)
        {
            var dbExpo = new ApplicationDbContextExpo();

            List<ddlPedidosCliente> ddlPedidos = dbExpo.Set<ddlPedidosCliente>()
                .FromSqlRaw("SELECT 0 AS IdPedido, 'TODOS' AS Pedido " +
                "UNION " +
                "SELECT vPedidos.IdPedido, CAST(vPedidos.IdPedido AS NVARCHAR) AS Pedido " +
                "FROM vPedidos " +
                "WHERE vPedidos.Cliente = " + IdCliente + " " +
                "AND vPedidos.Usuario = " + IdCliente + " " +
                "GROUP BY vPedidos.IdPedido").ToList();

            return ddlPedidos;
        }

        public static byte[] GetFotosArticulos(string Tipo, int Id)
        {
            SqlConnection conexion = new(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand command = new("ExpoGetImage", conexion);
            DataTable dataTable = new();
            SqlDataAdapter sqlDA;
            conexion.Open();
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@Type", Tipo));
            command.Parameters.Add(new SqlParameter("@Id", Id));
            command.Connection = conexion;
            sqlDA = new SqlDataAdapter(command);
            sqlDA.Fill(dataTable);
            conexion.Close();

            byte[] fotos = new byte[dataTable.Rows.Count];

            foreach (DataRow row in dataTable.Rows)
            {
                fotos = row.Field<byte[]>(0);
            }
            return fotos;
        }

        public List<TipoDocumentos> GetTipoDocumentos(int IdCliente)
        {
            var dbIndigo = new ApplicationDbContextIndigo();
            List<TipoDocumentos> tipos = dbIndigo.Set<TipoDocumentos>()
                .FromSqlRaw("Select mDocumentos.IdDocumento AS IdTipoDoc, mDocumentos.Documento AS TipoDoc"
                           + " From mDocumentos,"
                           + "  mClientes inner join"
                           + "  mPerEmp on mClientes.IdPersona = mPerEmp.IdPersona inner join"
                           + "  mPersonas on mPerEmp.Persona = mPersonas.Persona"
                           + "  Where mClientes.IdPersona = " + IdCliente
                           + "  And((mDocumentos.IdDocumento = 1"
                           + "  And(UPPER(mPersonas.RFC) like '[A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9][0-9][0-9,A-Z][0-9,A-Z][0-9,A-Z]'"
                           + "  Or UPPER(mPersonas.RFC) like '[A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9][0-9][0-9,A-Z][0-9,A-Z][0-9,A-Z]'))"
                           + "  Or mDocumentos.IdDocumento = 2"
                           + "  Or(mDocumentos.IdDocumento = 4 And mClientes.Traslado = 1)"
                           + "  )"
                           + "  Order by mDocumentos.Documento").ToList();

            tipos.Insert(0, new TipoDocumentos { IdTipoDoc = 0, TipoDoc = "Seleccione... " });

            return tipos;
        }

        public List<UsosCFDI> GetUsoCFDIs(int IdCliente)
        {
            var dbIndigo = new ApplicationDbContextIndigo();
            List<UsosCFDI> usoCFDIs = dbIndigo.Set<UsosCFDI>()
                .FromSqlRaw("SELECT GetUsoCFDI.IdUsoCFDI, GetUsoCFDI.UsoCFDI" +
                " FROM GetUsoCFDI(" + IdCliente + ")").ToList();

            return usoCFDIs;
        }

        public List<Entregas> GetDirecciones(int IdCliente)
        {
            var dbIndigo = new ApplicationDbContextIndigo();
            List<Entregas> entregas = dbIndigo.Set<Entregas>()
                .FromSqlRaw("Select vDirecciones.IdDireccion AS IdEntrega, vDirecciones.Ciudad + ', ' + vDirecciones.Estado + ' - ' + vDirecciones.Direccion AS Entrega"
                          + " From vDirecciones inner join"
                          + " mPerEmp ON vDirecciones.Persona = mPerEmp.Persona"
                          + " Where mPerEmp.IdPersona =" + IdCliente
                          + " Order by Entrega").ToList();

            entregas.Insert(0, new Entregas { IdEntrega = 0, Entrega = "Seleccione una opción... " });

            return entregas;
        }

        public List<Trasportes> GetTrasportes(int IdCliente)
        {
            var dbIndigo = new ApplicationDbContextIndigo();
            List<Trasportes> trasportes = dbIndigo.Set<Trasportes>()
                .FromSqlRaw("Select 0 AS IdTrasportes, 'Seleccione una opción... ' as Transporte"
                            + " UNION"
                            + " Select ISNULL(dTransCliente.IdTransporte,0)IdTrasportes, vPersonas.Nombre AS Transporte"
                            + " From dTransCliente INNER JOIN"
                            + " vPersonas ON dTransCliente.IdTransporte = vPersonas.IdPersona"
                            + " Where dTransCliente.IdPersona =" + IdCliente
                            + " Order by Transporte").ToList();

            return trasportes;
        }

        public List<TiposDeVenta> GetTiposVenta()
        {
            var dbIndigo = new ApplicationDbContextIndigo();
            List<TiposDeVenta> tipoventa = dbIndigo.Set<TiposDeVenta>()
                .FromSqlRaw("Select mTipoVenta.IdTipoVenta, mTipoVenta.TipoVenta"
                           + " From mTipoVenta"
                           + "  Where mTipoVenta.IdTipoVenta != 5"
                           + "  Order by mTipoVenta.TipoVenta").ToList();

            tipoventa.Insert(0, new TiposDeVenta { IdTipoVenta = 0, TipoVenta = "Seleccione una opción... " });

            return tipoventa;
        }
    }
}
