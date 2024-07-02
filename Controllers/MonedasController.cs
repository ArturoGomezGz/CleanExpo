using ExpoMR.Datos;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Cliente,ClienteVIP,Proveedor")]
    public class MonedasController : Controller
    {
        //Variables para verificar La sesión Y el Usuario
        private readonly UserManager<IdentityUser> _userManager;

        public MonedasController(UserManager<IdentityUser> userManager)
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
                int isFechaExpoVIP = ExpoMR.Clases.WebSite.ValidaExpoVIP();

                if (ExpoMR.Clases.WebSite.ValidaRolUsuario(currentUser.Email, "ClienteVIP") == true)
                {
                    if (isFechaExpoVIP == 1) // Expo VIP
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
                    if (isFechaExpoVIP == 0) //0 = Expo Normal
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
}