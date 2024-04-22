using ExpoMR.Clases;
using ExpoMR.Datos;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using MimeKit.Cryptography;
using NuGet.Protocol.Core.Types;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Data;
using System.Diagnostics.Metrics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Vendedor")]
    public class EmpleadosController : Controller
    {
        //private readonly string _db;

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
            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            int IdUsuario = GeneralesController.GetIdPersonas(username);

            string cadena = _configuration["ConnectionStrings:ExpoMRCS"];

            SqlConnection cnn = new SqlConnection(cadena);
            SqlCommand cmd = new SqlCommand();
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA;
            cnn.Open();
            cmd.CommandText = "DECLARE @idcotizacion INT " +
                            " EXEC @idcotizacion = dbo.CotizarGetCotizacion " + IdCliente + "," + IdUsuario +
                            " SELECT vCotizacionDatos.IdPersona AS IdCliente, vCotizacionDatos.IdCotizacion, vCotizacionDatos.Cliente, vCotizacionDatos.RFC, " +
                            " vCotizacionDatos.IdDocumento, vCotizacionDatos.Documento, vCotizacionDatos.IdUsoCFDI, vCotizacionDatos.UsoCFDI, " +
                            " vCotizacionDatos.Entrega as Direccion, vCotizacionDatos.DirEntrega, vCotizacionDatos.IdTipoVenta, vCotizacionDatos.TipoVenta, " +
                            " vCotizacionDatos.IdTransporte, vCotizacionDatos.Transportista,vCotizacionDatos.Obsv, vCotizacionDatos.IdPorte, vCotizacionDatos.Porte " +
                            " FROM vCotizacionDatos " +
                            " WHERE vCotizacionDatos.IdCotizacion = @idcotizacion";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dataTable);
            
            Cotizacion cotizacion = new Cotizacion();

            var dr = cmd.ExecuteReader();
            while (dr.Read())
            {

                cotizacion.IdCliente = Convert.ToInt32(dr["IdCliente"]);
                cotizacion.Cliente = Convert.ToString(dr["Cliente"]);
                cotizacion.IdCotizacion = Convert.ToInt32(dr["IdCotizacion"]);
                cotizacion.RFC = Convert.ToString(dr["RFC"]);
                cotizacion.IdDocumento = Convert.ToInt32(dr["IdDocumento"]);
                cotizacion.Documento = Convert.ToString(dr["Documento"]);
                cotizacion.IdUsoCFDI = Convert.ToInt32(dr["IdUsoCFDI"]);
                cotizacion.UsoCFDI = Convert.ToString(dr["UsoCFDI"]);
                cotizacion.IdTipoVenta = Convert.ToInt32(dr["IdTipoVenta"]);
                cotizacion.TipoVenta = Convert.ToString(dr["TipoVenta"]);
                cotizacion.Direccion = Convert.ToInt32(dr["Direccion"]);
                cotizacion.DirEntrega = Convert.ToString(dr["DirEntrega"]);
                cotizacion.Observaciones = Convert.ToString(dr["Obsv"]);
                cotizacion.Transportista = Convert.ToString(dr["Transportista"]);
                cotizacion.Porte = Convert.ToString(dr["Porte"]);
                cotizacion.TipoDocumentos = documentos();
                cotizacion.CFDI = UsosDelCFDI(IdCliente);
                cotizacion.Venta = TipoVenta();
                cotizacion.Direcciones = DirEntrega(IdCliente);

            }
            cnn.Close();
            return View(cotizacion);
        }

        public JsonResult CotizacionEditarCantidadProducto( int IdCliente, int IdProducto, int Cantidad, int IdCotizacion)
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
                cmd.CommandText ="DECLARE @idcotizacion INT " +
                                "EXEC @idcotizacion = dbo.CotizarGetCotizacion " + IdCliente + "," + Usuario +
                                "UPDATE dCotizaciones " +
                                "SET Cantidad = CASE WHEN (" + Cantidad + " = 0 OR " + Cantidad + " IS NULL) THEN Cantidad ELSE " + Cantidad + " END " +
                                "WHERE dCotizaciones.IdCotizacion = @idcotizacion " +
                                "AND dCotizaciones.IdProducto = " + IdProducto;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = cnn;
                sqlDA = new SqlDataAdapter(cmd);
                sqlDA.Fill(dataTable);
                cnn.Close();

                rm.response = true;
                rm.message = "El producto se agrego al carrito";


            }
            catch (Exception e)
            {
                rm.message = e.Message;
            }
            return Json(rm);

        }

        public List<TipoDocumento> documentos() 
        {
            List<TipoDocumento> documentos = new List<TipoDocumento>();

            //string cadena = _configuration["ConnectionStrings:IndigoCS"];

            //SqlConnection cnn = new SqlConnection(cadena);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();

            //cmd.CommandText = "SELECT mDocumentos.IdDocumento, mDocumentos.Documento " +
            //                "FROM mDocumentos " +
            //                "WHERE mDocumentos.IdDocumento IN (1,2) --1 = Factura, 2 = Nota";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            string query = "SELECT mDocumentos.IdDocumento, mDocumentos.Documento " +
                            "FROM mDocumentos " +
                            "WHERE mDocumentos.IdDocumento IN (1,2) --1 = Factura, 2 = Nota";

            DataTable dataTable = GeneralesController.sqlQueryDT("IndigoCS", query);

            foreach (DataRow row in dataTable.Rows)
            {
                TipoDocumento GetDocumento = new TipoDocumento();
                GetDocumento.IdDocumento = Convert.ToInt16(row.Field<byte>(0));
                GetDocumento.Documentos = row.Field<string>(1);
                documentos.Add(GetDocumento);
            }

            return documentos;
        }

        public List<GetUsoCFDI> UsosDelCFDI(int IdCliente) 
        {
            List<GetUsoCFDI> UsosDelCFDI = new List<GetUsoCFDI>();

            //string cadena = _configuration["ConnectionStrings:IndigoCS"];

            //SqlConnection cnn = new SqlConnection(cadena);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();

            //cmd.CommandText = "SELECT GetUsoCFDI.IdUsoCFDI, GetUsoCFDI.UsoCFDI " +
            //                "FROM GetUsoCFDI ("+ IdCliente + " ) ";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);


            string query = "SELECT GetUsoCFDI.IdUsoCFDI, GetUsoCFDI.UsoCFDI " +
                            "FROM GetUsoCFDI (" + IdCliente + " ) ";

            DataTable dataTable = GeneralesController.sqlQueryDT("IndigoCS", query);
            foreach (DataRow row in dataTable.Rows)
            {
                GetUsoCFDI GetCFDI = new GetUsoCFDI();
                GetCFDI.IdUsoCFDI = Convert.ToInt16(row.Field<byte>(0));
                GetCFDI.UsoCFDI = row.Field<string>(1);
                UsosDelCFDI.Add(GetCFDI);
            }

            return UsosDelCFDI;

        }

        public List<GetTipoVenta> TipoVenta()
        {

            List<GetTipoVenta> TiposDeVenta = new List<GetTipoVenta>();

            //string cadena = _configuration["ConnectionStrings:IndigoCS"];

            //SqlConnection cnn = new SqlConnection(cadena);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();

            //cmd.CommandText = "SELECT mTipoVenta.IdTipoVenta, mTipoVenta.TipoVenta " +
            //                  "FROM mTipoVenta " +
            //                  "WHERE mTipoVenta.IdTipoVenta != 5 " +
            //                  "ORDER BY mTipoVenta.TipoVenta ";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            string query = "SELECT mTipoVenta.IdTipoVenta, mTipoVenta.TipoVenta " +
                              "FROM mTipoVenta " +
                              "WHERE mTipoVenta.IdTipoVenta != 5 " +
                              "ORDER BY mTipoVenta.TipoVenta ";

            DataTable dataTable = GeneralesController.sqlQueryDT("IndigoCS", query);

            foreach (DataRow row in dataTable.Rows)
            {
                GetTipoVenta GetVenta = new GetTipoVenta();
                GetVenta.IdTipoVenta = Convert.ToInt16(row.Field<byte>(0));
                GetVenta.TipoVenta = row.Field<string>(1);
                TiposDeVenta.Add(GetVenta);
            }

            return TiposDeVenta;

        }

        public List<GetDireccionEntrega> DirEntrega( int Cliente) 
        {

            List<GetDireccionEntrega> Entrega = new List<GetDireccionEntrega>();

            //string cadena = _configuration["ConnectionStrings:IndigoCS"];

            //SqlConnection cnn = new SqlConnection(cadena);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();

            //cmd.CommandText = "SELECT Direcciones.IdDireccion AS Entrega, dbo.FormatAddress(Direcciones.CalleNum, Direcciones.Colonia, Direcciones.Ciudad, Direcciones.Estado, Direcciones.CP) AS DirEntrega " +
            //                "FROM Indigo.dbo.mPerEmp AS Personas INNER JOIN " +
            //                "Indigo.dbo.vDirecciones AS Direcciones ON Personas.Persona = Direcciones.Persona " +
            //                "WHERE Personas.IdPersona = " + Cliente +
            //                "--AND Direcciones.Status = 1 -- 1-= Activa " +
            //                "ORDER BY DirEntrega";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);

            string query = "SELECT Direcciones.IdDireccion AS Entrega, dbo.FormatAddress(Direcciones.CalleNum, Direcciones.Colonia, Direcciones.Ciudad, Direcciones.Estado, Direcciones.CP) AS DirEntrega " +
                            "FROM Indigo.dbo.mPerEmp AS Personas INNER JOIN " +
                            "Indigo.dbo.vDirecciones AS Direcciones ON Personas.Persona = Direcciones.Persona " +
                            "WHERE Personas.IdPersona = " + Cliente +
                            "--AND Direcciones.Status = 1 -- 1-= Activa " +
                            "ORDER BY DirEntrega";

            DataTable dataTable = GeneralesController.sqlQueryDT("IndigoCS", query);
            foreach (DataRow row in dataTable.Rows)
            {
                GetDireccionEntrega GetEntrega = new GetDireccionEntrega();
                GetEntrega.Entrega = row.Field<Int32>(0);
                GetEntrega.DirEntrega = row.Field<string>(1);
                Entrega.Add(GetEntrega);
            }

            return Entrega;

        }

        [HttpPost]
        public JsonResult GetPedidos() {
            //SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "SELECT vClientesRutas.IdCliente, vClientesRutas.Cliente"
            //                   + " FROM vClientesRutas"
            //                    + " ORDER BY vClientesRutas.Cliente";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();
            string query = "SELECT vClientesRutas.IdCliente, vClientesRutas.Cliente"
                            + " FROM vClientesRutas"
                            + " ORDER BY vClientesRutas.Cliente";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           acceder = col[0],
                           cliente = col[1]
                       }).ToArray()
            };

            return Json(jsonData.rows);
        }
        [HttpPost]
        public JsonResult GetClientes() {

            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            int IdPersona = GeneralesController.GetIdPersonas(username);
            
            //string cadena = _configuration["ConnectionStrings:ExpoMRCS"];

            //SqlConnection cnn = new SqlConnection(cadena);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "IF EXISTS(SELECT vClientesRutas.IdRuta " +
            //    "FROM vClientesRutas WHERE vClientesRutas.IdEmpleado = " + IdPersona + " ) " +
            //    "BEGIN " +
            //    "   SELECT vClientesRutas.IdCliente, vClientesRutas.Cliente" +
            //    "   FROM vClientesRutas" +
            //    "   WHERE vClientesRutas.IdEmpleado = "+ IdPersona + " " +
            //    "   ORDER BY vClientesRutas.Cliente " +
            //    "END " +
            //    "ELSE " +
            //    "BEGIN" +
            //    "   SELECT vClientesSucursales.IdCliente, vClientesSucursales.Cliente" +
            //    "   FROM vClientesSucursales" +
            //    "   ORDER BY vClientesSucursales.Cliente " +
            //    "END";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();

            string query = "IF EXISTS(SELECT vClientesRutas.IdRuta " +
                "FROM vClientesRutas WHERE vClientesRutas.IdEmpleado = " + IdPersona + " ) " +
                "BEGIN " +
                "   SELECT vClientesRutas.IdCliente, vClientesRutas.Cliente" +
                "   FROM vClientesRutas" +
                "   WHERE vClientesRutas.IdEmpleado = " + IdPersona + " " +
                "   ORDER BY vClientesRutas.Cliente " +
                "END " +
                "ELSE " +
                "BEGIN" +
                "   SELECT vClientesSucursales.IdCliente, vClientesSucursales.Cliente" +
                "   FROM vClientesSucursales" +
                "   ORDER BY vClientesSucursales.Cliente " +
                "END";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           acceder = col[0],
                           cliente = col[1]
                       }).ToArray()
            };

            return Json(jsonData.rows);
        }
        [HttpPost]
        public JsonResult ProductosCapturados( int IdCliente)
        {
            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            int Usuario = GeneralesController.GetIdPersonas(username);

            //string cadena = _configuration["ConnectionStrings:ExpoMRCS"];

            //SqlConnection cnn = new SqlConnection(cadena);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "SELECT vProductos.IdProducto, vProductos.Nombre, vProductos.Presentacion, mLaboratorios.Laboratorio, " +
            //    "vCotizaciones.Cantidad, vCotizaciones.PrecioFinal, vCotizaciones.Total, vCotizaciones.PorConfirmar " +
            //    "FROM vCotizaciones INNER JOIN " +
            //    "vProductos ON vCotizaciones.IdProducto = vProductos.IdProducto INNER JOIN " +
            //    "mLaboratorios ON vProductos.IdLaboratorio = mLaboratorios.IdLaboratorio " +
            //    "WHERE vCotizaciones.Cliente = " + IdCliente +
            //    "AND vCotizaciones.Usuario = " + Usuario +
            //    "AND vCotizaciones.Solicitado = 1 " +
            //    "ORDER BY mLaboratorios.Laboratorio, vProductos.Nombre";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();

            string query = "SELECT vProductos.IdProducto, vProductos.Nombre, vProductos.Presentacion, mLaboratorios.Laboratorio, " +
                " vCotizaciones.Cantidad, vCotizaciones.PrecioFinal, vCotizaciones.Total, vCotizaciones.PorConfirmar " +
                " FROM vCotizaciones INNER JOIN " +
                " vProductos ON vCotizaciones.IdProducto = vProductos.IdProducto INNER JOIN " +
                " mLaboratorios ON vProductos.IdLaboratorio = mLaboratorios.IdLaboratorio " +
                " WHERE vCotizaciones.Cliente = " + IdCliente +
                " AND vCotizaciones.Usuario = " + Usuario +
                " AND vCotizaciones.Solicitado = 1 " +
                " ORDER BY mLaboratorios.Laboratorio, vProductos.Nombre";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           IdProducto = col[0],
                           Nombre = col[1],
                           Presentacion = col[2],
                           Laboratorio = col[3],
                           Cantidad = col[4],
                           PrecioFinal = col[5],
                           Total = col[6],
                           Editar = true
                       }).ToArray()
            };

            return Json(jsonData.rows);
        }
        [HttpPost]
        public JsonResult ProductoBusqueda(string Busqueda, int IdCotizacion)                               
        {
            ResponseModel rm = new ResponseModel();

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

                DataTable dt = GeneralesController.sqlQueryDT("ExpoMRCS", query);
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
                rm.response = true;
                rm.message = "ok";
                rm.result = jsonData.rows;
            }
            catch (Exception e)
            {
                rm.response = false;
                rm.message = e.Message;
            }


            return Json(rm.result);

        }

        [HttpPost]
        public JsonResult detalleProducto(int IdProducto, int Cliente)
        {
            string cadena = _configuration["ConnectionStrings:IndigoCS"];

            SqlConnection cnn = new SqlConnection(cadena);
            SqlCommand cmd = new SqlCommand("ExpoCotizarVerProducto", cnn);
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA;
            cnn.Open();
            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;
            //// 3. add parameter to command, which will be passed to the stored procedure
            cmd.Parameters.Add(new SqlParameter("@cliente", Cliente));
            cmd.Parameters.Add(new SqlParameter("@idproducto", IdProducto));
            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dataTable);
            cnn.Close();

            var jsonData = new
            {
                row = (

                from row in dataTable.AsEnumerable()
                select new
                {
                    imgProducto = HomeController.GetFotos(IdProducto),
                    idproducto = row[0],
                    nombre = row[1],
                    presentacion = row[2],
                    unidades = row[3],
                    publico = row[4],
                    neto = row[5],
                    iva = row[6],
                    laboratorio = row[7],
                    caja = row[8],
                    codigo = row[9],
                    restringido = row[10],
                    cantidad = row[11],
                    formula = row[12],
                    controlado = row[13],
                    preciomr = row[14],
                    descuentoexpo = row[15],
                    descuentocliente = row[16],
                    valoriva = row[17],
                    preciofinal = row[18]
                }).ToArray()

            };

            return Json(jsonData.row[0]);
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

        public IActionResult EditarClienteCotizacion(int IdCotizacion, int IdDocumento, int IdUsoCFDI, int IdTipoVenta, int IdEntrega, string Obsv )
        {
            ResponseModel rm = new ResponseModel();

            try
            {
                //string cadena = _configuration["ConnectionStrings:ExpoMRCS"];

                //SqlConnection cnn = new SqlConnection(cadena);
                //SqlCommand cmd = new SqlCommand();
                //DataTable dataTable = new DataTable();
                //SqlDataAdapter sqlDA;
                //cnn.Open();
                //cmd.CommandText = "UPDATE mCotizaciones " +
                //                "SET mCotizaciones.IdDocumento = " + IdDocumento + ", " +
                //                "mCotizaciones.IdUsoCFDI = CASE WHEN " + IdDocumento + " = 1 THEN " + IdUsoCFDI + " ELSE 23 END, " +
                //                "mCotizaciones.IdTipoVenta = " + IdTipoVenta + ", " +
                //                "mCotizaciones.Entrega = " + IdEntrega + ", " +
                //                "mCotizaciones.Obsv = '" + Obsv +"' "+
                //                " WHERE mCotizaciones.IdCotizacion = " + IdCotizacion;
                //cmd.CommandType = CommandType.Text;
                //cmd.Connection = cnn;
                //sqlDA = new SqlDataAdapter(cmd);
                //sqlDA.Fill(dataTable);
                //cnn.Close();

                string query = "UPDATE mCotizaciones " +
                                "SET mCotizaciones.IdDocumento = " + IdDocumento + ", " +
                                "mCotizaciones.IdUsoCFDI = CASE WHEN " + IdDocumento + " = 1 THEN " + IdUsoCFDI + " ELSE 23 END, " +
                                "mCotizaciones.IdTipoVenta = " + IdTipoVenta + ", " +
                                "mCotizaciones.Entrega = " + IdEntrega + ", " +
                                "mCotizaciones.Obsv = '" + Obsv + "' " +
                                " WHERE mCotizaciones.IdCotizacion = " + IdCotizacion;

                DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

                rm.response = true;
                rm.message = "Datos guardados con exito";

            }
            catch (Exception e)
            {

                rm.message = e.Message;
            }
           
            return View(rm);
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


            }
            catch (Exception e)
            {
                rm.message = e.Message;
            }
            return Json(rm);
        }

        [HttpPost]
        public JsonResult TotalCotizacion(int IdCliente) 
        {
            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            int Usuario = GeneralesController.GetIdPersonas(username);

            //string cadena = _configuration["ConnectionStrings:ExpoMRCS"];

            //SqlConnection cnn = new SqlConnection(cadena);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "SELECT vCotizacionPorConfirmar.Cliente, vCotizacionPorConfirmar.Productos, vCotizacionPorConfirmar.Subtotal, " +
            //    "vCotizacionPorConfirmar.IvaMonto AS Iva, vCotizacionPorConfirmar.Total, " +
            //    "dbo.GetCredit(" + IdCliente + ", 1) AS Credito " +
            //    "FROM vCotizacionPorConfirmar LEFT OUTER JOIN " +
            //    "vPedido ON vCotizacionPorConfirmar.Cliente = vPedido.Cliente " +
            //    "WHERE vCotizacionPorConfirmar.Cliente = " + IdCliente +
            //    "AND vCotizacionPorConfirmar.Usuario = " + Usuario;
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();

            string query = "SELECT vCotizacionPorConfirmar.Cliente, vCotizacionPorConfirmar.Productos, vCotizacionPorConfirmar.Subtotal, " +
                " vCotizacionPorConfirmar.IvaMonto AS Iva, vCotizacionPorConfirmar.Total, " +
                " dbo.GetCredit(" + IdCliente + ", 1) AS Credito " +
                " FROM vCotizacionPorConfirmar LEFT OUTER JOIN " +
                " vPedido ON vCotizacionPorConfirmar.Cliente = vPedido.Cliente " +
                " WHERE vCotizacionPorConfirmar.Cliente = " + IdCliente +
                " AND vCotizacionPorConfirmar.Usuario = " + Usuario;

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            if (dataTable.Rows.Count == 0)
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
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           cliente = col[0],
                           productos = col[1] == DBNull.Value ? 0 : col[1],
                           subtotal = col[2] == DBNull.Value ? 0 : Convert.ToDecimal(col[2]),
                           iva = col[3] == DBNull.Value ? 0 : Convert.ToDecimal(col[3]),
                           total = col[4] == DBNull.Value ? 0 : Convert.ToDecimal(col[4]),
                           credito = col[5] == DBNull.Value ? 0 : Convert.ToDecimal(col[5])

                       }).ToArray()
                };
                return Json(jsonData.rows[0]);
            }

        }

        [HttpPost]
        public JsonResult getPedidosCliente(int idCliente)
        {
            //SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "SELECT vPersonas.Nombre AS Cliente, mPedidos.IdPedido, mDocumentos.Documento, mPedidos.Fecha, COUNT(dPedidos.IdProducto) AS NumProd, Capturo.Nombre AS Capturo"
            //                    + " FROM mPedidos INNER JOIN"
            //                    + " dPedidos ON mPedidos.IdPedido = dPedidos.IdPedido INNER JOIN"
            //                    + " vPersonas ON mPedidos.Cliente = vPersonas.IdPersona INNER JOIN"
            //                    + " Indigo.dbo.mDocumentos ON mPedidos.IdDocumento = mDocumentos.IdDocumento INNER JOIN"
            //                    + " vPersonas AS Capturo ON mPedidos.Usuario = Capturo.IdPersona"
            //                    + " WHERE mPedidos.Cliente = " + idCliente
            //                    + " GROUP BY vPersonas.Nombre, mPedidos.IdPedido, mDocumentos.Documento, mPedidos.Fecha, Capturo.Nombre"
            //                    + " ORDER BY mPedidos.IdPedido";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();
            string query = "SELECT vPersonas.Nombre AS Cliente, mPedidos.IdPedido, mDocumentos.Documento, mPedidos.Fecha, COUNT(dPedidos.IdProducto) AS NumProd, Capturo.Nombre AS Capturo"
                                + " FROM mPedidos INNER JOIN"
                                + " dPedidos ON mPedidos.IdPedido = dPedidos.IdPedido INNER JOIN"
                                + " vPersonas ON mPedidos.Cliente = vPersonas.IdPersona INNER JOIN"
                                + " Indigo.dbo.mDocumentos ON mPedidos.IdDocumento = mDocumentos.IdDocumento INNER JOIN"
                                + " vPersonas AS Capturo ON mPedidos.Usuario = Capturo.IdPersona"
                                + " WHERE mPedidos.Cliente = " + idCliente
                                + " GROUP BY vPersonas.Nombre, mPedidos.IdPedido, mDocumentos.Documento, mPedidos.Fecha, Capturo.Nombre"
                                + " ORDER BY mPedidos.IdPedido";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);
            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           cliente = col[0],
                           idPedido = col[1],
                           documento = col[2],
                           fecha = col[3],
                           numProds = col[4],
                           capturo = col[5]
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
            //SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "SELECT vPedidoDatos.IdPedido, vPedidoDatos.Cliente, vPedidoDatos.RFC, vPedidoDatos.Documento, vPedidoDatos.UsoCFDI,"
            //                  + " vPedidoDatos.TipoVenta, vPedidoDatos.DirEntrega, vPedidoDatos.Transportista, vPedidoDatos.Obsv, vPedidoDatos.Porte, Capturo.Nombre AS Capturo"
            //                  + " FROM vPedidoDatos INNER JOIN"
            //                  + " mPedidos ON vPedidoDatos.IdPedido = mPedidos.IdPedido INNER JOIN"
            //                  + " vPersonas AS Capturo ON mPedidos.Usuario = Capturo.IdPersona"
            //                  + " WHERE vPedidoDatos.IdPedido = " + idPedido;
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();

            string query = "SELECT vPedidoDatos.IdPedido, vPedidoDatos.Cliente, vPedidoDatos.RFC, vPedidoDatos.Documento, vPedidoDatos.UsoCFDI,"
                              + " vPedidoDatos.TipoVenta, vPedidoDatos.DirEntrega, vPedidoDatos.Transportista, vPedidoDatos.Obsv, vPedidoDatos.Porte, Capturo.Nombre AS Capturo"
                              + " FROM vPedidoDatos INNER JOIN"
                              + " mPedidos ON vPedidoDatos.IdPedido = mPedidos.IdPedido INNER JOIN"
                              + " vPersonas AS Capturo ON mPedidos.Usuario = Capturo.IdPersona"
                              + " WHERE vPedidoDatos.IdPedido = " + idPedido;

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           IdPedido = col[0],
                           Cliente = col[1],
                           RFC = col[2],
                           Documento = col[3],
                           UsoCFDI = col[4],
                           TipoVenta = col[5],
                           DirEntrega = col[6],
                           Transportista = col[7],
                           Obsv = col[8],
                           Porte = col[9],
                           Capturo = col[10]
                       }).ToArray()
            };

            return Json(jsonData.rows[0]);
        }

        [HttpPost]
        public JsonResult getProductosPedido(int idPedido)
        {
            //SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "SELECT vProductos.IdProducto, vProductos.Nombre, vProductos.Presentacion, mLaboratorios.Laboratorio,"
            //                   + " vPedidos.Cantidad, vPedidos.PorSurtir, vPedidos.PrecioFinal, vPedidos.Total"
            //                   + " FROM vPedidos INNER JOIN"
            //                   + " vProductos ON vPedidos.IdProducto = vProductos.IdProducto INNER JOIN"
            //                   + " mLaboratorios ON vProductos.IdLaboratorio = mLaboratorios.IdLaboratorio"
            //                   + " WHERE vPedidos.IdPedido = " + idPedido
            //                   + " ORDER BY mLaboratorios.Laboratorio, vProductos.Nombre";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();
            string query = "SELECT vProductos.IdProducto, vProductos.Nombre, vProductos.Presentacion, mLaboratorios.Laboratorio,"
                               + " vPedidos.Cantidad, vPedidos.PorSurtir, vPedidos.PrecioFinal, vPedidos.Total"
                               + " FROM vPedidos INNER JOIN"
                               + " vProductos ON vPedidos.IdProducto = vProductos.IdProducto INNER JOIN"
                               + " mLaboratorios ON vProductos.IdLaboratorio = mLaboratorios.IdLaboratorio"
                               + " WHERE vPedidos.IdPedido = " + idPedido
                               + " ORDER BY mLaboratorios.Laboratorio, vProductos.Nombre";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);
            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           idProducto = col[0],
                           nombre = col[1],
                           presentacion = col[2],
                           laboratorio = col[3],
                           cantidad = col[4],
                           porSurtir = col[5],
                           precioFinal = col[6],
                           total = col[7]
                       }).ToArray()
            };

            return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult getTotalPedido(int idPedido)
        {
            //SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "SELECT mPedidos.Cliente," 
            //                   + " COUNT(dPedidos.IdProducto) AS Productos,"
            //                   + " CAST(SUM(dPedidos.Cantidad * (dPedidos.Unitario - dPedidos.Descuento)) AS DECIMAL(18, 2)) AS Subtotal,"
            //                   + " CAST(SUM(CASE WHEN mProductos.Iva = 1 THEN dPedidos.Cantidad * (dPedidos.Unitario - dPedidos.Descuento) * Indigo.dbo.GetIva(0) ELSE 0 END) AS DECIMAL(18, 2)) AS Iva,"
            //                   + " CAST(SUM(dPedidos.Cantidad * (dPedidos.Unitario - dPedidos.Descuento)) +"
            //                   + " SUM(CASE WHEN mProductos.Iva = 1 THEN dPedidos.Cantidad * (dPedidos.Unitario - dPedidos.Descuento) * Indigo.dbo.GetIva(0) ELSE 0 END) AS DECIMAL(18, 2)) AS Total"
            //                   + " FROM mPedidos INNER JOIN"
            //                   + " dPedidos ON mPedidos.IdPedido = dPedidos.IdPedido INNER JOIN"
            //                   + " mProductos ON dPedidos.IdProducto = mProductos.IdProducto"
            //                   + " WHERE mPedidos.IdPedido =" + idPedido
            //                   + " GROUP BY mPedidos.Cliente";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();
            string query = "SELECT mPedidos.Cliente,"
                               + " COUNT(dPedidos.IdProducto) AS Productos,"
                               + " CAST(SUM(dPedidos.Cantidad * (dPedidos.Unitario - dPedidos.Descuento)) AS DECIMAL(18, 2)) AS Subtotal,"
                               + " CAST(SUM(CASE WHEN mProductos.Iva = 1 THEN dPedidos.Cantidad * (dPedidos.Unitario - dPedidos.Descuento) * Indigo.dbo.GetIva(0) ELSE 0 END) AS DECIMAL(18, 2)) AS Iva,"
                               + " CAST(SUM(dPedidos.Cantidad * (dPedidos.Unitario - dPedidos.Descuento)) +"
                               + " SUM(CASE WHEN mProductos.Iva = 1 THEN dPedidos.Cantidad * (dPedidos.Unitario - dPedidos.Descuento) * Indigo.dbo.GetIva(0) ELSE 0 END) AS DECIMAL(18, 2)) AS Total"
                               + " FROM mPedidos INNER JOIN"
                               + " dPedidos ON mPedidos.IdPedido = dPedidos.IdPedido INNER JOIN"
                               + " mProductos ON dPedidos.IdProducto = mProductos.IdProducto"
                               + " WHERE mPedidos.IdPedido =" + idPedido
                               + " GROUP BY mPedidos.Cliente";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);
            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           prods = col[1],
                           subtotal = col[2],
                           iva = col[3],
                           total = col[4]
                       }).ToArray()
            };

            return Json(jsonData.rows[0]);
        }

        [HttpPost]
        public void actualizarProducto(int idPedido, int idProducto, int cantidad)
        {
            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand cmd = new SqlCommand();
            cnn.Open();
            cmd.CommandText = "UPDATE dPedidos"
                               + " SET Cantidad = Surtido + "+ cantidad
                               + " WHERE dPedidos.IdPedido = " + idPedido
                               + " AND dPedidos.IdProducto = " + idProducto;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;
            cmd.ExecuteNonQuery();
            cnn.Close();
        }

        [HttpPost]
        public void eliminaProducto(int idPedido, int idProducto)
        {
            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand cmd = new SqlCommand();
            cnn.Open();
            cmd.CommandText = "DELETE dPedidos"
                               + " WHERE dPedidos.IdPedido = " + idPedido
                               + " AND dPedidos.IdProducto = " + idProducto
                               + " AND dPedidos.Surtido = 0";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;
            cmd.ExecuteNonQuery();
            cnn.Close();
        }

        [HttpPost]
        public int colocarOrden(int IdCliente){

            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            int IdUsuario = GeneralesController.GetIdPersonas(username);

            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand command = new SqlCommand("ExpoPedidoValidar", cnn);
            SqlDataReader dr;
            command.Parameters.AddWithValue("@cliente", IdCliente);
            command.Parameters.AddWithValue("@usuario", IdUsuario);

            command.CommandType = CommandType.StoredProcedure;
            cnn.Open();

            dr = command.ExecuteReader();
            int success = 0;
            while (dr.Read())
            {
                success = dr.GetInt32(0);

            }
            cnn.Close();

            return success;


        }

        [HttpPost]

        public void ExpoPedidoAgregar( int IdCliente)
        {
            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            int IdUsuario = GeneralesController.GetIdPersonas(username);

            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand command = new SqlCommand("ExpoPedidoAgregar", cnn);
            SqlDataReader dr;
            command.Parameters.AddWithValue("@cliente", IdCliente);
            command.Parameters.AddWithValue("@usuario", IdUsuario);

            command.CommandType = CommandType.StoredProcedure;
            cnn.Open();

            SqlParameter returnParameter = command.Parameters.Add("@Id_Inserted", SqlDbType.Int);
            returnParameter.Direction = ParameterDirection.Output;

            command.ExecuteNonQuery();

            cnn.Close();
        }
    }
}
