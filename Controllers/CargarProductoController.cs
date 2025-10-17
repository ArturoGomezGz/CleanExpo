using ExpoMR.Clases;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Cliente,ClienteVIP")]
    public class CargarProductoController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        //Variables para verificar La sesión Y el Usuario
        public CargarProductoController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "MiCuenta");
            }
            else
            {
                int isFechaExpoVIP = WebSite.ValidaExpoVIP();

                if (WebSite.ValidaRolUsuario(currentUser.Email, "ClienteVIP") == true)
                {
                    if (isFechaExpoVIP == 1) //1 = Expo VIP
                    {
                        return View();
                    }
                    else //0 = Expo Normal; 2 = El dia no es de expo
                    {
                        return RedirectToAction("Index", "MiCuenta");
                    }
                }
                else
                {
                    if (isFechaExpoVIP == 0) //0 = Expo Normal
                    {
                        return View();
                    }
                    else //1 = Expo VIP; 2 = El dia no es de expo
                    {
                        return RedirectToAction("Index", "MiCuenta");
                    }
                }
            }
        }

        [HttpPost]
        public async Task<bool> ProcesarArchivo(IFormFile formFile, bool borrarCarrito, string gRecaptchaResponse)
        {
            HttpClient clienteRecaptcha = new HttpClient();

            // LocalHost
            string claveSecreta = "6Ld2eHwpAAAAAM2u7KZeACkePwn3-cDiLRUoGZG0";

            // https://expomr.com.mx/
            //string claveSecreta = "6LcEQXcoAAAAAAg-hlXHDu0W3rc4nUxsFzA4DUkW";

            // https://expomr.online/
            //string claveSecreta = "6LdOQncoAAAAAKxSu3g2fXZF1G07cGW4Fg6PZjTF";

            string respuesta = await clienteRecaptcha.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret={claveSecreta}&response={gRecaptchaResponse}");

            RecaptchaResponse resultado = JsonConvert.DeserializeObject<RecaptchaResponse>(respuesta);

            if (resultado.Success)
            {
                if (formFile != null && formFile.Length > 0)
                {
                    DB dB = new();
                    SqlParameter[] parameters = new SqlParameter[2];
                    parameters[0] = new SqlParameter("@cliente", HttpContext.Session.GetString("ClienteSel"));
                    parameters[1] = new SqlParameter("@idemail", HttpContext.Session.GetString("Session_IdEmail"));

                    if (borrarCarrito)
                    {
                        dB.ExecuteNonQuery("CotizarEliminar", ref parameters);
                    }

                    dB = new();
                    parameters = new SqlParameter[2];
                    parameters[0] = new SqlParameter("@cliente", HttpContext.Session.GetString("ClienteSel"));
                    parameters[1] = new SqlParameter("@idemail", HttpContext.Session.GetString("Session_IdEmail"));

                    int cotizacion = dB.ExecuteReturnValue("CotizarGetCotizacion", ref parameters);

                    using (var reader = new StreamReader(formFile.OpenReadStream()))
                    {                        
                        string line;
                        int n = 0;
                        string[] values = new string[2];
                        int v;
                        while ((line = reader.ReadLine()) != null && n < 700)
                        {
                            values = line.Split(',');
                            if (values.Count() >= 2 && int.TryParse(values[0], out v) && (int.TryParse(values[1], out v)))
                            {
                                dB = new DB();
                                parameters = new SqlParameter[5];
                                parameters[0] = new SqlParameter("@cliente", HttpContext.Session.GetString("ClienteSel"));
                                parameters[1] = new SqlParameter("@cotizacion", cotizacion);
                                parameters[2] = new SqlParameter("@producto", values[0]);
                                parameters[3] = new SqlParameter("@cantidad", values[1]);
                                parameters[4] = new SqlParameter("@idemail", HttpContext.Session.GetString("Session_IdEmail"));

                                dB.ExecuteNonQuery("CotizarAgregarMasivo", ref parameters);
                            }

                            n++;
                        }
                        if (n > 0)
                        {
                            return true;
                        }
                    }

                }
            }
            return false;
        }
    }
}