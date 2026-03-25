using ExpoMR.Models;
using ExpoMR.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace ExpoMR.Controllers
{
    public class MrApiAdminController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly MrAdminApiService _mrAdminApi;

        public MrApiAdminController(IConfiguration configuration, MrAdminApiService mrAdminApi)
        {
            _configuration = configuration;
            _mrAdminApi = mrAdminApi;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ApiAdminLogin()
        {
            if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString("ApiAdmin_Token")))
                return RedirectToAction(nameof(ApiAdminDashboard));

            var vm = new ApiAdminPanelViewModel
            {
                Entorno = HttpContext.Session.GetString("ApiAdmin_Entorno"),
                Usuario = HttpContext.Session.GetString("ApiAdmin_Usuario"),
                IsAuthenticated = false,
                Mensaje = TempData["ApiAdmin_AlertMessage"] as string
            };

            ViewBag.ApiEnvironments = GetApiEnvironmentOptions();
            return View(vm);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ApiAdminPanel()
        {
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
            {
                ViewBag.ApiEnvironments = GetApiEnvironmentOptions();
                return View("ApiAdminLogin", vm);
            }

            string baseUrl;
            try
            {
                baseUrl = BuildApiBaseUrl(vm.Entorno);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(nameof(vm.Entorno), ex.Message);
                ViewBag.ApiEnvironments = GetApiEnvironmentOptions();
                return View("ApiAdminLogin", vm);
            }

            try
            {
                var token = await _mrAdminApi.GetTokenAsync(baseUrl, vm.Usuario!, vm.Password!, ct);

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
            catch (HttpRequestException httpEx)
            {
                vm.IsAuthenticated = false;
                vm.Mensaje = httpEx.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized or
                    System.Net.HttpStatusCode.BadRequest  => "Usuario o contraseña incorrectos.",
                    System.Net.HttpStatusCode.Forbidden   => "Tu cuenta no tiene acceso al panel de administración.",
                    System.Net.HttpStatusCode.ServiceUnavailable or
                    System.Net.HttpStatusCode.BadGateway or
                    System.Net.HttpStatusCode.GatewayTimeout => "La API no está disponible en este momento. Intenta más tarde.",
                    null => "No fue posible conectar con la API. Verifica el entorno seleccionado.",
                    _    => "No se pudo autenticar. Intenta nuevamente."
                };
            }
            catch (TaskCanceledException)
            {
                vm.IsAuthenticated = false;
                vm.Mensaje = "La solicitud tardó demasiado tiempo. Verifica tu conexión e intenta nuevamente.";
            }
            catch
            {
                vm.IsAuthenticated = false;
                vm.Mensaje = "Ocurrió un error inesperado al autenticar. Intenta nuevamente.";
            }

            vm.Password = null;
            ViewBag.ApiEnvironments = GetApiEnvironmentOptions();
            return View("ApiAdminLogin", vm);
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

                // Sesión expirada: limpiar y redirigir al login
                if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Remove("ApiAdmin_Token");
                    TempData["ApiAdmin_AlertMessage"] = "Tu sesión expiró. Vuelve a iniciar sesión.";
                    return RedirectToAction(nameof(ApiAdminLogin));
                }

                TempData["ApiAdmin_AlertMessage"] = InterpretarRespuestaApi(resp, body, vm.Operacion);

                return RedirectToOperationPage(vm.Operacion);
            }
            catch (HttpRequestException)
            {
                TempData["ApiAdmin_AlertMessage"] = "No fue posible conectar con la API. Verifica tu conexión e intenta nuevamente.";
                return RedirectToOperationPage(vm.Operacion);
            }
            catch (TaskCanceledException)
            {
                TempData["ApiAdmin_AlertMessage"] = "La solicitud tardó demasiado tiempo. Intenta nuevamente.";
                return RedirectToOperationPage(vm.Operacion);
            }
            catch
            {
                TempData["ApiAdmin_AlertMessage"] = "Ocurrió un error inesperado. Intenta nuevamente.";
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

        private List<ApiEnvironmentOption> GetApiEnvironmentOptions()
        {
            var options = new List<ApiEnvironmentOption>();

            var section = _configuration.GetSection("ApiUrls");
            foreach (var child in section.GetChildren())
            {
                var key = child.Key?.Trim();
                if (string.IsNullOrWhiteSpace(key)) continue;

                var displayName = child["DisplayName"];
                var baseUrl = child["BaseUrl"];

                if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(baseUrl))
                    continue;

                options.Add(new ApiEnvironmentOption
                {
                    Key = key,
                    DisplayName = displayName,
                    BaseUrl = baseUrl.TrimEnd('/')
                });
            }

            return options;
        }

        private string BuildApiBaseUrl(string entorno)
        {
            if (string.IsNullOrWhiteSpace(entorno))
                throw new InvalidOperationException("Entorno no válido.");

            var key = entorno.Trim().ToUpperInvariant();
            var url = _configuration.GetSection($"ApiUrls:{key}")["BaseUrl"];
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidOperationException($"No hay URL configurada para el entorno '{key}'.");

            return url.TrimEnd('/');
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

        private static int TryParseInt(string? value, int fallback)
            => int.TryParse(value, out var n) ? n : fallback;

        /// <summary>
        /// Traduce la respuesta HTTP de la API a un mensaje amigable para el usuario.
        /// Nunca expone códigos de estado, nombres técnicos ni JSON crudo.
        /// </summary>
        private static string InterpretarRespuestaApi(HttpResponseMessage resp, string? body, string? operacion)
        {
            if (resp.IsSuccessStatusCode)
            {
                // Preferir el mensaje que manda la API si es claro
                var apiMsg = ExtraerCampoMensaje(body);
                if (!string.IsNullOrWhiteSpace(apiMsg))
                    return apiMsg;

                return (operacion ?? string.Empty).Trim() switch
                {
                    "crear"             => "Usuario creado correctamente.",
                    "cambiar-contrasena"=> "Contraseña actualizada correctamente.",
                    "cambiar-status"    => "Estado del usuario actualizado correctamente.",
                    "cambiar-rol"       => "Rol del usuario actualizado correctamente.",
                    "eliminar"          => "Usuario eliminado correctamente.",
                    _                   => "Operación realizada correctamente."
                };
            }

            return (int)resp.StatusCode switch
            {
                403 => "No tienes permisos para realizar esta operación.",
                404 => "No se encontró ningún usuario con el correo indicado.",
                409 => "Ya existe un usuario registrado con ese correo.",
                422 => InterpretarError422(body),
                >= 500 => "Error en el servidor de la API. Intenta nuevamente en unos momentos.",
                _ => ExtraerCampoMensaje(body) ?? "Ocurrió un error al procesar la operación. Intenta nuevamente."
            };
        }

        /// <summary>
        /// Extrae únicamente el campo "mensaje" o "message" del JSON de respuesta.
        /// Devuelve null si no existe o si el cuerpo no es JSON válido (nunca devuelve JSON crudo).
        /// </summary>
        private static string? ExtraerCampoMensaje(string? body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                    return null;

                if (root.TryGetProperty("mensaje", out var m) && m.ValueKind == JsonValueKind.String)
                    return m.GetString();
                if (root.TryGetProperty("message", out var m2) && m2.ValueKind == JsonValueKind.String)
                    return m2.GetString();

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Genera un mensaje amigable para errores de validación 422.
        /// Intenta leer el array "detail" de FastAPI pero nunca lo expone directamente.
        /// </summary>
        private static string InterpretarError422(string? body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return "Los datos ingresados no son válidos. Verifica el correo y los campos requeridos.";

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                if (!root.TryGetProperty("detail", out var detail))
                    return "Los datos ingresados no son válidos. Verifica el correo y los campos requeridos.";

                // FastAPI devuelve detail como array de errores de validación
                if (detail.ValueKind == JsonValueKind.Array)
                {
                    var campos = detail.EnumerateArray()
                        .Select(e =>
                        {
                            // Obtener el nombre del campo (último elemento de "loc")
                            var campo = e.TryGetProperty("loc", out var loc) && loc.ValueKind == JsonValueKind.Array
                                ? loc.EnumerateArray().LastOrDefault().GetString()
                                : null;
                            return campo;
                        })
                        .Where(c => !string.IsNullOrWhiteSpace(c) && c != "body")
                        .Distinct()
                        .ToList();

                    if (campos.Count > 0)
                        return $"Los siguientes campos contienen datos inválidos: {string.Join(", ", campos)}. Verifica e intenta nuevamente.";
                }

                // detail como string directo
                if (detail.ValueKind == JsonValueKind.String)
                {
                    var msg = detail.GetString();
                    if (!string.IsNullOrWhiteSpace(msg))
                        return msg;
                }
            }
            catch { /* ignorar errores de parseo */ }

            return "Los datos ingresados no son válidos. Verifica el correo y los campos requeridos.";
        }
    }
}
