#define TESTMODE 
using ExpoMR.Models;
using ExpoMR.Clases;
using ExpoMR.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

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
            _rolManager = rolManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        //Vista del La Página de Registro
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(string returnUrl = null)
        {
            //Para la creación de Roles
            if (!await _rolManager.RoleExistsAsync("Administrador")) {

                await _rolManager.CreateAsync(new IdentityRole("Administrador"));
            } 
            //Para la creación de Roles
            if (!await _rolManager.RoleExistsAsync("Cliente")) {

                await _rolManager.CreateAsync(new IdentityRole("Cliente"));
            }

            ViewData["ReturnUrl"] = returnUrl;
            RegistroViewModel RegistroVM = new RegistroViewModel();
            return View(RegistroVM);
        }

        //Formulario para el registro en la aplicación
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel rgViewModel, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl = returnUrl ?? Url.Content("~/");
            await _rolManager.CreateAsync(new IdentityRole("Cliente"));
            if (ModelState.IsValid)
            {
                var usuario = new AppUsuario
                {
                    UserName = rgViewModel.Email,
                    Email = rgViewModel.Email,
                    //Telefono = rgViewModel.Telefono,
                    //Nombre = rgViewModel.Nombre,
                    //Ciudad = rgViewModel.Ciudad,
                    Estado = rgViewModel.Estado
                };
                var resultado = await _userManager.CreateAsync(usuario, rgViewModel.Password);

                //await _signInManager.SignInAsync(usuario, isPersistent: false);
                if (resultado.Succeeded)
                {
                    //Se asigna el Rol Por Default
                    await _userManager.AddToRoleAsync(usuario, "Cliente");

                    //Implementación de configuracion de email en el registro
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(usuario);

                    var urlRetorno = Url.Action("ConfirmarEmail", "Cuentas", new { userId = usuario.Id, code = code }, protocol: HttpContext.Request.Scheme);

#if (TESTMODE)
                    await _emailSender.SendEmailAsync("pruebas@medicinasrosario.com.mx", "Confirmar su cuenta - ExpoMR",
                                            "Por favor confirme su cuenta dando click aquí: <a href=\"" + urlRetorno + "\"> enlace </a>");
                    //await _signInManager.SignInAsync(usuario, isPersistent: false);
#else
                    
                    await _emailSender.SendEmailAsync(rgViewModel.Email, "Confirmar su cuenta - ExpoMR",
                        "Por favor confirme su cuenta dando click aquí: <a href=\""+urlRetorno+"\"> enlace </a>");                    
                    await _signInManager.SignInAsync(usuario, isPersistent: false);
#endif
//                  return LocalRedirect(returnUrl);
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
            List<SelectListItem> listaRoles = new List<SelectListItem>();
            listaRoles.Add(new SelectListItem()
            {
                Value = "Cliente",
                Text = "Cliente"
            });
            listaRoles.Add(new SelectListItem()
            {
                Value = "Administrador",
                Text = "Administrador"
            });

            ViewData["ReturnUrl"] = returnUrl;
            RegistroViewModel RegistroVM = new RegistroViewModel() { 
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
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var usuario = new AppUsuario
                {
                    UserName = rgViewModel.Email,
                    Email = rgViewModel.Email,
                    Telefono = rgViewModel.Telefono,
                    Nombre = rgViewModel.Nombre,
                    Ciudad = rgViewModel.Ciudad,
                    Estado = rgViewModel.Estado
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

                    var urlRetorno = Url.Action("ConfirmarEmail", "Cuentas", new { userId = usuario.Id, code = code }, protocol: HttpContext.Request.Scheme);

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
            List<SelectListItem> listaRoles = new List<SelectListItem>();
            listaRoles.Add(new SelectListItem()
            {
                Value = "Cliente",
                Text = "Cliente"
            });

            listaRoles.Add(new SelectListItem()
            {
                Value = "Administrador",
                Text = "Administrador"
            });

            rgViewModel.ListaRoles = listaRoles;

            return View(rgViewModel);
        }

        ////Generar Lista de errores
        ///
        [AllowAnonymous]
        private void ValidarErrores(IdentityResult resultado)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(String.Empty, error.Description);
            }
        }

        ////Mostrar formulario de acceso
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Acceso(String returnUrl = null)
        {           
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        ////Login de la aplicación
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Acceso(AccesoViewModel accViewModel, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var resultado = await _signInManager.PasswordSignInAsync(accViewModel.Email, accViewModel.Password, accViewModel.RememberMe, lockoutOnFailure: true);

                if (resultado.Succeeded)
                {
                    HttpRequest request = HttpContext.Request;
                    bool SaveSession = Log.SaveSesion(request, accViewModel.Email, true);

                    var identity = new ClaimsIdentity(IdentityConstants.ApplicationScheme);
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "IdUser"));
                    identity.AddClaim(new Claim(ClaimTypes.Name, "UserName"));
                    //await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme,
                    //    new ClaimsPrincipal(identity));

                    return Redirect("https://localhost:44304/Login.aspx");
                    //return LocalRedirect(returnUrl);
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

        //Metodo para Olvido de contraseña
        [HttpGet]
        [AllowAnonymous]
        public IActionResult OlvidoPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> OlvidoPassword(OlvidoPasswordViewModel opViewModel)
        {
            if (ModelState.IsValid) { 
                var usuario = await _userManager.FindByEmailAsync(opViewModel.Email);
                if (usuario == null) {
                    return RedirectToAction("ConfirmacionOlvidoPassword");
                }

                var codigo = await _userManager.GeneratePasswordResetTokenAsync(usuario);

                var urlRetorno = Url.Action("ResetPassword", "Cuentas", new { userId = usuario.Id, code = codigo }, protocol: HttpContext.Request.Scheme);

#if (TESTMODE)
                await _emailSender.SendEmailAsync("pruebas@medicinasrosario.com.mx", "Recuperar Contraseña ExpoMR",
                     "Por favor recupre su contraseña entrando en el siguiente enlace: <a href=\"" + urlRetorno + "\"> Enlace </a>");
#else
                    
                    await _emailSender.SendEmailAsync(opViewModel.Email, "Recuperar Contraseña ExpoMR", 
                    "Por favor recupre su contraseña entrando en el siguiente enlace: <a href=\"" + urlRetorno + "\"> Enlace </a>");
#endif


                return RedirectToAction("ConfirmacionOlvidoPassword");
            }  

            return View(opViewModel);
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
        public IActionResult ResetPassword(string code=null)
        {
            return code == null ? View("Error") : View();
        }

        //REcuperar Contraseña Funcionalidad
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

        //Vista Confirmación Password
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ConfirmacionRecuperaPassword()
        {
            return View();
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

            if (usuario == null) {
                return View("Error");
            }

            var resultado = await _userManager.ConfirmEmailAsync(usuario, code);

            return View(resultado.Succeeded ? "ConfirmarEmail": "Error");
        }

        //Acceso denegado
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Denegado(String returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl = returnUrl ?? Url.Content("~/");
            return View();
        }
        [AllowAnonymous]
        private void ValidarErroresCuenta(IdentityResult resultado)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(String.Empty, error.Description);
            }
        }

    }
}
