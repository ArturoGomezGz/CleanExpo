using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using ExpoMR.Datos;
using Microsoft.EntityFrameworkCore;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Cliente,ClienteVIP")]
    public class MiCuentaController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private static IConfiguration _configuration;

        //Variables para verificar La sesión Y el Usuario
        public MiCuentaController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
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

        public async Task<IActionResult> MiCuenta()
        {
            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
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

            DataTable dt = GeneralesController.SqlQueryDT("IndigoCS", query);

            MiCuentaViewModel Cuenta = new()
            {
                Nombre = dt.Rows[0]["Nombre"].ToString(),
                RFC = dt.Rows[0]["RFC"].ToString(),
                Direccion = dt.Rows[0]["Direccion"].ToString(),
                IdTipoDoc = Convert.ToInt16(dt.Rows[0]["IdDocumento"]),
                TipoDocumentos = GetTipoDocumentos(cliente),
                IdUsoCFDI = Convert.ToInt16(dt.Rows[0]["IdUsoCFDI"]),
                UsoCFDI = GetUsoCFDIs(cliente),
                IdTipoVenta = Convert.ToInt16(dt.Rows[0]["IdTipoVenta"]),
                TiposVenta = GetTiposVenta(),
                Entrega = Convert.ToInt16(dt.Rows[0]["IdDireccion"]),
                Direccciones = GetDirecciones(cliente),
                IdTransporte = Convert.ToInt16(dt.Rows[0]["IdTransporte"]),
                Trasportes = GetTrasportes(cliente),
                IdPorte = Convert.ToInt16(dt.Rows[0]["IdPorte"]),
                Portes = GetPorte(cliente)
            };

            var currentUser = await _userManager.GetUserAsync(User);
            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "MiCuenta");
            }
            else
            {
                var userId = _userManager.GetUserId(User); // Obtiene el ID del usuario actualmente autenticado
                var userAsync = await _userManager.FindByIdAsync(userId); // Encuentra el usuario por su ID
                var roles = await _userManager.GetRolesAsync(userAsync);

                string Rol = roles.FirstOrDefault();
                using var dbExpo = new ApplicationDbContextExpo();
                List<IsFechaExpo> usuario = dbExpo.Set<IsFechaExpo>()
                    .FromSqlRaw("EXEC GetFechaExpo").ToList();
                List<IsFechaExpoVIP> usuarioVIP = dbExpo.Set<IsFechaExpoVIP>()
                    .FromSqlRaw("EXEC GetFechaExpoVIP").ToList();

                bool isFechaExpo = usuario.Select(x => x.FechaExpo).FirstOrDefault();
                bool isFechaExpoVIP = usuarioVIP.Select(x => x.FechaVip).FirstOrDefault();

                if (Rol == "ClienteVIP")
                {
                    if (isFechaExpoVIP)
                    {
                        return View(Cuenta);
                    }
                    else
                    {
                        return RedirectToAction("Index", "MiCuenta");
                    }
                }
                else
                {
                    if (isFechaExpoVIP)
                    {
                        return RedirectToAction("Index", "MiCuenta");
                    }
                    else
                    {
                        if (isFechaExpo)
                        {
                            return View(Cuenta);
                        }
                        else
                        {
                            return RedirectToAction("Index", "MiCuenta");
                        }
                    }
                }
            }
            
        }
        [HttpPost]
        public JsonResult GetDatosFacturacion(int IdCliente)
        {
            string query = "SELECT vPersonas.Nombre, vPersonas.RFC, mDocumentos.IdDocumento , ISNULL(mSatUsoCFDI.IdUsoCFDI,0) AS IdUsoCFDI,"
                            + "Indigo.dbo.FormatAddress(vPersonas.CalleNum, vPersonas.Colonia, vPersonas.Ciudad, vPersonas.Estado, vPersonas.CP) AS Direccion"
                            + " FROM ExpoMR.dbo.mClientes INNER JOIN"
                            + " mDocumentos ON mClientes.IdDocumento = mDocumentos.IdDocumento INNER JOIN"
                            + " mPerEmp ON mClientes.IdPersona = mPerEmp.IdPersona INNER JOIN"
                            + " vPersonas ON mPerEmp.IdPersona = vPersonas.IdPersona INNER JOIN"
                            + " dDirecciones ON mPerEmp.Persona = dDirecciones.Persona AND dDirecciones.Fiscal = 1 LEFT OUTER JOIN"
                            + " mSatUsoCFDI ON mClientes.IdUsoCFDI = mSatUsoCFDI.IdUsoCFDI"
                            + " WHERE mClientes.IdPersona = " + IdCliente;

            DataTable dataTable = GeneralesController.SqlQueryDT("IndigoCS", query);

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

            DataTable dt = GeneralesController.SqlQueryDT("IndigoCS", query);
            List<TipoDocumentos> td = new();

            foreach (DataRow dr in dt.Rows)
            {
                TipoDocumentos tipoDocumentos = new()
                {
                    IdTipoDoc = Convert.ToByte(dr["IdDocumento"]),
                    TipoDoc = dr["Documento"].ToString()
                };
                td.Add(tipoDocumentos);
            }
            return td;
        }

        public List<UsosCFDI> GetUsoCFDIs(int IdCliente)
        {
            List<UsosCFDI> usoCFDIs = new();
            string query = "SELECT GetUsoCFDI.IdUsoCFDI, GetUsoCFDI.UsoCFDI" +
                " FROM GetUsoCFDI(" + IdCliente + ")";

            //string d = "SELECT GetUsoCFDI.IdUsoCFDI, GetUsoCFDI.UsoCFDI FROM GetUsoCFDI (@cliente)";
            DataTable dt = GeneralesController.SqlQueryDT("IndigoCS", query);
            if(dt.Rows.Count <= 1)
                usoCFDIs.Add(new UsosCFDI { IdUsoCFDI = 23, UsoCFDI = "Sin efectos fiscales" });
            foreach (DataRow dr in dt.Rows)
            {
                UsosCFDI CFDI = new()
                {
                    IdUsoCFDI = Convert.ToByte(dr["IdUsoCFDI"]),
                    UsoCFDI = dr["UsoCFDI"].ToString()
                };
                usoCFDIs.Add(CFDI);
            }
            return usoCFDIs;
        }

        public List<TiposDeVenta> GetTiposVenta()
        {
            List<TiposDeVenta> tipoventa = new()
            {
                new TiposDeVenta { IdTipoVenta = 0, TipoVenta = "Seleccione una opción... " }
            };
            string query = "Select mTipoVenta.IdTipoVenta, mTipoVenta.TipoVenta"
                           + " From mTipoVenta"
                           + "  Where mTipoVenta.IdTipoVenta != 5"
                           + "  Order by mTipoVenta.TipoVenta";

            DataTable dt = GeneralesController.SqlQueryDT("IndigoCS", query);
            foreach (DataRow dr in dt.Rows)
            {
                TiposDeVenta tipo = new()
                {
                    IdTipoVenta = Convert.ToByte(dr["IdTipoVenta"]),
                    TipoVenta = dr["TipoVenta"].ToString()
                };
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

            DataTable dt = GeneralesController.SqlQueryDT("IndigoCS", query);
            foreach (DataRow dr in dt.Rows)
            {
                Entregas Entrega = new()
                {
                    IdEntrega = Convert.ToInt16(dr["Entrega"]),
                    Entrega = dr["DireccionE"].ToString()
                };
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

            DataTable dt = GeneralesController.SqlQueryDT("IndigoCS", query);
            foreach (DataRow dr in dt.Rows)
            {
                Trasportes Trasporte = new()
                {
                    IdTrasportes = Convert.ToInt16(dr["IdPersona"]),
                    Transporte = dr["Nombre"].ToString()
                };
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

            DataTable dt = GeneralesController.SqlQueryDT("IndigoCS", query);
            foreach (DataRow dr in dt.Rows)
            {
                Portes Porte = new()
                {
                    Idporte = Convert.ToInt16(dr["idporte"]),
                    Porte = dr["Porte"].ToString()
                };
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
                                    " IdUsoCFDI = CASE WHEN @iddocumento = 1 THEN @idusocfdi ELSE 23 END " +
                                    " Where mClientes.IdPersona = "+ Usuario;

                using (SqlConnection connection = new(cadena))
                {
                    SqlCommand command =  new(insertQuery, connection);
                    command.Parameters.AddWithValue("@iddocumento", TipoDoc);
                    command.Parameters.AddWithValue("@idusocfdi", UsoCFDI);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                rm.Response = true;
                rm.Message = "Sus datos se guardaron correctamente";

            }
            catch (Exception e) {
                rm.Response = false;
                rm.Message = e.Message;
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

                using (SqlConnection connection = new(cadena))
                {
                    SqlCommand command =  new(insertQuery, connection);
                    command.Parameters.AddWithValue("@idtipoventa", TipoVenta);
                    command.Parameters.AddWithValue("@entrega", Entrega);
                    command.Parameters.AddWithValue("@idtransporte", Transporte);
                    command.Parameters.AddWithValue("@idporte", Porte);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                rm.Response = true;
                rm.Message = "Sus datos se guardaron correctamente";

            }
            catch (Exception e) {
                rm.Response = false;
                rm.Message = e.Message;
            }
            return Json(rm);
        }
    }
}
