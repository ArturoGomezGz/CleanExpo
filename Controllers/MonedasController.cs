using ExpoMR.Datos;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Cliente,Proveedor")]
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
                int isFechaExpo = ExpoMR.Clases.WebSite.ValidaDiaExpo();

                if (isFechaExpo == 1) //1 = Dia Expo
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