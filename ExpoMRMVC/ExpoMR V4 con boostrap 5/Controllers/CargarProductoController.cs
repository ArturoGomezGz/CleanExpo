using ExpoMR.Datos;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Cliente,ClienteVIP")]
    public class CargarProductoController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private static IConfiguration _configuration;

        //Variables para verificar La sesión Y el Usuario
        public CargarProductoController(UserManager<IdentityUser> userManager, IConfiguration configuration)
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
            else
            {
                var userId = _userManager.GetUserId(User); // Obtiene el ID del usuario actualmente autenticado
                var user = await _userManager.FindByIdAsync(userId); // Encuentra el usuario por su ID
                var roles = await _userManager.GetRolesAsync(user);

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
                        return View();
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
                            return View();
                        }
                        else
                        {
                            return RedirectToAction("Index", "MiCuenta");
                        }
                    }
                }
            }
        }

        public async Task<IActionResult> CargarProducto()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "MiCuenta");
            }
            else
            {
                var userId = _userManager.GetUserId(User); // Obtiene el ID del usuario actualmente autenticado
                var user = await _userManager.FindByIdAsync(userId); // Encuentra el usuario por su ID
                var roles = await _userManager.GetRolesAsync(user);

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
                        return View();
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
                            return View();
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
        public async Task<bool> ProcesarArchivo(IFormFile formFile, bool borrarCarrito, string gRecaptchaResponse)
        {
            HttpClient clienteRecaptcha = new HttpClient();

            // LocalHost
            string claveSecreta = "6Ld2eHwpAAAAAM2u7KZeACkePwn3-cDiLRUoGZG0";

            // https://expomr.online/
            //string claveSecreta = "6LdOQncoAAAAAKxSu3g2fXZF1G07cGW4Fg6PZjTF";

            // https://expomr.com.mx/
            //string claveSecreta = "6LcEQXcoAAAAAAg-hlXHDu0W3rc4nUxsFzA4DUkW";

            string respuesta = await clienteRecaptcha.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret={claveSecreta}&response={gRecaptchaResponse}");

            RecaptchaResponse resultado = JsonConvert.DeserializeObject<RecaptchaResponse>(respuesta);

            if (resultado.Success)
            {
                if (formFile != null && formFile.Length > 0)
                {
                    var dbExpo = new ApplicationDbContextExpo();
                    var user = _userManager.GetUserAsync(User);
                    string username = user.Result.UserName.ToString();
                    int IdPersona = GeneralesController.GetIdPersonas(username);
                    int cliente = GeneralesController.GetClientes(username);

                    List<SqlParameter> parms = new List<SqlParameter>
                {
                    new SqlParameter { ParameterName = "@cliente", Value = cliente },
                    new SqlParameter { ParameterName = "@usuario", Value = IdPersona }
                };

                    var resultadoParameter = new SqlParameter("@success", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };

                    if (borrarCarrito)
                    {
                        dbExpo.Database.ExecuteSqlRaw($"EXEC CotizarEliminar @cliente = {parms[0].Value}, @usuario = {parms[1].Value}");
                    }

                    dbExpo.Database.ExecuteSqlRaw($"EXEC @success = CotizarGetCotizacion @cliente = {parms[0].Value}, @usuario = {parms[1].Value}", resultadoParameter);

                    int cotizacion = (int)resultadoParameter.Value;

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
                                dbExpo.Database
                                    .ExecuteSqlRaw($"EXEC CotizarAgregarMasivo @cliente = {parms[0].Value}, @cotizacion = {cotizacion}, @producto = {values[0]}, @cantidad = {values[1]}");
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