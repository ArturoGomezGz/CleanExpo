#define TESTMODE

using ExpoMR.Models;
using ExpoMR.Clases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExpoMR.Datos;
using Microsoft.Data.SqlClient;
using Nancy.Session;

namespace ExpoMR.Controllers
{
    [Authorize]
    public class CuentasController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _rolManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;

        //Variables para verificar La sesión Y el Usuario
        public CuentasController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender, RoleManager<IdentityRole> rolManager)
        {
            _userManager = userManager;
            _rolManager = rolManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        ////Mostrar formulario de acceso
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Acceso(String returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        //Metodo para Olvido de contraseña
        [HttpGet]
        [AllowAnonymous]
        public IActionResult OlvidoPassword()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ConfirmacionOlvidoPassword()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegistroConfirmacion()
        {
            return View();
        }

        //Recuperar Contraseña Vista
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        //Vista Confirmación Password
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ConfirmacionRecuperaPassword()
        {
            return View();
        }

        //Acceso denegado
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Denegado(String returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            _ = returnUrl ?? Url.Content("~/");

            return View();
        }

        //Vista del La Página de Registro
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(string returnUrl = null)
        {
            //Para la creación de Roles
            if (!await _rolManager.RoleExistsAsync("Administrador"))
            {
                await _rolManager.CreateAsync(new IdentityRole("Administrador"));
            }

            //Para la creación de Roles
            if (!await _rolManager.RoleExistsAsync("Cliente")) 
            {
                await _rolManager.CreateAsync(new IdentityRole("Cliente"));
            }

            ViewData["ReturnUrl"] = returnUrl;
            RegistroViewModel RegistroVM = new();
            return View(RegistroVM);
        }

        //Formulario para el registro en la aplicación
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel rgViewModel, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl ??= Url.Content("~/");
            await _rolManager.CreateAsync(new IdentityRole("Cliente"));

            if (ModelState.IsValid)
            {
                var usuario = new AppUsuario
                {
                    UserName = rgViewModel.Email,
                    Email = rgViewModel.Email
                };

                var resultado = await _userManager.CreateAsync(usuario, rgViewModel.Password);

                if (resultado.Succeeded)
                {
                    //Se asigna el Rol Por Default
                    await _userManager.AddToRoleAsync(usuario, "Cliente");

                    //Implementación de configuracion de email en el registro
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(usuario);

                    var urlRetorno = Url.Action("ConfirmarEmail", "Cuentas", new { userId = usuario.Id, code }, protocol: HttpContext.Request.Scheme);

                #if (TESTMODE)
                    await _emailSender.SendEmailAsync("pruebas@medicinasrosario.com.mx", "Confirmar su cuenta - ExpoMR",
                                            "Por favor confirme su cuenta dando click aquí: <a href=\"" + urlRetorno + "\"> enlace </a>");
                #else                   
                    await _emailSender.SendEmailAsync(rgViewModel.Email, "Confirmar su cuenta - ExpoMR",
                        "Por favor confirme su cuenta dando click aquí: <a href=\""+urlRetorno+"\"> enlace </a>");                    
                    await _signInManager.SignInAsync(usuario, isPersistent: false);
                #endif

                    return RedirectToAction("RegistroConfirmacion");
                }

                ValidarErrores(resultado);
            }

            return View(rgViewModel);
        }

        //Registro para administradores
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> RegistroAdministrador(string returnUrl = null)
        {
            //Para la creación de Roles
            if (!await _rolManager.RoleExistsAsync("Administrador"))
            {
                await _rolManager.CreateAsync(new IdentityRole("Administrador"));
            }

            //Para la creación de Roles
            if (!await _rolManager.RoleExistsAsync("Cliente"))
            {
                await _rolManager.CreateAsync(new IdentityRole("Cliente"));
            }

            //para selección de roles
            List<SelectListItem> listaRoles = new()
            {
                new SelectListItem()
                {
                    Value = "Cliente",
                    Text = "Cliente"
                },
                new SelectListItem()
                {
                    Value = "Administrador",
                    Text = "Administrador"
                }
            };

            ViewData["ReturnUrl"] = returnUrl;
            RegistroViewModel RegistroVM = new()
            { 
                ListaRoles = listaRoles
            };

            return View(RegistroVM);
        }

        //Formulario para el registro en la aplicación
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registroadministrador(RegistroViewModel rgViewModel, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var usuario = new AppUsuario
                {
                    UserName = rgViewModel.Email,
                    Email = rgViewModel.Email
                };

                var resultado = await _userManager.CreateAsync(usuario, rgViewModel.Password);

                await _signInManager.SignInAsync(usuario, isPersistent: false);

                if (resultado.Succeeded)
                {
                    //para Seleccionar rol
                    if (rgViewModel.RolSeleccionado != null && rgViewModel.RolSeleccionado.Length > 0) {
                        await _userManager.AddToRoleAsync(usuario, rgViewModel.RolSeleccionado);
                    }
                    //Se asigna el Rol Por Default
                    await _userManager.AddToRoleAsync(usuario, "Cliente");

                    //Implementación de configuracion de email en el registro
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(usuario);

                    var urlRetorno = Url.Action("ConfirmarEmail", "Cuentas", new { userId = usuario.Id, code }, protocol: HttpContext.Request.Scheme);

                #if (TESTMODE)
                    await _emailSender.SendEmailAsync("pruebas@medicinasrosario.com.mx", "Confirmar su cuenta - ExpoMR",
                                            "Por favor confirme su cuenta dando click aquí: <a href=\"" + urlRetorno + "\"> enlace </a>");
                    await _signInManager.SignInAsync(usuario, isPersistent: false);
                #else
                    
                    await _emailSender.SendEmailAsync(rgViewModel.Email, "Confirmar su cuenta - ExpoMR",
                        "Por favor confirme su cuenta dando click aquí: <a href=\""+urlRetorno+"\"> enlace </a>");                    
                    await _signInManager.SignInAsync(usuario, isPersistent: false);
                #endif                   
                    
                    return LocalRedirect(returnUrl);
                }

                ValidarErrores(resultado);
            }

            //para selección de roles
            List<SelectListItem> listaRoles = new()
            {
                new SelectListItem()
                {
                    Value = "Cliente",
                    Text = "Cliente"
                },

                new SelectListItem()
                {
                    Value = "Administrador",
                    Text = "Administrador"
                }
            };

            rgViewModel.ListaRoles = listaRoles;

            return View(rgViewModel);
        }

        ////Login de la aplicación
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Acceso(AccesoViewModel accViewModel, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl ??= Url.Content("~/");
            HttpContext.Session.Remove("ClienteSel"); //Se elimina la variable de session que almacena el idpersona del cliente seleccionado del control ddlClientes en la vista _loginUser

            if (ModelState.IsValid)
            {
                var resultado = await _signInManager.PasswordSignInAsync(accViewModel.Email, accViewModel.Password, accViewModel.RememberMe, lockoutOnFailure: true);
				var currentUser = await _userManager.FindByNameAsync(accViewModel.Email);

				if (resultado.Succeeded)
                {
                    if (!currentUser.EmailConfirmed)
                    {
						return RedirectToAction("Index", "MiCuenta");
					}
                    else
                    {
						//Se inicializa la variable de session "ClienteSeleccionado" con el primer cliente que tenga el email
						GetClientes(accViewModel.Email);

						HttpRequest request = HttpContext.Request;
						Log.SaveSesion(request, accViewModel.Email, true);
						var user = await _userManager.FindByNameAsync(accViewModel.Email);

						if (await _userManager.IsInRoleAsync(user, "Administrador"))
						{
							return LocalRedirect("~/Administracion/Usuarios");
						}

						if (await _userManager.IsInRoleAsync(user, "Vendedor"))
						{
							return LocalRedirect("~/Empleados/Cotizacion");
						}

						if (await _userManager.IsInRoleAsync(user, "Proveedor"))
						{
							return LocalRedirect("~/Estadisticas/Estadisticas");
						}

						if (await _userManager.IsInRoleAsync(user, "Capturista"))
						{
							return LocalRedirect("~/Empleados/Cotizacion");
						}

						return LocalRedirect(returnUrl);
					}
                }
                if (resultado.IsLockedOut)
                {
                    return View("Bloqueado");
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Acceso incorrecto!! Verifique su Contraseña o Correo Electronico estan bien escritos");

                    return View(accViewModel);
                }
            }

            return View(accViewModel);
        }

        //LogOut de la aplicación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalirAplicacion()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        //Recuperar Contraseña Funcionalidad
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(RecuperaPasswordViewModel rpViewModel)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _userManager.FindByEmailAsync(rpViewModel.Email);

                if (usuario == null)
                {
                    return RedirectToAction("ConfirmacionRecuperaPassword");
                }

                var resultado = await _userManager.ResetPasswordAsync(usuario, rpViewModel.Code, rpViewModel.Password);

                if (resultado.Succeeded)
                {
                    return RedirectToAction("ConfirmacionRecuperaPassword");
                }

                ValidarErrores(resultado);
            }

            return View(rpViewModel);
        }

        //Metodo para Email
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmarEmail(string userId, string code)
        {
            if(userId ==null || code == null)
            {
                return View("Error");
            }

            var usuario = await _userManager.FindByIdAsync(userId);

            if (usuario == null) 
            {
                return View("Error");
            }

            var resultado = await _userManager.ConfirmEmailAsync(usuario, code);

            return View(resultado.Succeeded ? "ConfirmarEmail": "Error");
        }

        [AllowAnonymous]
        private void ValidarErroresCuenta(IdentityResult resultado)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(String.Empty, error.Description);
            }
        }

        ////Generar Lista de errores
        [AllowAnonymous]
        private void ValidarErrores(IdentityResult resultado)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(String.Empty, error.Description);
            }
        }

        public void GetClientes(string UserName)
        {
            string query = "SELECT Persona.IdPersona, Persona.Nombre FROM GetPersonasByEmail(@UserName) AS Persona";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@UserName", UserName);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: System.Data.CommandType.Text);

            while (dr.Read())
            {
                if (HttpContext.Session.GetString("ClienteSel") == null)
                {
                    HttpContext.Session.SetString("ClienteSel", dr["IdPersona"].ToString());

                    break;
                }
            }
        }
    }
}