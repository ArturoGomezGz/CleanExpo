using ExpoMR.Clases;
using ExpoMR.Datos;
using ExpoMR.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.Authorization;
using ExpoMR.Servicios;
using System.Text.Json;
using System.Text;

namespace ExpoMR.Controllers
{
    //[Authorize]
    public class GeneralesController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private static IConfiguration _configuration;
        private readonly MrAdminApiService _mrAdminApi;

        //Variables para verificar La sesión Y el Usuario
        public GeneralesController(UserManager<IdentityUser> userManager, IConfiguration configuration, MrAdminApiService mrAdminApi)
        {
            _userManager = userManager;
            _configuration = configuration;
            _mrAdminApi = mrAdminApi;
        }

        public int GetMonedas() 
        {
            return WebSite.SaldoMonedas(Convert.ToInt32(HttpContext.Session.GetString("ClienteSel")));
        }

        public string GetRuleta(int IdPersona)
        {
            return WebSite.GetRuleta(IdPersona);
        }

        public int GetCarrito()
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@cliente", HttpContext.Session.GetString("ClienteSel"));
            parameters[1] = new SqlParameter("@idEmail", HttpContext.Session.GetString("Session_IdEmail"));

            return (int)dB.ExecuteScalar("ExpoCountProducts", ref parameters);
        }

        [HttpPost]
        public JsonResult GetRegistroMonedas()
        {
             DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@idpersona", HttpContext.Session.GetString("ClienteSel"));
            parameters[1] = new()
            {
                ParameterName = "@saldo",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            };

            SqlDataReader dr = dB.ExecuteReader("MonedasSaldoDetalle", ref parameters);

            var jsonData = new
            {
                rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           fecha = row["Fecha"],
                           concepto = row["Concepto"],
                           importe = row["Monedas"],
                           saldo = row["Saldo"]
                       }).ToArray()
            };

			dB.Close();

			return Json(jsonData.rows);
        }

        public static byte[] GetFoto(int idProducto)
        {
            byte[] fotos = Array.Empty<byte>();

            string query = "SELECT Foto.IdFoto, Foto.IdProducto, Foto.Foto FROM GetFotoProd(@IdProducto) AS Foto";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProducto", idProducto);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);
           
            while (dr.Read())
            {
                fotos = (dr["Foto"] != DBNull.Value ? (byte[])dr["Foto"] : Array.Empty<byte>());
            }

			dB.Close();

			return fotos;
        }

        public int SetSessionClienteSel(int IdPersona, int IdEmail)
        {
            HttpContext.Session.SetString("ClienteSel", IdPersona.ToString());
            HttpContext.Session.SetString("Session_IdEmail", IdEmail.ToString());

            Log.SaveVisita(IdPersona, 0, 0);  //Se guarda el registro de la visita general del cliente en la Expo

            return Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));
        }

        public static List<Clientes> GetClientesSeleccion(string UserName)
        {
            List<Clientes> ClientesSeleccion = new();

            string query = "SELECT Persona.IdPersona, UPPER(Persona.Nombre) AS Nombre, Persona.IdEmailFuncion, Persona.IdEmail FROM GetPersonasByEmail(@UserName) AS Persona";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@UserName", UserName);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                Clientes Cliente = new Clientes();

                Cliente.IdPersona = (int)dr["IdPersona"];
                Cliente.Nombre = (string)dr["Nombre"];
                Cliente.IdTipoUsuario = (byte)dr["IdEmailFuncion"];
                Cliente.IdEmail = (int)dr["IdEmail"];

                ClientesSeleccion.Add(Cliente);
            }

			dB.Close();

			return ClientesSeleccion;
        }

        public bool AsignarMonedas(int Origen, int Destino, int IdMotivo, int Monedas, int IdProducto, int Usuario)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[6];
            parameters[0] = new SqlParameter("@origen", Origen);
            parameters[1] = new SqlParameter("@destino", Destino);
            parameters[2] = new SqlParameter("@idmotivo", IdMotivo);
            parameters[3] = new SqlParameter("@monedas", Monedas == 0 ? null : Monedas);
            parameters[4] = new SqlParameter("@idproducto", IdProducto == 0 ? null : IdProducto);
            parameters[5] = new SqlParameter("@usuario", Usuario);

            bool ret = Convert.ToBoolean(dB.ExecuteReturnValue("MonedasAsignar", ref parameters));
            return ret;
        }

        public bool AsignarMonedasVisitaStand(int Origen, int Destino, int Usuario)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@origen", Origen);
            parameters[1] = new SqlParameter("@destino", Destino);
            parameters[2] = new SqlParameter("@usuario", Usuario);

            bool ret = Convert.ToBoolean(dB.ExecuteReturnValue("MonedasStand", ref parameters));
            return ret;
        }

        public bool AsignarMonedasVisitaProducto(int Origen, int Destino, int IdProducto, int Usuario)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[4];
            parameters[0] = new SqlParameter("@origen", Origen);
            parameters[1] = new SqlParameter("@destino", Destino);
            parameters[2] = new SqlParameter("@idproducto", IdProducto);
            parameters[3] = new SqlParameter("@usuario", Usuario);

            bool ret = Convert.ToBoolean(dB.ExecuteReturnValue("MonedasProducto", ref parameters));

            return ret;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ApiAdminLogin()
        {
            // Si ya hay token, ir directo al dashboard
            if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString("ApiAdmin_Token")))
                return RedirectToAction(nameof(ApiAdminDashboard));

            var vm = new ApiAdminPanelViewModel
            {
                Entorno = HttpContext.Session.GetString("ApiAdmin_Entorno"),
                Usuario = HttpContext.Session.GetString("ApiAdmin_Usuario"),
                IsAuthenticated = false,
                Mensaje = TempData["ApiAdmin_AlertMessage"] as string
            };

            return View(vm);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ApiAdminPanel()
        {
            // Mantener compatibilidad: ApiAdminPanel ahora es el dashboard
            return RedirectToAction(nameof(ApiAdminDashboard));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ApiAdminDashboard()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("ApiAdmin_Token")))
                return RedirectToAction(nameof(ApiAdminLogin));

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ApiAdminCrearUsuario()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("ApiAdmin_Token")))
                return RedirectToAction(nameof(ApiAdminLogin));

            return View(new ApiAdminPanelViewModel { Mensaje = TempData["ApiAdmin_AlertMessage"] as string });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ApiAdminCambiarContrasena()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("ApiAdmin_Token")))
                return RedirectToAction(nameof(ApiAdminLogin));

            return View(new ApiAdminPanelViewModel { Mensaje = TempData["ApiAdmin_AlertMessage"] as string });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ApiAdminCambiarStatus()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("ApiAdmin_Token")))
                return RedirectToAction(nameof(ApiAdminLogin));

            return View(new ApiAdminPanelViewModel { Mensaje = TempData["ApiAdmin_AlertMessage"] as string });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ApiAdminCambiarRol()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("ApiAdmin_Token")))
                return RedirectToAction(nameof(ApiAdminLogin));

            return View(new ApiAdminPanelViewModel { Mensaje = TempData["ApiAdmin_AlertMessage"] as string });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ApiAdminEliminarUsuario()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("ApiAdmin_Token")))
                return RedirectToAction(nameof(ApiAdminLogin));

            return View(new ApiAdminPanelViewModel { Mensaje = TempData["ApiAdmin_AlertMessage"] as string });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiAdminPanelLogin(ApiAdminPanelViewModel vm, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(vm.Entorno))
                ModelState.AddModelError(nameof(vm.Entorno), "Entorno es requerido");
            if (string.IsNullOrWhiteSpace(vm.Usuario))
                ModelState.AddModelError(nameof(vm.Usuario), "Usuario es requerido");
            if (string.IsNullOrWhiteSpace(vm.Password))
                ModelState.AddModelError(nameof(vm.Password), "Contraseña es requerida");

            if (!ModelState.IsValid)
                return View("ApiAdminLogin", vm);

            var baseUrl = BuildApiBaseUrl(vm.Entorno);

            try
            {
                var token = await _mrAdminApi.GetTokenAsync(baseUrl, vm.Usuario!, vm.Password!, ct);

                // Validar rol admin desde el token (idRol debe ser "1")
                if (!TryGetJwtClaim(token, "idRol", out var idRol) || idRol != "1")
                {
                    TempData["ApiAdmin_AlertMessage"] = "Acceso denegado: este usuario no tiene permisos de administrador para usar el panel.";
                    return RedirectToAction(nameof(ApiAdminLogin));
                }

                HttpContext.Session.SetString("ApiAdmin_Token", token);
                HttpContext.Session.SetString("ApiAdmin_BaseUrl", baseUrl);
                HttpContext.Session.SetString("ApiAdmin_Usuario", vm.Usuario!);
                HttpContext.Session.SetString("ApiAdmin_Entorno", vm.Entorno ?? string.Empty);

                return RedirectToAction(nameof(ApiAdminPanel));
            }
            catch (Exception ex)
            {
                vm.IsAuthenticated = false;
                vm.Mensaje = "No se pudo autenticar: " + ex.Message;
            }

            // Volver al login del panel
            vm.Password = null;
            return View("ApiAdminLogin", vm);
        }

        private static bool TryGetJwtClaim(string jwt, string claimName, out string? value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(jwt))
                return false;

            var parts = jwt.Split('.');
            if (parts.Length < 2)
                return false;

            try
            {
                var payloadJson = DecodeBase64Url(parts[1]);
                using var doc = JsonDocument.Parse(payloadJson);
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                    return false;

                if (!root.TryGetProperty(claimName, out var claim))
                    return false;

                value = claim.ValueKind switch
                {
                    JsonValueKind.String => claim.GetString(),
                    JsonValueKind.Number => claim.GetInt32().ToString(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => claim.ToString()
                };

                return !string.IsNullOrWhiteSpace(value);
            }
            catch
            {
                return false;
            }
        }

        private static string DecodeBase64Url(string input)
        {
            // base64url -> base64
            var s = input.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 0: break;
                case 2: s += "=="; break;
                case 3: s += "="; break;
                default: throw new FormatException("Base64Url inválido");
            }

            var bytes = Convert.FromBase64String(s);
            return Encoding.UTF8.GetString(bytes);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ApiAdminPanelLogout()
        {
            HttpContext.Session.Remove("ApiAdmin_Token");
            return RedirectToAction(nameof(ApiAdminLogin));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiAdminPanelOperacion(ApiAdminPanelViewModel vm, CancellationToken ct)
        {
            var token = HttpContext.Session.GetString("ApiAdmin_Token");
            var baseUrl = HttpContext.Session.GetString("ApiAdmin_BaseUrl");

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(baseUrl))
            {
                TempData["ApiAdmin_AlertMessage"] = "No autenticado.";
                return RedirectToAction(nameof(ApiAdminDashboard));
            }

            if (string.IsNullOrWhiteSpace(vm.TargetEmail))
            {
                TempData["ApiAdmin_AlertMessage"] = "El correo objetivo es requerido.";
                return RedirectToOperationPage(vm.Operacion);
            }

            try
            {
                HttpResponseMessage resp;

                switch ((vm.Operacion ?? string.Empty).Trim())
                {
                    case "crear":
                        if (string.IsNullOrWhiteSpace(vm.NewPassword))
                        {
                            TempData["ApiAdmin_AlertMessage"] = "La contraseña es requerida para crear usuario.";
                            return RedirectToAction(nameof(ApiAdminCrearUsuario));
                        }

                        var rol = TryParseInt(vm.TargetRole, 0);
                        resp = await _mrAdminApi.CreateUserAsync(baseUrl, token, vm.TargetEmail, vm.NewPassword, rol, ct);
                        break;

                    case "cambiar-contrasena":
                        if (string.IsNullOrWhiteSpace(vm.NewPassword))
                        {
                            TempData["ApiAdmin_AlertMessage"] = "La nueva contraseña es requerida.";
                            return RedirectToAction(nameof(ApiAdminCambiarContrasena));
                        }
                        resp = await _mrAdminApi.ChangePasswordAsync(baseUrl, token, vm.TargetEmail, vm.NewPassword, ct);
                        break;

                    case "cambiar-status":
                        var status = vm.TargetStatus.HasValue ? (vm.TargetStatus.Value ? 1 : 0) : 0;
                        resp = await _mrAdminApi.ChangeStatusAsync(baseUrl, token, vm.TargetEmail, status, ct);
                        break;

                    case "cambiar-rol":
                        var newRol = TryParseInt(vm.TargetRole, 0);
                        resp = await _mrAdminApi.ChangeRoleAsync(baseUrl, token, vm.TargetEmail, newRol, ct);
                        break;

                    case "eliminar":
                        resp = await _mrAdminApi.DeleteUserAsync(baseUrl, token, vm.TargetEmail, ct);
                        break;

                    default:
                        TempData["ApiAdmin_AlertMessage"] = "Operación no soportada.";
                        return RedirectToAction(nameof(ApiAdminDashboard));
                }

                var body = await resp.Content.ReadAsStringAsync(ct);
                var messageOnly = ExtractMensaje(body);

                TempData["ApiAdmin_AlertMessage"] = string.IsNullOrWhiteSpace(messageOnly)
                    ? (resp.IsSuccessStatusCode ? "Operación realizada." : $"Error {(int)resp.StatusCode}: {resp.ReasonPhrase}")
                    : messageOnly;

                return RedirectToOperationPage(vm.Operacion);
            }
            catch (Exception ex)
            {
                TempData["ApiAdmin_AlertMessage"] = "Error: " + ex.Message;
                return RedirectToOperationPage(vm.Operacion);
            }
        }

        private IActionResult RedirectToOperationPage(string? operacion)
        {
            return (operacion ?? string.Empty).Trim() switch
            {
                "crear" => RedirectToAction(nameof(ApiAdminCrearUsuario)),
                "cambiar-contrasena" => RedirectToAction(nameof(ApiAdminCambiarContrasena)),
                "cambiar-status" => RedirectToAction(nameof(ApiAdminCambiarStatus)),
                "cambiar-rol" => RedirectToAction(nameof(ApiAdminCambiarRol)),
                "eliminar" => RedirectToAction(nameof(ApiAdminEliminarUsuario)),
                _ => RedirectToAction(nameof(ApiAdminDashboard))
            };
        }

        private static int TryParseInt(string? value, int fallback)
            => int.TryParse(value, out var n) ? n : fallback;

        private string BuildApiBaseUrl(string entorno)
        {
            const string origin = "https://expomr.online";

            var suffix = entorno.Trim().ToUpperInvariant() switch
            {
                "DEV" => "/apidev",
                "TEST" => "/apitest",
                "GDL" => "/apigdl",
                "QRO" => "/apiqro",
                "MX" => "/apimx",
                _ => throw new InvalidOperationException("Entorno no válido.")
            };

            return origin.TrimEnd('/') + suffix;
        }

        private static string? ExtractMensaje(string? body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("mensaje", out var m))
                        return m.GetString();
                    if (root.TryGetProperty("message", out var m2))
                        return m2.GetString();
                }

                return body;
            }
            catch
            {
                return body;
            }
        }
    }
}