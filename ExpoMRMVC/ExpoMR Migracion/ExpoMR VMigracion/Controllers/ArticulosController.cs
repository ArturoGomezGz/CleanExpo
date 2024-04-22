using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Proveedor")]
    public class ArticulosController : Controller
    {
        public IActionResult Articulos()
        {
            return RedirectToAction("Proveedores", "Articulos");
            //return View();
        }
    }
}
