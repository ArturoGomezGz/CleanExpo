using ExpoMR.Controllers;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using System.Data;
using System.Diagnostics;

namespace ExpoMR.Controllers
{
    [Authorize]
    public class MonedasController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _rolManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private static IConfiguration _configuration;

        //Variables para verificar La sesión Y el Usuario
        public MonedasController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender, RoleManager<IdentityRole> rolManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _rolManager = rolManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _rolManager = rolManager;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "MiCuenta");
            }
            return View();
        }
    }
}
