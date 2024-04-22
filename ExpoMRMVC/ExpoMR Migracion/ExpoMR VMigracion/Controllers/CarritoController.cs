using ExpoMR.Clases;
using ExpoMR.Models;
using ExpoMR.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace ExpoMR.Controllers
{
    [Authorize]
    public class CarritoController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _rolManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private static IConfiguration _configuration;

        //Variables para verificar La sesión Y el Usuario
        public CarritoController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender, RoleManager<IdentityRole> rolManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _rolManager = rolManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _rolManager = rolManager;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "MiCuenta");
            }
            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            int IdPersona = GeneralesController.GetIdPersonas(username);
            int cliente = GeneralesController.GetClientes(username);
            string query = "SELECT vCotizacionPorConfirmar.Cliente, vCotizacionPorConfirmar.Productos, vCotizacionPorConfirmar.Subtotal, " +
                           " vCotizacionPorConfirmar.IvaMonto AS Iva, vCotizacionPorConfirmar.Total, dbo.GetCredit("+cliente+", 1) AS Credito " +
                           " FROM vCotizacionPorConfirmar LEFT OUTER JOIN " +
                           " vPedido ON vCotizacionPorConfirmar.Cliente = vPedido.Cliente " +
                           " WHERE vCotizacionPorConfirmar.Cliente =" + cliente +
                           " AND vCotizacionPorConfirmar.Usuario = " + IdPersona;
            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            var CarritoModel = new CarritoViewModel();
            CarritoModel.Carrito = GetCantidadCarrito(IdPersona, cliente);
            CarritoModel.IdUsuario = IdPersona;
            CarritoModel.IdCliente = cliente;
            if (dataTable.Rows.Count > 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    CarritoModel.Productos = Convert.ToInt32(row["Productos"]);
                    CarritoModel.Subtotal = Convert.ToDecimal(row["Subtotal"]);
                    CarritoModel.IVA = Convert.ToDecimal(row["Iva"]);
                    CarritoModel.Total = Convert.ToDecimal(row["Total"]);
                    CarritoModel.Credito = Convert.ToDecimal(row["Credito"]);
                }
                CarritoModel.HasProduct = true;
            }
            else
            {
                CarritoModel.HasProduct = false;
                CarritoModel.Productos = 0;
                CarritoModel.Subtotal = 0;
                CarritoModel.IVA = 0;
                CarritoModel.Total = 0;
                CarritoModel.Credito = 0;
            }
            CarritoModel.IdPedido = 0;
            CarritoModel.PedidosCliente = GetddlClientes(cliente);
            CarritoModel.IdTipoDoc = 0;
            CarritoModel.TipoDocumentos = GetTipoDocumentos(cliente);
            CarritoModel.IdUsoCFDI = 0;
            CarritoModel.UsoCFDI = GetUsoCFDIs(cliente);
            CarritoModel.Entrega = 0;
            CarritoModel.Direccciones = GetDirecciones(cliente);
            CarritoModel.IdTransporte = 0;
            CarritoModel.Trasportes = GetTrasportes(cliente);
            CarritoModel.IdTipoVenta = 0;
            CarritoModel.TiposVenta = GetTiposVenta();

            return View(CarritoModel);
        }

        public static List<ListaCarrito> GetCarrito(int IdCliente, int IdUsuario) {
            var carritoList = new List<ListaCarrito>();

            string query = "SELECT ROW_NUMBER() OVER(PARTITION BY vProductos.IdLaboratorio ORDER BY mProveedores.Nombre, vProductos.Nombre ASC) AS Registro,"+
                            " mLaboratorios.Proveedor, mLaboratorios.Laboratorio, vProductos.IdProducto, vProductos.Nombre, vProductos.Presentacion,"+
                            " vCotizaciones.Cantidad, vCotizaciones.PrecioFinal, vCotizaciones.Total, vCotizaciones.PorConfirmar"+
                            " FROM vCotizaciones INNER JOIN "+
                            " vProductos ON vCotizaciones.IdProducto = vProductos.IdProducto INNER JOIN "+
                            " mLaboratorios ON vProductos.IdLaboratorio = mLaboratorios.IdLaboratorio INNER JOIN "+
                            " mProveedores ON mLaboratorios.Proveedor = mProveedores.IdPersona "+
                            " WHERE vCotizaciones.Cliente = " + IdCliente +
                            " AND vCotizaciones.Usuario = " + IdUsuario +
                            " AND vCotizaciones.Solicitado = 1 "+
                            " ORDER BY mProveedores.Nombre, mLaboratorios.Laboratorio, vProductos.Nombre";
            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            foreach (DataRow row in dataTable.Rows)
            {
                var Carrito = new ListaCarrito();
                Carrito.Registro = Convert.ToInt32(row["Registro"]);
                Carrito.Proveedor = Convert.ToInt32(row["Proveedor"]);
                Carrito.Laboratorio = row["Laboratorio"].ToString();
                Carrito.IdProducto = Convert.ToInt32(row["IdProducto"]);
                Carrito.Nombre = row["Nombre"].ToString();
                Carrito.Presentacion = row["Presentacion"].ToString();
                Carrito.cantidad = Convert.ToInt32(row["Cantidad"]);
                Carrito.PrecioFinal = Convert.ToDecimal(row["PrecioFinal"]);
                Carrito.total = Convert.ToDecimal(row["Total"]);
                Carrito.PorConfirmar = Convert.ToBoolean(row["PorConfirmar"]);
                Carrito.imgProducto = HomeController.GetFotos(Carrito.IdProducto);
                carritoList.Add(Carrito);
            }

            return carritoList;
        }

        public int GetCantidadCarrito(int IdPersona, int cliente)
        {
            try
            {
                DB dB = new DB();
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@cliente", cliente);
                parameters[1] = new SqlParameter("@usuario", IdPersona);
                int ret = (int)dB.ExecuteScalar("ExpoCountProducts", ref parameters);
                return ret;
            }
            catch (Exception ex) { 
            }
            return 0;
        }

        [HttpPost]
        public JsonResult ProductoDelete(int IdProducto, int Cliente)
        {
            ResponseModel rm = new ResponseModel();

            try
            {
                var user = _userManager.GetUserAsync(User);
                string username = user.Result.UserName.ToString();
                int Usuario = GeneralesController.GetIdPersonas(username);

                string cadena = _configuration["ConnectionStrings:ExpoMRCS"];

                SqlConnection cnn = new SqlConnection(cadena);
                SqlCommand cmd = new SqlCommand();
                DataTable dataTable = new DataTable();
                SqlDataAdapter sqlDA;
                cnn.Open();
                cmd.CommandText = "DECLARE @idcotizacion INT " +
                                "EXEC @idcotizacion = dbo.CotizarGetCotizacion " + Cliente + "," + Usuario +
                                "UPDATE dCotizaciones " +
                                "SET Solicitado = 0, " +
                                "PorConfirmar = 0 " +
                                "WHERE dCotizaciones.IdCotizacion = @idcotizacion " +
                                "AND dCotizaciones.IdProducto = " + IdProducto;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = cnn;
                sqlDA = new SqlDataAdapter(cmd);
                sqlDA.Fill(dataTable);
                cnn.Close();

                rm.response = true;
                rm.message = "producto eliminado";
            }
            catch (Exception e)
            {
                rm.message = e.Message;
            }
            return Json(rm);

        }

        [HttpPost]
        public JsonResult AddCotizacion(int cliente, int idProducto, decimal unitario, int cantidad)
        {
            ResponseModel rm = new ResponseModel();
            try
            {
                var user = _userManager.GetUserAsync(User);
                string username = user.Result.UserName.ToString();
                int IdPersona = GeneralesController.GetIdPersonas(username);
                string cadena = _configuration["ConnectionStrings:ExpoMRCS"];

                SqlConnection cnn = new SqlConnection(cadena);
                SqlCommand cmd = new SqlCommand("CotizarAgregar", cnn);
                cnn.Open();
                // 2. set the command object so it knows to execute a stored procedure
                cmd.CommandType = CommandType.StoredProcedure;
                //// 3. add parameter to command, which will be passed to the stored procedure
                cmd.Parameters.Add(new SqlParameter("@cliente", cliente));
                cmd.Parameters.Add(new SqlParameter("@idproducto", idProducto));
                cmd.Parameters.Add(new SqlParameter("@unitario", unitario));
                cmd.Parameters.Add(new SqlParameter("@cantidad", cantidad));
                cmd.Parameters.Add(new SqlParameter("@usuario", IdPersona));
                cmd.Connection = cnn;
                SqlDataReader dr = cmd.ExecuteReader();
                cnn.Close();

                rm.response = true;
                rm.message = "El producto se agrego al carrito";
                rm.result = GetCantidadCarrito(IdPersona, cliente);
            }
            catch (Exception e)
            {
                rm.message = e.Message;
            }
            return Json(rm);
        }

        [HttpPost]
        public JsonResult GetPedidosCarrito(int IdCliente, int IdPedido = 0)
        {
            ResponseModel rm = new ResponseModel();
            try {
                string query = "SELECT ROW_NUMBER() OVER(PARTITION BY vPedidos.IdPedido,vProductos.IdLaboratorio " +
                   "ORDER BY mProveedores.Nombre, vProductos.Nombre ASC) AS RegistroProveedor, mLaboratorios.Proveedor, " +
                   "ROW_NUMBER() OVER(PARTITION BY vProductos.IdLaboratorio, vPedidos.IdPedido ORDER BY mProveedores.Nombre, " +
                   "vProductos.Nombre ASC) AS RegistroPedido, vPedidos.IdPedido, vProductos.IdProducto, vProductos.Nombre, " +
                   "vProductos.Presentacion, vPedidos.Cantidad, vPedidos.PrecioFinal, vPedidos.Total, mLaboratorios.Laboratorio " +
                   "FROM vPedidos INNER JOIN " +
                   "vProductos ON vPedidos.IdProducto = vProductos.IdProducto INNER JOIN " +
                   "mLaboratorios ON vProductos.IdLaboratorio = mLaboratorios.IdLaboratorio INNER JOIN " +
                   "mProveedores ON mLaboratorios.Proveedor = mProveedores.IdPersona " +
                   "WHERE vPedidos.Cliente = " + IdCliente +
                   " AND vPedidos.Usuario = " + IdCliente +
                   " AND (vPedidos.IdPedido = " + IdPedido + " or " + IdPedido + " = 0) " +
                   "ORDER BY vPedidos.IdPedido, mProveedores.Nombre, mLaboratorios.Laboratorio, vProductos.Nombre";
                DataTable dt = GeneralesController.sqlQueryDT("ExpoMRCS", query);

                if (dt.Rows.Count > 0) {
                    var jsonData = new
                    {
                        rows = (
                           from c in dt.AsEnumerable()
                           select new
                           {
                               RegistroProveedor = c[0],
                               Proveedor = c[1],
                               RegistroPedido = c[2],
                               IdPedido = c[3],
                               IdProducto = c[4],
                               Nombre = c[5],
                               Presentacion = c[6],
                               Cantidad = c[7],
                               PrecioFinal = c[8],
                               Total = c[9],
                               Laboratorio = c[10]
                           }).ToArray()
                    };
                    rm.result = jsonData.rows;
                    rm.response = true;
                }
                else
                {
                    rm.response = false;
                }
                    
            } catch (Exception e) {
                rm.message = e.Message;
                rm.response = false;
            }

            return Json(rm);
        }

        [HttpPost]
        public JsonResult GetDatosFacturacion(int IdCliente)
        {
            ResponseModel rm = new ResponseModel();
            try {
                string query = "DECLARE @idcotizacion INT" +
                    " EXEC @idcotizacion = dbo.CotizarGetCotizacion "+IdCliente+ ", "+IdCliente+" " +
                    "SELECT vCotizacionDatos.IdCotizacion, vCotizacionDatos.Cliente, vCotizacionDatos.RFC, vCotizacionDatos.IdDocumento, " +
                    "vCotizacionDatos.Documento, ISNULL(vCotizacionDatos.IdUsoCFDI, 0) AS IdUsoCFDI, vCotizacionDatos.UsoCFDI " +
                    "FROM vCotizacionDatos WHERE vCotizacionDatos.IdCotizacion = @idcotizacion";
                DataTable dt = GeneralesController.sqlQueryDT("ExpoMRCS", query);

                if (dt.Rows.Count > 0)
                {
                    var jsonData = new
                    {
                        rows = (
                          from c in dt.AsEnumerable()
                          select new
                          {
                              IdCotizacion = c[0],
                              Cliente = c[1],
                              RFC = c[2],
                              IdDocumento = c[3],
                              Documento = c[4],
                              IdUsoCFDI = c[5],
                              UsoCFDI = c[6]
                          }).ToArray()
                    };
                    rm.result = jsonData.rows;
                    rm.response = true;
                }
                else {
                    rm.response = false;
                    rm.message = "No hay Promociones Ganadas.";
                }               
            } catch (Exception e) {
                rm.message = e.Message;
                rm.response = false;
            }
            return Json(rm);
        }
        [HttpPost]
        public JsonResult GetDatosTransporte(int IdCliente)
        {
            ResponseModel rm = new ResponseModel();
            try {
                string query = "DECLARE @idcotizacion INT EXEC @idcotizacion = dbo.CotizarGetCotizacion " + IdCliente + ", " + IdCliente + " " +
                    " SELECT vCotizacionDatos.IdCotizacion, vCotizacionDatos.IdTipoVenta, vCotizacionDatos.TipoVenta," +
                    " vCotizacionDatos.Entrega, vCotizacionDatos.DirEntrega, vCotizacionDatos.IdTransporte, vCotizacionDatos.Transportista," +
                    " vCotizacionDatos.IdPorte, vCotizacionDatos.Porte, vCotizacionDatos.Obsv " +
                    " FROM vCotizacionDatos" +
                    " WHERE vCotizacionDatos.IdCotizacion = @idcotizacion";

                DataTable dt = GeneralesController.sqlQueryDT("ExpoMRCS", query);

                if (dt.Rows.Count > 0)
                {
                    var jsonData = new
                    {
                        rows = (
                          from c in dt.AsEnumerable()
                          select new
                          {
                              IdCotizacion = c[0],
                              IdTipoVenta = c[1],
                              TipoVenta = c[2],
                              Entrega = c[3],
                              DirEntrega = c[4],
                              IdTransporte = c[5],
                              Transportista = c[6],
                              IdPorte = c[7],
                              Porte = c[8],
                              Obsv = c[9]
                          }).ToArray()
                    };
                    rm.result = jsonData.rows;
                    rm.response = true;
                }
                else {
                    rm.response = false;
                    rm.message = "No hay Promociones Ganadas.";
                }               
            } catch (Exception e) {
                rm.message = e.Message;
                rm.response = false;
            }
            return Json(rm);
        }
        [HttpPost]
        public JsonResult GetSumary(int IdCliente)
        {
            ResponseModel rm = new ResponseModel();
            try {
                string query = "DECLARE @idcotizacion INT EXEC @idcotizacion = dbo.CotizarGetCotizacion " + IdCliente + ", " + IdCliente + " " +
                    " SELECT vCotizacionDatos.IdCotizacion, vCotizacionDatos.Cliente, vCotizacionDatos.RFC, vCotizacionDatos.Documento," +
                    " vCotizacionDatos.UsoCFDI, vCotizacionDatos.DirEntrega, vCotizacionDatos.TipoVenta, vCotizacionDatos.Transportista," +
                    " vCotizacionDatos.Porte, vCotizacionDatos.Obsv " +
                    " FROM vCotizacionDatos" +
                    " WHERE vCotizacionDatos.IdCotizacion = @idcotizacion";

                DataTable dt = GeneralesController.sqlQueryDT("ExpoMRCS", query);

                if (dt.Rows.Count > 0)
                {
                    var jsonData = new
                    {
                        rows = (
                          from c in dt.AsEnumerable()
                          select new
                          {
                              Cliente = c[1],
                              RFC = c[2],
                              Documento = c[3],
                              UsoCFDI = c[4],
                              DirEntrega = c[5],
                              TipoVenta = c[6],
                              Transportista = c[7],
                              Porte = c[8],
                              Obsv = c[9]
                          }).ToArray()
                    };
                    rm.result = jsonData.rows;
                    rm.response = true;
                }
                else {
                    rm.response = false;
                    rm.message = "No hay Promociones Ganadas.";
                }               
            } catch (Exception e) {
                rm.message = e.Message;
                rm.response = false;
            }
            return Json(rm);
        }
        
        [HttpPost]
        public JsonResult ExpoPedidoAgregar()
        {
            ResponseModel rm = new ResponseModel();
            try {
                var user = _userManager.GetUserAsync(User);
                string username = user.Result.UserName.ToString();
                int IdPersona = GeneralesController.GetIdPersonas(username);
                int cliente = GeneralesController.GetClientes(username);

                SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
                SqlCommand cmd = new SqlCommand("ExpoPedidoAgregar", cnn);
                DataTable dataTable = new DataTable();
                SqlDataAdapter sqlDA;
                cnn.Open();

                // 2. set the command object so it knows to execute a stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                //// 3. add parameter to command, which will be passed to the stored procedure
                cmd.Parameters.Add(new SqlParameter("@cliente", cliente));
                cmd.Parameters.Add(new SqlParameter("@usuario", IdPersona));
                SqlParameter Id_Inserted = new SqlParameter();
                Id_Inserted.ParameterName = "@Id_Inserted";
                Id_Inserted.DbType = DbType.Int32;
                Id_Inserted.Direction = ParameterDirection.Output;                

                cmd.Parameters.Add(Id_Inserted);
                cmd.Connection = cnn;
                sqlDA = new SqlDataAdapter(cmd);
                sqlDA.Fill(dataTable);
                cnn.Close();

                rm.response = true;
                rm.message = "El pedido ha sido colocado correctamente.";
            } catch (Exception e) {
                rm.message = e.Message;
                rm.response = false;
            }


            return Json(rm);
        }

        [HttpPost]
        public JsonResult GetPremios(int IdCliente)
        {
            ResponseModel rm = new ResponseModel();
            try {
                string query = "SELECT dPromociones.IdArticulo, mProveedores.Nombre AS Proveedor, "
                                +" SUM(dPromociones.Cantidad * dPromoCliente.Cantidad) As Cantidad, mArticulos.Nombre"
                                +" FROM dPromoCliente INNER JOIN"
                                +" dPromociones ON dPromoCliente.IdPromocion = dPromociones.IdPromocion INNER JOIN"
                                +" mPromociones ON dPromoCliente.IdPromocion = mPromociones.IdPromocion INNER JOIN"
                                +" mArticulos ON dPromociones.IdArticulo = mArticulos.IdArticulo INNER JOIN"
                                +" mProveedores ON mArticulos.Proveedor = mProveedores.IdPersona"
                                + " WHERE dPromoCliente.Cliente =" + IdCliente
                                + " GROUP BY dPromociones.IdArticulo, mProveedores.Nombre,mArticulos.Nombre";
                DataTable dt = GeneralesController.sqlQueryDT("ExpoMRCS", query);

                if (dt.Rows.Count > 0)
                {
                    var jsonData = new
                    {
                        rows = (
                          from c in dt.AsEnumerable()
                          select new
                          {
                              idArticulo = c[0],
                              proveedor = c[1],
                              cantidad = c[2],
                              Nombre = c[3]
                          }).ToArray()
                    };
                    rm.result = jsonData.rows;
                    rm.response = true;
                }
                else {
                    rm.response = false;
                    rm.message = "No hay Promociones Ganadas.";
                }               
            } catch (Exception e) {
                rm.message = e.Message;
                rm.response = false;
            }
            return Json(rm);
        }
        [HttpPost]
        public JsonResult Validarcolocar(int IdCliente)
        {
            ResponseModel rm = new ResponseModel();
            try {
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@cliente", IdCliente);
                parameters[1] = new SqlParameter("@usuario", IdCliente);
                DB dB = new DB();

                int ret = Convert.ToInt32(dB.ExecuteScalar("ExpoPedidoValidar", ref parameters));
                rm.result = ret;
                rm.response = true;
                              
            } catch (Exception e) {
                rm.message = e.Message;
                rm.response = false;
            }
            return Json(rm);
        }
        [HttpPost]
        public JsonResult GetDetailArticulo(int IdArticulo)
        {
            ResponseModel rm = new ResponseModel();
            try {
                string query = "SELECT dArticulos.IDdArticulos, mArticulos.Descripcion " +
                    "FROM dArticulos INNER JOIN " +
                    "mArticulos ON dArticulos.IdArticulo = mArticulos.IdArticulo " +
                    "WHERE dArticulos.IdArticulo = " + IdArticulo;
                DataTable dt = GeneralesController.sqlQueryDT("ExpoMRCS", query);

                if (dt.Rows.Count > 0)
                {
                    var jsonData = new
                    {
                        rows = (
                          from c in dt.AsEnumerable()
                          select new
                          {
                              imgArticulo = GetFotosArticulos("item", Convert.ToInt16(c[0])),
                              descripcion = c[1]
                          }).ToArray()
                    };
                    rm.result = jsonData.rows;
                    rm.response = true;
                }
                else {
                    rm.response = false;
                    rm.message = "No hay Promociones Ganadas.";
                }               
            } catch (Exception e) {
                rm.message = e.Message;
                rm.response = false;
            }
            return Json(rm);
        }

        public List<ddlPedidosCliente> GetddlClientes(int IdCliente) {
            string query = "SELECT 0 AS IdPedido, 'TODOS' AS Pedido " +
                "UNION " +
                "SELECT vPedidos.IdPedido, CAST(vPedidos.IdPedido AS NVARCHAR) AS Pedido " +
                "FROM vPedidos " +
                "WHERE vPedidos.Cliente = "+ IdCliente + " " +
                "AND vPedidos.Usuario = "+ IdCliente + " " +
                "GROUP BY vPedidos.IdPedido";
            DataTable Dt = GeneralesController.sqlQueryDT("ExpoMRCS", query);
            var ListaPedidos = new List<ddlPedidosCliente>();

            foreach (DataRow row in Dt.Rows)
            {
                var pedido = new ddlPedidosCliente();
                pedido.IdPedido = Convert.ToInt32(row["IdPedido"]);
                pedido.Pedido = row["Pedido"].ToString();
                ListaPedidos.Add(pedido);
            }

            return ListaPedidos;
        }

        public static byte[] GetFotosArticulos(string Tipo, int Id)
        {
            SqlConnection conexion = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand command = new SqlCommand("ExpoGetImage", conexion);
            DataTable dataTable = new DataTable();
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
            string query = "Select mDocumentos.IdDocumento, mDocumentos.Documento"
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
                           + "  Order by mDocumentos.Documento";

            DataTable dt = GeneralesController.sqlQueryDT("IndigoCS", query);
            List<TipoDocumentos> td = new();
            td.Add(new TipoDocumentos { IdTipoDoc = 0, TipoDoc = "Seleccione... " });

            foreach (DataRow dr in dt.Rows)
            {
                TipoDocumentos tipoDocumentos = new();
                tipoDocumentos.IdTipoDoc = Convert.ToInt16(dr["IdDocumento"]);
                tipoDocumentos.TipoDoc = dr["Documento"].ToString();
                td.Add(tipoDocumentos);
            }

            return td;
        }

        public List<UsoCFDI> GetUsoCFDIs(int IdCliente)
        {
            List<UsoCFDI> usoCFDIs = new();
            string query = "SELECT GetUsoCFDI.IdUsoCFDI, GetUsoCFDI.UsoCFDI" +
                " FROM GetUsoCFDI(" + IdCliente + ")";

            DataTable dt = GeneralesController.sqlQueryDT("IndigoCS", query);


            //usoCFDIs.Add(new UsoCFDI { IdUsoCFDI = 0, UsosCFDI = "Seleccione una opción... " });
            foreach (DataRow dr in dt.Rows)
            {
                UsoCFDI CFDI = new();
                CFDI.IdUsoCFDI = Convert.ToInt16(dr["IdUsoCFDI"]);
                CFDI.UsosCFDI = dr["UsoCFDI"].ToString();
                usoCFDIs.Add(CFDI);
            }

            return usoCFDIs;
        }

        public List<Entregas> GetDirecciones(int IdCliente)
        {
            List<Entregas> Entregas = new();
            Entregas.Add(new Entregas { IdEntrega = 0, Entrega = "Seleccione una opción... " });
            string query = "Select vDirecciones.IdDireccion AS Entrega, vDirecciones.Ciudad + ', ' + vDirecciones.Estado + ' - ' + vDirecciones.Direccion AS DireccionE"
                          + " From vDirecciones inner join"
                          + " mPerEmp ON vDirecciones.Persona = mPerEmp.Persona"
                          + " Where mPerEmp.IdPersona =" + IdCliente
                          + " Order by DireccionE";

            DataTable dt = GeneralesController.sqlQueryDT("IndigoCS", query);
            foreach (DataRow dr in dt.Rows)
            {
                Entregas Entrega = new();
                Entrega.IdEntrega = Convert.ToInt16(dr["Entrega"]);
                Entrega.Entrega = dr["DireccionE"].ToString();
                Entregas.Add(Entrega);
            }

            return Entregas;
        }

        public List<Trasportes> GetTrasportes(int IdCliente)
        {
            List<Trasportes> Trasportes = new();
            //Trasportes.Add(new Trasportes { IdTrasportes = 0, Transporte = "Seleccione una opción... " });
            string query = "Select 0 AS IdPersona, 'Seleccione una opción... ' as Nombre"
                            + " UNION"
                            + " Select ISNULL(dTransCliente.IdTransporte,0)IdTransporte, vPersonas.Nombre"
                            + " From dTransCliente INNER JOIN"
                            + " vPersonas ON dTransCliente.IdTransporte = vPersonas.IdPersona"
                            + " Where dTransCliente.IdPersona =" + IdCliente
                            + " Order by Nombre";

            DataTable dt = GeneralesController.sqlQueryDT("IndigoCS", query);
            foreach (DataRow dr in dt.Rows)
            {
                Trasportes Trasporte = new();
                Trasporte.IdTrasportes = Convert.ToInt16(dr["IdPersona"]);
                Trasporte.Transporte = dr["Nombre"].ToString();
                Trasportes.Add(Trasporte);
            }

            return Trasportes;

        }

        public List<TiposDeVenta> GetTiposVenta()
        {
            List<TiposDeVenta> tipoventa = new();
            tipoventa.Add(new TiposDeVenta { IdTipoVenta = 0, TipoVenta = "Seleccione una opción... " });
            string query = "Select mTipoVenta.IdTipoVenta, mTipoVenta.TipoVenta"
                           + " From mTipoVenta"
                           + "  Where mTipoVenta.IdTipoVenta != 5"
                           + "  Order by mTipoVenta.TipoVenta";

            DataTable dt = GeneralesController.sqlQueryDT("IndigoCS", query);
            foreach (DataRow dr in dt.Rows)
            {
                TiposDeVenta tipo = new();
                tipo.IdTipoVenta = Convert.ToInt16(dr["IdTipoVenta"]);
                tipo.TipoVenta = dr["TipoVenta"].ToString();
                tipoventa.Add(tipo);
            }

            return tipoventa;
        }
    }
}
