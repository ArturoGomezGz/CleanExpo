using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ExpoMR.Models;
using ExpoMR.Servicios;

namespace ExpoMR.Controllers
{
    public class CuentasController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;


        //Variables para verificar La sesión Y el Usuario
        public CuentasController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        //Vista del La Página de Registro
        [HttpGet]
        public async Task<IActionResult> Registro(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            RegistroViewModel RegistroVM = new RegistroViewModel();
            return View(RegistroVM);
        }

        //Formulario para el registro en la aplicación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel rgViewModel, string returnUrl = null)
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
                    await _emailSender.SendEmailAsync(rgViewModel.Email, "Confirmar su cuenta - Proyecto Identity",
                        "Por favor confirme su cuenta dando click aquí: <a href=''> enlace </a>");                    
                    await _signInManager.SignInAsync(usuario, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                ValidarErrores(resultado);
            }

            return View(rgViewModel);
        }

        ////Generar Lista de errores
        private void ValidarErrores(IdentityResult resultado)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(String.Empty, error.Description);
            }
        }

        ////Mostrar formulario de acceso
        [HttpGet]
        public IActionResult Acceso(String returnUrl = null)
        {

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        ////Login de la aplicación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Acceso(AccesoViewModel accViewModel, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var resultado = await _signInManager.PasswordSignInAsync(accViewModel.Email, accViewModel.Password, accViewModel.RememberMe, lockoutOnFailure: true);

                if (resultado.Succeeded)
                {
                    //return RedirectToAction("Index", "Home");
                    return LocalRedirect(returnUrl);
                }
                if (resultado.IsLockedOut)
                {
                    return View("Bloqueado");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Acceso Invalido");
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
        public IActionResult OlvidoPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OlvidoPassword(OlvidoPasswordViewModel opViewModel)
        {
            if (ModelState.IsValid) { 
                var usuario = await _userManager.FindByEmailAsync(opViewModel.Email);
                if (usuario == null) {
                    return RedirectToAction("ConfirmacionOlvidoPassword");
                }

                var codigo = await _userManager.GeneratePasswordResetTokenAsync(usuario);

                var urlRetorno = Url.Action("ResetPassword", "Cuentas", new { userId = usuario.Id, code = codigo }, protocol: HttpContext.Request.Scheme);
                await _emailSender.SendEmailAsync(opViewModel.Email, "Recuperar Contraseña ExpoMR", 
                    "Por favor recupre su contraseña entrando en el siguiente enlace: <a href=\"" + urlRetorno + "\"> Enlace </a>");

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

        //Recuperar Contraseña Vista
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword( string code=null)
        {
            return code == null ? View("Error") : View();
        }

        //REcuperar Contraseña Funcionalidad
        [HttpPost]
        [ValidateAntiForgeryToken]
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

    }
}
