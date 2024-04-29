using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using ExpoMR.Clases;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Vendedor, Capturista")]
    public class EmpleadosController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _rolManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private static IConfiguration _configuration;

        public EmpleadosController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender, RoleManager<IdentityRole> rolManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _rolManager = rolManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _configuration = configuration;
        }
        public IActionResult Cotizacion()
        {
            return View();
        }

        public IActionResult Pedidos()
        {
            return View();
        }
        public IActionResult CotizacionDetail(int IdCliente)
        {
            int IdUsuario = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));

            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@cliente", IdCliente);
            parameters[1] = new SqlParameter("@usuario", IdUsuario);

            dr = dB.ExecuteReader("CotizarDatosCliente", ref parameters);
            Cotizacion cotizacion = new();
            while (dr.Read())
            {
                cotizacion.IdCliente = Convert.ToInt32(dr["IdCliente"] == DBNull.Value ? 0 : dr["IdCliente"]);
                cotizacion.Cliente = Convert.ToString(dr["Cliente"] == DBNull.Value ? "" : dr["Cliente"]);
                cotizacion.IdCotizacion = Convert.ToInt32(dr["IdCotizacion"] == DBNull.Value ? 0 : dr["IdCotizacion"]);
                cotizacion.RFC = Convert.ToString(dr["RFC"] == DBNull.Value ? "" : dr["RFC"]);
                cotizacion.IdDocumento = Convert.ToInt32(dr["IdDocumento"] == DBNull.Value ? 0 : dr["IdDocumento"]);
                cotizacion.Documento = Convert.ToString(dr["Documento"] == DBNull.Value ? "" : dr["Documento"]);
                cotizacion.IdUsoCFDI = Convert.ToInt32(dr["IdUsoCFDI"] == DBNull.Value ? 0 : dr["IdUsoCFDI"]);
                cotizacion.UsoCFDI = Convert.ToString(dr["UsoCFDI"] == DBNull.Value ? "" : dr["UsoCFDI"]);
                cotizacion.IdTipoVenta = Convert.ToInt32(dr["IdTipoVenta"] == DBNull.Value ? 0 : dr["IdTipoVenta"]);
                cotizacion.TipoVenta = Convert.ToString(dr["TipoVenta"] == DBNull.Value ? "" : dr["TipoVenta"]);
                cotizacion.Direccion = Convert.ToInt32(dr["Direccion"] == DBNull.Value ? 0 : dr["Direccion"]);
                cotizacion.DirEntrega = Convert.ToString(dr["DirEntrega"] == DBNull.Value ? "" : dr["DirEntrega"]);
                cotizacion.Observaciones = Convert.ToString(dr["Obsv"] == DBNull.Value ? "" : dr["Obsv"]);
                cotizacion.IdTransporte = Convert.ToInt32(dr["IdTransporte"] == DBNull.Value ? 0 : dr["IdTransporte"]);
                cotizacion.Transportista = Convert.ToString(dr["Transportista"] == DBNull.Value ? "" : dr["Transportista"]);
                cotizacion.IdPorte = Convert.ToInt32(dr["IdPorte"] == DBNull.Value ? 0 : dr["IdPorte"]);
                cotizacion.Porte = Convert.ToString(dr["Porte"] == DBNull.Value ? "" : dr["Porte"]);
                cotizacion.TipoDocumentos = Documentos();
                cotizacion.CFDI = UsosDelCFDI(IdCliente);
                cotizacion.Venta = TipoVenta();
                cotizacion.Direcciones = DirEntrega(IdCliente);
                cotizacion.transportistas = Transportista(IdCliente);
                cotizacion.portes = Porte(IdCliente);
            }

            return View(cotizacion);
        }

        public JsonResult CotizacionEditarCantidadProducto(int IdProducto, int Cantidad, int IdCotizacion)
        {
            ResponseModel rm = new();
            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[3];
                parameters[0] = new SqlParameter("@IdCotizacion", IdCotizacion);
                parameters[1] = new SqlParameter("@Cantidad", Cantidad);
                parameters[2] = new SqlParameter("@IdProducto", IdProducto);

                dB.ExecuteNonQuery("CotizarEditarCantidadProducto", ref parameters);

                rm.Response = true;
                rm.Message = "El producto se agrego al carrito";
            }
            catch (Exception e)
            {
                rm.Message = e.Message;
            }
            return Json(rm);

        }

        public List<TipoDocumento> Documentos() 
        {

            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[0];
            List<TipoDocumento> documentos = new();

            string query = "SELECT mDocumentos.IdDocumento, mDocumentos.Documento " +
                            "FROM mDocumentos " +
                            "WHERE mDocumentos.IdDocumento IN (1,2) --1 = Factura, 2 = Nota";

            dr = dB.ExecuteReader(query, ref parameters, CommandType.Text, "IndigoCS");

            while (dr.Read())
            {
                TipoDocumento GetDocumento = new()
                {
                    IdDocumento = Convert.ToInt16(dr["IdDocumento"] == DBNull.Value ? 0 : dr["IdDocumento"]),
                    Documentos = dr["Documento"] == DBNull.Value ? "" : dr["Documento"].ToString()
                };

                documentos.Add(GetDocumento);                
            }
            return documentos;
        }

        public List<GetUsoCFDI> UsosDelCFDI(int IdCliente) 
        {
            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdCliente", IdCliente);
            List<GetUsoCFDI> UsosDelCFDI = new();

            string query = "SELECT GetUsoCFDI.IdUsoCFDI, GetUsoCFDI.UsoCFDI " +
                            "FROM GetUsoCFDI (@IdCliente) ";

            dr = dB.ExecuteReader(query, ref parameters, CommandType.Text, "IndigoCS");

            while (dr.Read())
            {
                GetUsoCFDI GetCFDI = new()
                {
                    IdUsoCFDI = Convert.ToInt16(dr["IdUsoCFDI"] == DBNull.Value ? 0 : dr["IdUsoCFDI"]),
                    UsoCFDI = dr["UsoCFDI"] == DBNull.Value ? "" : dr["UsoCFDI"].ToString()
                };

                UsosDelCFDI.Add(GetCFDI);
            }

            return UsosDelCFDI;
        }

        public List<GetTipoVenta> TipoVenta()
        {
            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[0];
            List<GetTipoVenta> TiposDeVenta = new();

            string query = "SELECT mTipoVenta.IdTipoVenta, mTipoVenta.TipoVenta " +
                              "FROM mTipoVenta " +
                              "WHERE mTipoVenta.IdTipoVenta != 5 " +
                              "ORDER BY mTipoVenta.TipoVenta ";

            dr = dB.ExecuteReader(query, ref parameters, CommandType.Text, "IndigoCS");

            while (dr.Read())
            {
                GetTipoVenta GetVenta = new()
                {
                    IdTipoVenta = Convert.ToInt16(dr["IdTipoVenta"] == DBNull.Value ? 0 : dr["IdTipoVenta"]),
                    TipoVenta = dr["TipoVenta"] == DBNull.Value ? "" : dr["TipoVenta"].ToString()
                };

                TiposDeVenta.Add(GetVenta);
            }

            return TiposDeVenta;
        }

        public List<GetDireccionEntrega> DirEntrega( int Cliente) 
        {
            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdCliente", Cliente);
            List<GetDireccionEntrega> Entrega = new();

            dr = dB.ExecuteReader("GetDirEntrega", ref parameters);

            while (dr.Read())
            {
                GetDireccionEntrega GetEntrega = new()
                {
                    Entrega = Convert.ToInt16(dr["Entrega"] == DBNull.Value ? 0 : dr["Entrega"]),
                    DirEntrega = dr["DirEntrega"] == DBNull.Value ? "" : dr["DirEntrega"].ToString()
                };

                Entrega.Add(GetEntrega);
            }
            return Entrega;
        }

        public List<GetTransportista> Transportista(int Cliente)
        {
            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@cliente", Cliente);
            List<GetTransportista> transportistas = new();

            string query = "SELECT NULL AS IdTransporte, '' AS Transportista" +
                            " UNION" +
                            " SELECT dTransCliente.IdTransporte, vPersonas.Nombre" +
                            " FROM dTransCliente INNER JOIN" +
                            " vPersonas ON dTransCliente.IdTransporte = vPersonas.IdPersona" +
                            " WHERE dTransCliente.IdPersona = @cliente" +
                            " ORDER BY Transportista";

            dr = dB.ExecuteReader(query, ref parameters, CommandType.Text, "IndigoCS");

            while (dr.Read())
            {
                GetTransportista transportista = new()
                {
                    IdTransporte = Convert.ToInt16(dr["IdTransporte"] == DBNull.Value ? 0 : dr["IdTransporte"]),
                    Transportista = dr["Transportista"] == DBNull.Value ? "" : dr["Transportista"].ToString()
                };

                transportistas.Add(transportista);
            }

            return transportistas;
        }

        public List<GetPorte> Porte(int Cliente)
        {
            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@cliente", Cliente);
            List<GetPorte> portes = new();

            string query = "SELECT NULL AS IdPorte, '' AS Porte" +
                            " UNION" +
                            " SELECT dPorteCliente.IdPorte, mPortes.Porte" +
                            " FROM dPorteCliente INNER JOIN" +
                            " mPortes ON dPorteCliente.IdPorte = mPortes.IdPorte" +
                            " WHERE dPorteCliente.IdPersona = @cliente" +
                            " ORDER BY Porte";

            dr = dB.ExecuteReader(query, ref parameters, CommandType.Text, "IndigoCS");

            while (dr.Read())
            {
                GetPorte porte = new()
                {
                    IdPorte = Convert.ToInt16(dr["IdPorte"] == DBNull.Value ? 0 : dr["IdPorte"]),
                    Porte = dr["Porte"] == DBNull.Value ? "" : dr["Porte"].ToString()
                };

                portes.Add(porte);
            }

            return portes;
        }

        [HttpPost]
        public JsonResult GetClientes() {

            int IdEmpleado = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));

            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdEmpleado", IdEmpleado);

            dr = dB.ExecuteReader("GetClientes", ref parameters);

            var jsonData = new
            {
                rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           acceder = row["IdCliente"],
                           cliente = row["Cliente"]
                       }).ToArray()
            };

            return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult ProductosCapturados( int IdCliente)
        {
            int idUsuario = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));

            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@cliente", IdCliente);
            parameters[1] = new SqlParameter("@usuario", idUsuario);

            dr = dB.ExecuteReader("CotizarDetalleProductos", ref parameters);

            var jsonData = new
            {
                rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           IdProducto = row["IdProducto"],
                           Nombre = row["Nombre"],
                           Presentacion = row["Presentacion"],
                           Laboratorio = row["Laboratorio"],
                           Cantidad = row["Cantidad"],
                           PrecioFinal = row["PrecioFinal"],
                           Total = row["Total"],
                           Editar = true
                       }).ToArray()
            };

            return Json(jsonData.rows);
        }
        [HttpPost]
        public JsonResult ProductoBusqueda(string Busqueda, int IdCotizacion)                               
        {
            ResponseModel rm = new();
            try
            {
                string querySearhTerms = "";
                string[] busqueda = Busqueda.Split(' ');
                int n = 1;
                string[] fields = { "vProductos.Nombre", "vProductos.Presentacion", "vProductos.Formula" };

                foreach (string w in busqueda)
                {
                    querySearhTerms += "And (";
                    foreach (string f in fields)
                    {
                        querySearhTerms += f + " like '%" + w + "%' Or ";
                    }
                    querySearhTerms = querySearhTerms.Substring(0, querySearhTerms.Length - 3) + ") ";
                    n++;
                }
                string query = "SELECT vProductos.IdProducto, vProductos.Nombre, vProductos.Presentacion, vProductos.Formula, mLaboratorios.Laboratorio "
                               + "FROM mLaboratorios INNER JOIN "
                               + "vProductos ON mLaboratorios.IdLaboratorio = vProductos.IdLaboratorio "
                               + querySearhTerms
                               + " AND vProductos.IdProducto NOT IN (SELECT dCotizaciones.IdProducto FROM dCotizaciones WHERE dCotizaciones.IdCotizacion = " + IdCotizacion + " AND dCotizaciones.Solicitado = 1 AND dCotizaciones.PorConfirmar = 1) "
                               + " ORDER BY vProductos.Nombre ASC ";

                DataTable dt = GeneralesController.SqlQueryDT("ExpoMRCS", query);
                var jsonData = new
                {
                    rows = (
                        from col in dt.AsEnumerable()
                        select new
                        {
                           idProducto = col[0],
                           nombre = col[1],
                           presentacion = col[2],
                           formula = col[3] == DBNull.Value ? "" : col[3],
                           laboratorio = col[4]
                           
                        }).ToArray()
              };
                rm.Response = true;
                rm.Message = "ok";
                rm.Result = jsonData.rows;
            }
            catch (Exception e)
            {
                rm.Response = false;
                rm.Message = e.Message;
            }
            return Json(rm.Result);
        }

        [HttpPost]
        public JsonResult DetalleProducto(int IdProducto, int Cliente)
        {
            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@cliente", Cliente);
            parameters[1] = new SqlParameter("@idproducto", IdProducto);

            dr = dB.ExecuteReader("ExpoCotizarVerProducto", ref parameters, CommandType.StoredProcedure, "IndigoCS");

            var jsonData = new
            {
                rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                            imgProducto = GeneralesController.GetFoto(IdProducto),
                            idproducto = row["IdProducto"],
                            nombre = row["Nombre"],
                            presentacion = row["Presentacion"],
                            unidades = row["Unidades"],
                            publico = row["Publico"],
                            neto = row["Neto"],
                            iva = row["Iva"],
                            laboratorio = row["Laboratorio"],
                            caja = row["Caja"],
                            codigo = row["Codigo"],
                            restringido = row["Restringido"],
                            cantidad = row["Cantidad"],
                            formula = row["Formula"],
                            controlado = row["Controlado"],
                            preciomr = row["PrecioMR"],
                            descuentoexpo = row["DescuentoExpo"],
                            descuentocliente = row["DescuentoCliente"],
                            valoriva = row["ValorIva"],
                            preciofinal = row["PrecioFinal"]
                }).ToArray()
            };
            return Json(jsonData.rows[0]);
        }

        [HttpPost]
        public JsonResult ProductoDelete(int IdProducto, int Cliente)
        {
            int idUsuario = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@cliente", Cliente);
            parameters[1] = new SqlParameter("@usuario", idUsuario);
            parameters[2] = new SqlParameter("@IdProducto", IdProducto);

            ResponseModel rm = new();

            try
            {
                dB.ExecuteNonQuery("CotizarEliminarProducto", ref parameters);

                rm.Response = true;
                rm.Message = "producto eliminado";
            }
            catch (Exception e)
            {
                rm.Message = e.Message;
            }
            return Json(rm);
        }

        public JsonResult EditarClienteCotizacion(int IdCotizacion, int IdDocumento, int IdUsoCFDI, int IdTipoVenta, int IdTransportista, int IdPorte, int IdEntrega,  string Obsv )
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[8];
            parameters[0] = new SqlParameter("@IdCotizacion", IdCotizacion);
            parameters[1] = new SqlParameter("@IdDocumento", IdDocumento);
            parameters[2] = new SqlParameter("@IdUsoCFDI", IdUsoCFDI);
            parameters[3] = new SqlParameter("@IdTipoVenta", IdTipoVenta);
            parameters[4] = new SqlParameter("@IdTransporte", IdTransportista);
            parameters[5] = new SqlParameter("@IdPorte", IdPorte);
            parameters[6] = new SqlParameter("@IdEntrega", IdEntrega);
            parameters[7] = new SqlParameter("@Obsv", Obsv == null ? "" : Obsv);

            ResponseModel rm = new();
            try
            {
                dB.ExecuteNonQuery("CotizarEditarCliente", ref parameters);

                rm.Response = true;
                rm.Message = "Datos guardados con exito";
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
            int IdPersona = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[5];
            parameters[0] = new SqlParameter("@cliente", cliente);
            parameters[1] = new SqlParameter("@idproducto", idProducto);
            parameters[2] = new SqlParameter("@unitario", unitario);
            parameters[3] = new SqlParameter("@cantidad", cantidad);
            parameters[4] = new SqlParameter("@usuario", IdPersona);

            ResponseModel rm = new();
            try
            {
                dB.ExecuteNonQuery("CotizarAgregar", ref parameters);

                rm.Response = true;
                rm.Message = "El producto se agrego al carrito";
            }
            catch (Exception e)
            {
                rm.Message = e.Message;
            }
            return Json(rm);
        }

        [HttpPost]
        public JsonResult TotalCotizacion(int IdCliente) 
        {
            int IdUsuario = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));

            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@cliente", IdCliente);
            parameters[1] = new SqlParameter("@usuario", IdUsuario);

            dr = dB.ExecuteReader("CotizarTotalCotizacion", ref parameters);

            if (!dr.HasRows)
            {
                var jsonData = new
                {
                    cliente = IdCliente,
                    productos = 0,
                    subtotal = 0,
                    iva = 0,
                    total = 0,
                    credito = 0
                };
                return Json(jsonData);
            }
            else
            {
                var jsonData = new
                {
                    rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           cliente = row["Cliente"].ToString(),
                           productos = row["Productos"] == DBNull.Value ? 0 : row["Productos"],
                           subtotal = row["Subtotal"] == DBNull.Value ? 0 : row["Subtotal"],
                           iva = row["Iva"] == DBNull.Value ? 0 : row["Iva"],
                           total = row["Total"] == DBNull.Value ? 0 : row["Total"],
                           credito = row["Credito"] == DBNull.Value ? 0 : row["Credito"]
                       }).ToArray()
                };
                return Json(jsonData.rows[0]);
            }
        }

        [HttpPost]
        public JsonResult GetPedidosCliente(int idCliente)
        {
            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdCLiente", idCliente);

            dr = dB.ExecuteReader("PedidosCliente", ref parameters);

            var jsonData = new
            {
                rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           cliente = row["Cliente"].ToString(),
                           idPedido = row["IdPedido"] == DBNull.Value ? 0 : Convert.ToInt32(row["IdPedido"]),
                           documento = row["Documento"] == DBNull.Value ? "" : row["Documento"].ToString(),
                           fecha = row["Fecha"] == DBNull.Value ? "" : row["Fecha"].ToString(),
                           numProds = row["NumProd"] == DBNull.Value ? 0 : Convert.ToInt32(row["NumProd"]),
                           capturo = row["Capturo"] == DBNull.Value ? "" : row["Capturo"].ToString()
                       }).ToArray()
            };
            return Json(jsonData.rows);
        }

        public IActionResult DetallePedido(int idPedido)
        {
            return View(idPedido);
        }

        [HttpPost]
        public JsonResult pedidoCliente(int idPedido)
        {
            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdPedido", idPedido);

            dr = dB.ExecuteReader("PedidoClienteDetalle", ref parameters);

            var jsonData = new
            {
                rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           IdPedido = row["IdPedido"] == DBNull.Value ? 0 : row["IdPedido"],
                           Cliente = row["Cliente"] == DBNull.Value ? "" : row["Cliente"],
                           RFC = row["RFC"] == DBNull.Value ? "" : row["RFC"],
                           Documento = row["Documento"] == DBNull.Value ? "" : row["Documento"],
                           UsoCFDI = row["UsoCFDI"] == DBNull.Value ? "" : row["UsoCFDI"],
                           TipoVenta = row["TipoVenta"] == DBNull.Value ? "" : row["TipoVenta"],
                           DirEntrega = row["DirEntrega"] == DBNull.Value ? "" : row["DirEntrega"],
                           Transportista = row["Transportista"] == DBNull.Value ? "" : row["Transportista"],
                           Obsv = row["Obsv"] == DBNull.Value ? "" : row["Obsv"],
                           Porte = row["Porte"] == DBNull.Value ? "" : row["Porte"],
                           Capturo = row["Capturo"] == DBNull.Value ? "" : row["Capturo"]
                       }).ToArray()
            };
            return Json(jsonData.rows[0]);
        }

        [HttpPost]
        public JsonResult GetProductosPedido(int idPedido)
        {
            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdPedido", idPedido);

            dr = dB.ExecuteReader("PedidoDetalleProductos", ref parameters);

            var jsonData = new
            {
                rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           idProducto = row["IdProducto"] == DBNull.Value ? 0 : row["IdProducto"],
                           nombre = row["Nombre"] == DBNull.Value ? "" : row["Nombre"],
                           presentacion = row["Presentacion"] == DBNull.Value ? "" : row["Presentacion"],
                           laboratorio = row["Laboratorio"] == DBNull.Value ? "" : row["Laboratorio"],
                           cantidad = row["Cantidad"] == DBNull.Value ? 0 : row["Cantidad"],
                           porSurtir = row["PorSurtir"] == DBNull.Value ? 0 : row["PorSurtir"],
                           precioFinal = row["PrecioFinal"] == DBNull.Value ? 0.0 : row["PrecioFinal"],
                           total = row["Total"] == DBNull.Value ? 0.0 : row["Total"]
                       }).ToArray()
            };

            return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult GetTotalPedido(int idPedido)
        {
            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdPedido", idPedido);

            dr = dB.ExecuteReader("PedidoTotalPedido", ref parameters);

            var jsonData = new
            {
                rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           prods = row["Productos"] == DBNull.Value ? 0 : row["Productos"],
                           subtotal = row["Subtotal"] == DBNull.Value ? 0.0 : row["Subtotal"],
                           iva = row["Iva"] == DBNull.Value ? 0.0 : row["Iva"],
                           total = row["Total"] == DBNull.Value ? 0.0 : row["Total"]
                       }).ToArray()
            };
            return Json(jsonData.rows[0]);
        }

        [HttpPost]
        public void ActualizarProducto(int idPedido, int idProducto, int cantidad)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@IdPedido", idPedido);
            parameters[1] = new SqlParameter("@Cantidad", cantidad);
            parameters[2] = new SqlParameter("@IdProducto", idProducto);

            dB.ExecuteNonQuery("PedidoEditarCantidadProducto", ref parameters);
        }

        [HttpPost]
        public void EliminaProducto(int idPedido, int idProducto)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@IdPedido", idPedido);
            parameters[1] = new SqlParameter("@IdProducto", idProducto);

            dB.ExecuteNonQuery("PedidoEliminarProducto", ref parameters);
        }

        [HttpPost]
        public int ColocarOrden(int IdCliente){
            int IdUsuario = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));

            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@cliente", IdCliente);
            parameters[1] = new SqlParameter("@usuario", IdUsuario);

            int success = dB.ExecuteReturnValue("ExpoPedidoValidar", ref parameters);

            return success;
        }

        [HttpPost]

        public void ExpoPedidoAgregar( int IdCliente)
        {
            int IdUsuario = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@cliente", IdCliente);
            parameters[1] = new SqlParameter("@usuario", IdUsuario);
            parameters[2] = new()
            {
                ParameterName = "@Id_Inserted",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            };

            dB.ExecuteNonQuery("ExpoPedidoAgregar", ref parameters);
        }


        public IActionResult Contacto()
        {
            return View();
        }
        public IActionResult Preguntas()
        {
            return View();
        }
        public IActionResult Privacidad()
        {
            return View();
        }
    }
}
