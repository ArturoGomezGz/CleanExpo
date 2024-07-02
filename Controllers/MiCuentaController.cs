using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using ExpoMR.Datos;
using Microsoft.EntityFrameworkCore;
using ExpoMR.Clases;
using System.Net;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Capturista,Cliente,ClienteVIP,Proveedor")]
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
            string query = "SELECT Datos.Nombre, Datos.RFC, Datos.IdDocumento, Datos.IdUsoCFDI, Datos.IdTipoVenta, Datos.IdDireccion, Datos.Direccion, Datos.IdTransporte, Datos.IdPorte FROM GetDatosCliente(@IdPersona) AS Datos";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdPersona", HttpContext.Session.GetString("ClienteSel"));

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            MiCuentaViewModel MiCuenta = new();

            while (dr.Read())
            {
                MiCuenta.Nombre = (string)dr["Nombre"];
                MiCuenta.RFC = (string)dr["RFC"];
                MiCuenta.Direccion = (string)dr["Direccion"];
                MiCuenta.IdTipoDoc = (byte)dr["IdDocumento"];
                MiCuenta.TipoDocumentos = WebSite.GetTiposDocumentos(Convert.ToInt32(HttpContext.Session.GetString("ClienteSel")));
                MiCuenta.IdUsoCFDI = (byte)dr["IdUsoCFDI"];
                MiCuenta.UsoCFDI = WebSite.GetUsosCFDI(Convert.ToInt32(HttpContext.Session.GetString("ClienteSel")));
                MiCuenta.IdTipoVenta = (byte)dr["IdTipoVenta"];
                MiCuenta.TiposVenta = WebSite.GetTiposVenta();
                MiCuenta.Entrega = (int)dr["IdDireccion"];
                MiCuenta.Direccciones = WebSite.GetDireccionesEntrega(Convert.ToInt32(HttpContext.Session.GetString("ClienteSel")));
                MiCuenta.IdTransporte = (int)dr["IdTransporte"];
                MiCuenta.Trasportes = WebSite.GetTransportes(Convert.ToInt32(HttpContext.Session.GetString("ClienteSel")));
                MiCuenta.IdPorte = (byte)dr["IdPorte"];
                MiCuenta.Portes = GetPorte(Convert.ToInt32(HttpContext.Session.GetString("ClienteSel")));
            }

			dB.Close();

			var currentUser = await _userManager.GetUserAsync(User);

            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "MiCuenta");
            }
            else
            {
                int isFechaExpoVIP = ExpoMR.Clases.WebSite.ValidaExpoVIP();

                if (ExpoMR.Clases.WebSite.ValidaRolUsuario(currentUser.Email, "ClienteVIP") == true)
                {
                    if (isFechaExpoVIP == 1) //1 = Expo VIP
                    {
                        return View(MiCuenta);
                    }
                    else
                    {
                        return RedirectToAction("Index", "MiCuenta");
                    }
                }
                else
                {
                    if (isFechaExpoVIP == 0) // 0 = Expo Normal
                    {
                        return View(MiCuenta);
                    }
                    else
                    {
                        return RedirectToAction("Index", "MiCuenta");                        
                    }
                }
            }            
        }

        [HttpPost]
        public JsonResult GetDatosFacturacion(int IdCliente)
        {
            string query = "SELECT Datos.Nombre, Datos.RFC, Datos.IdDocumento, Datos.IdUsoCFDI, Datos.IdTipoVenta, Datos.IdDireccion, Datos.Direccion, Datos.IdTransporte, Datos.IdPorte FROM GetDatosCliente(@IdPersona) AS Datos";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdPersona", IdCliente);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            var jsonData = new
            {
                rows = (
                       from c in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           Nombre = c["Nombre"],
                           RFC = c["RFC"],
                           IdDocumento = c["IdDocumento"],
                           IdUsoCFDI = c["IdUsoCFDI"],
                           Direccion = c["Direccion"]
                       }).ToArray()
            };

			dB.Close();

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
      
        public List<Portes> GetPorte(int IdCliente)
        {
            string query = "SELECT 0 AS IdPorte, '...' AS Porte " +
                            "UNION " +
                            "SELECT dPorteCliente.IdPorte, mPortes.Porte " +
                            "FROM Indigo.dbo.dPorteCliente INNER JOIN " +
                            "Indigo.dbo.mPortes ON dPorteCliente.IdPorte = mPortes.IdPorte " +
                            "WHERE dPorteCliente.IdPersona =  @IdPersona " +
                            "ORDER BY Porte";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdPersona", IdCliente);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            List<Portes> Portes = new();

            while (dr.Read())
            {
                Portes Porte = new();

                Porte.Idporte = (int)dr["IdPorte"];
                Porte.Porte = (string)dr["Porte"];

                Portes.Add(Porte);
            }

			dB.Close();

			return Portes;
        }

        public JsonResult SaveTipoDoc(int UsoCFDI, int TipoDoc)
        {
            var rm = new ResponseModel();
       
            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[7];
                parameters[0] = new SqlParameter("@IdPersona", HttpContext.Session.GetString("ClienteSel"));
                parameters[1] = new SqlParameter("@TipoDoc", TipoDoc);
                parameters[2] = new SqlParameter("@IdUsoCFDI", UsoCFDI);
                parameters[3] = new SqlParameter("@IdTipoVenta", "");
                parameters[4] = new SqlParameter("@IdDirEntrega", "");
                parameters[5] = new SqlParameter("@IdTransporte", "");
                parameters[6] = new SqlParameter("@IdPorte", "");

                dB.ExecuteNonQuery("EditCliente", ref parameters);

                rm.Response = true;
                rm.Message = "Sus datos se actualizaron correctamente";

            }
            catch (Exception e) 
            {
                rm.Response = false;
                rm.Message = e.Message;
            }
            return Json(rm);
        }

        public JsonResult Entrega(int TipoVenta, int Entrega, int Transporte, int Porte) 
        {
            var rm = new ResponseModel();

            try 
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[7];
                parameters[0] = new SqlParameter("@IdPersona", HttpContext.Session.GetString("ClienteSel"));
                parameters[1] = new SqlParameter("@TipoDoc", "");
                parameters[2] = new SqlParameter("@IdUsoCFDI", "");
                parameters[3] = new SqlParameter("@IdTipoVenta", TipoVenta);
                parameters[4] = new SqlParameter("@IdDirEntrega", Entrega);
                parameters[5] = new SqlParameter("@IdTransporte", Transporte);
                parameters[6] = new SqlParameter("@IdPorte", Porte);

                dB.ExecuteNonQuery("EditCliente", ref parameters);

                rm.Response = true;
                rm.Message = "Sus datos se actualizaron correctamente";

            }
            catch (Exception e)
            {
                rm.Response = false;
                rm.Message = e.Message;
            }
            return Json(rm);
        }
    }
}