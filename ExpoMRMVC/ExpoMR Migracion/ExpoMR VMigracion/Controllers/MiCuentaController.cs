using ExpoMR.Models;
using ExpoMR.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;
using ExpoMR.Clases;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ExpoMR.Controllers
{
    [Authorize]
    public class MiCuentaController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _rolManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private static IConfiguration _configuration;

        //Variables para verificar La sesión Y el Usuario
        public MiCuentaController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender, RoleManager<IdentityRole> rolManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _rolManager = rolManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _rolManager = rolManager;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CambiarPasswordViewModel model, string email)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _userManager.FindByEmailAsync(email);

                if (usuario == null)
                {
                    return RedirectToAction("Error");
                }
                string token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                //string code = token.Replace(" ", "+");
                var resultado = await _userManager.ResetPasswordAsync(usuario, token, model.Password);

                if (resultado.Succeeded)
                {
                    var currentUser = await _userManager.GetUserAsync(User);

                    // Verificar si el correo electrónico ya está confirmado
                    if (!currentUser.EmailConfirmed)
                    {
                        // Generar el token de confirmación de correo electrónico
                        var tokenConfirmacion = await _userManager.GenerateEmailConfirmationTokenAsync(currentUser);

                        // Confirmar el correo electrónico
                        var result = await _userManager.ConfirmEmailAsync(currentUser, tokenConfirmacion);

                        if (result.Succeeded)
                        {
                            // El correo electrónico se ha confirmado exitosamente
                            // Realiza la lógica adicional aquí, como redirigir al usuario a una página de éxito
                        }
                        else
                        {
                            // Ocurrió un error al confirmar el correo electrónico
                            // Puedes mostrar un mensaje de error o tomar acciones adicionales
                        }
                    }
                    return RedirectToAction("ConfirmacionCambioPassword");
                }
                else
                {
                    ValidarErrores(resultado);
                }
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult ConfirmacionCambioPassword()
        {
            return View();
        }

        public IActionResult MiCuenta()
        {
            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            int IdPersona = GeneralesController.GetIdPersonas(username);
            int cliente = GeneralesController.GetClientes(username);

            string query = "SELECT vPersonas.Nombre, vPersonas.RFC, mDocumentos.IdDocumento , ISNULL(mSatUsoCFDI.IdUsoCFDI,0) AS IdUsoCFDI,"
                             + "   Indigo.dbo.FormatAddress(vPersonas.CalleNum, vPersonas.Colonia, vPersonas.Ciudad, vPersonas.Estado, vPersonas.CP) AS Direccion,"
                             + "   mClientes.IdTipoVenta, dDirecciones.IdDireccion, ISNULL(mClientes.IdTransporte,0) AS IdTransporte,"
                             + "   ISNULL(mClientes.IdPorte,0) AS IdPorte"
                             + "   FROM ExpoMR.dbo.mClientes INNER JOIN"
                             + "   mDocumentos ON mClientes.IdDocumento = mDocumentos.IdDocumento INNER JOIN"
                             + "   mPerEmp ON mClientes.IdPersona = mPerEmp.IdPersona INNER JOIN"
                             + "   vPersonas ON mPerEmp.IdPersona = vPersonas.IdPersona INNER JOIN"
                             + "   dDirecciones ON mPerEmp.Persona = dDirecciones.Persona AND dDirecciones.Fiscal = 1 LEFT OUTER JOIN"
                             + "   mSatUsoCFDI ON mClientes.IdUsoCFDI = mSatUsoCFDI.IdUsoCFDI"
                             + "   WHERE mClientes.IdPersona = " + cliente;

            DataTable dt = GeneralesController.sqlQueryDT("IndigoCS", query);

            MiCuentaViewModel Cuenta = new();
            Cuenta.Nombre = dt.Rows[0]["Nombre"].ToString();
            Cuenta.RFC = dt.Rows[0]["RFC"].ToString();
            Cuenta.Direccion = dt.Rows[0]["Direccion"].ToString();
            Cuenta.IdTipoDoc = Convert.ToInt16(dt.Rows[0]["IdDocumento"]);
            Cuenta.TipoDocumentos = GetTipoDocumentos(cliente);
            Cuenta.IdUsoCFDI = Convert.ToInt16(dt.Rows[0]["IdUsoCFDI"]);
            Cuenta.UsoCFDI = GetUsoCFDIs(cliente);
            Cuenta.IdTipoVenta = Convert.ToInt16(dt.Rows[0]["IdTipoVenta"]);
            Cuenta.TiposVenta = GetTiposVenta();
            Cuenta.Entrega = Convert.ToInt16(dt.Rows[0]["IdDireccion"]);
            Cuenta.Direccciones = GetDirecciones(cliente);
            Cuenta.IdTransporte = Convert.ToInt16(dt.Rows[0]["IdTransporte"]);
            Cuenta.Trasportes = GetTrasportes(cliente);
            Cuenta.IdPorte = Convert.ToInt16(dt.Rows[0]["IdPorte"]);
            Cuenta.Portes = GetPorte(cliente);

            return View(Cuenta);
        }
        [HttpPost]
        public JsonResult GetDatosFacturacion(int IdCliente)
        {

            //SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:IndigoCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "SELECT vPersonas.Nombre, vPersonas.RFC, mDocumentos.IdDocumento , ISNULL(mSatUsoCFDI.IdUsoCFDI,0) AS IdUsoCFDI,"
            //                    + "Indigo.dbo.FormatAddress(vPersonas.CalleNum, vPersonas.Colonia, vPersonas.Ciudad, vPersonas.Estado, vPersonas.CP) AS Direccion"
            //                    + " FROM ExpoMR.dbo.mClientes INNER JOIN"
            //                    + " mDocumentos ON mClientes.IdDocumento = mDocumentos.IdDocumento INNER JOIN"
            //                    + " mPerEmp ON mClientes.IdPersona = mPerEmp.IdPersona INNER JOIN"
            //                    + " vPersonas ON mPerEmp.IdPersona = vPersonas.IdPersona INNER JOIN"
            //                    + " dDirecciones ON mPerEmp.Persona = dDirecciones.Persona AND dDirecciones.Fiscal = 1 LEFT OUTER JOIN"
            //                    + " mSatUsoCFDI ON mClientes.IdUsoCFDI = mSatUsoCFDI.IdUsoCFDI"
            //                    + " WHERE mClientes.IdPersona = " + IdCliente;
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();

            string query = "SELECT vPersonas.Nombre, vPersonas.RFC, mDocumentos.IdDocumento , ISNULL(mSatUsoCFDI.IdUsoCFDI,0) AS IdUsoCFDI,"
                            + "Indigo.dbo.FormatAddress(vPersonas.CalleNum, vPersonas.Colonia, vPersonas.Ciudad, vPersonas.Estado, vPersonas.CP) AS Direccion"
                            + " FROM ExpoMR.dbo.mClientes INNER JOIN"
                            + " mDocumentos ON mClientes.IdDocumento = mDocumentos.IdDocumento INNER JOIN"
                            + " mPerEmp ON mClientes.IdPersona = mPerEmp.IdPersona INNER JOIN"
                            + " vPersonas ON mPerEmp.IdPersona = vPersonas.IdPersona INNER JOIN"
                            + " dDirecciones ON mPerEmp.Persona = dDirecciones.Persona AND dDirecciones.Fiscal = 1 LEFT OUTER JOIN"
                            + " mSatUsoCFDI ON mClientes.IdUsoCFDI = mSatUsoCFDI.IdUsoCFDI"
                            + " WHERE mClientes.IdPersona = " + IdCliente;

            DataTable dataTable = GeneralesController.sqlQueryDT("IndigoCS", query);

            var jsonData = new
            {
                rows = (
                       from c in dataTable.AsEnumerable()
                       select new
                       {
                           Nombre = c[0],
                           RFC = c[1],
                           IdDocumento = c[2],
                           IdUsoCFDI = c[3],
                           Direccion = c[4]
                       }).ToArray()
            };


            return Json(jsonData.rows);
        }

        [AllowAnonymous]
        private void ValidarErrores(IdentityResult resultado)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(String.Empty, error.Description);
            }
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

        public List<Entregas> GetDirecciones(int IdCliente)
        {
            List<Entregas> Entregas = new();
            Entregas.Add(new Entregas { IdEntrega = 0, Entrega = "Seleccione una opción... " });
            string query = "Select vDirecciones.IdDireccion AS Entrega, vDirecciones.Ciudad + ', ' + vDirecciones.Estado + ' - ' + vDirecciones.Direccion AS DireccionE"
                          + " From vDirecciones inner join"
                          + " mPerEmp ON vDirecciones.Persona = mPerEmp.Persona"
                          + " Where mPerEmp.IdPersona =" +IdCliente
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
        public List<Portes> GetPorte(int IdCliente)
        {
            List<Portes> Portes = new();
            string query = "Select 0 as idporte, 'Seleccione una opción... ' as Porte"
                           + " UNION"
                           + "  Select dPorteCliente.IdPorte, mPortes.Porte"
                           + "  From dPorteCliente inner join"
                           + "  mPortes on dPorteCliente.IdPorte = mPortes.IdPorte"
                           + "  Where dPorteCliente.IdPersona = " + IdCliente
                           + " Order by Porte";

            DataTable dt = GeneralesController.sqlQueryDT("IndigoCS", query);
            foreach (DataRow dr in dt.Rows)
            {
                Portes Porte = new();
                Porte.Idporte = Convert.ToInt16(dr["idporte"]);
                Porte.Porte = dr["Porte"].ToString();
                Portes.Add(Porte);
            }

            return Portes;

        }

        public JsonResult SaveTipoDoc(int UsoCFDI, int TipoDoc) {
            var rm = new ResponseModel();
            try {
                var user = _userManager.GetUserAsync(User);
                string username = user.Result.UserName.ToString();
                int Usuario = GeneralesController.GetIdPersonas(username);
                string cadena = _configuration["ConnectionStrings:IndigoCS"];

                string insertQuery = "Update ExpoMR.dbo.mClientes " +
                                    " Set IdDocumento = @iddocumento,"+
                                    " IdUsoCFDI = CASE WHEN @iddocumento = 1 THEN @idusocfdi ELSE 0 END " +
                                    " Where mClientes.IdPersona = "+ Usuario;

                using (SqlConnection connection = new SqlConnection(cadena))
                {
                    SqlCommand command = new SqlCommand(insertQuery, connection);
                    command.Parameters.AddWithValue("@iddocumento", TipoDoc);
                    command.Parameters.AddWithValue("@idusocfdi", UsoCFDI);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                rm.response = true;
                rm.message = "Sus datos se guardaron correctamente";

            }
            catch (Exception e) {
                rm.response = false;
                rm.message = e.Message;
            }

            return Json(rm);
        }
        public JsonResult Entrega(int TipoVenta, int Entrega, int Transporte, int Porte) {
            var rm = new ResponseModel();
            try {
                var user = _userManager.GetUserAsync(User);
                string username = user.Result.UserName.ToString();
                int Usuario = GeneralesController.GetIdPersonas(username);
                string cadena = _configuration["ConnectionStrings:ExpoMRCS"];

                string insertQuery = "Update mClientes"+
                                " Set IdTipoVenta = @idtipoventa,"+
                                " Entrega = @entrega,"+
                                " IdTransporte = @idtransporte,"+
                                " IdPorte = @idporte"+
                                " Where mClientes.IdPersona =" + Usuario;

                using (SqlConnection connection = new SqlConnection(cadena))
                {
                    SqlCommand command = new SqlCommand(insertQuery, connection);
                    command.Parameters.AddWithValue("@idtipoventa", TipoVenta);
                    command.Parameters.AddWithValue("@entrega", Entrega);
                    command.Parameters.AddWithValue("@idtransporte", Transporte);
                    command.Parameters.AddWithValue("@idporte", Porte);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                rm.response = true;
                rm.message = "Sus datos se guardaron correctamente";

            }
            catch (Exception e) {
                rm.response = false;
                rm.message = e.Message;
            }
            return Json(rm);
        }
    }
}
