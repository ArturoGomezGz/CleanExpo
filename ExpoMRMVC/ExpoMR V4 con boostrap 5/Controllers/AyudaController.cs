using ExpoMR.Datos;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Cliente,ClienteVIP")]
    public class AyudaController : Controller
    {        
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Contacto() { 
            return View();
        }       
        public IActionResult Preguntas() {
            return View();
        }
        public IActionResult Privacidad() {
            return View();
        }
        public IActionResult CondicionesRuleta() {
            return View();
        }
        public IActionResult TerminosyCondiciones() {
            return View();
        }
        [HttpPost]
        public JsonResult GetSales(string letra, string palabra)
        {
            using var dbIndigo = new ApplicationDbContextIndigo();

            var result = dbIndigo.Set<SalesBuscar>()
                .FromSqlRaw("Select mSales.IdSal, mSales.Sal, " +
                " CAST(Case When COUNT(mProductos.IdProducto) = 0 Then 0 Else 1 End as bit) as Habilitado " +
                " From mSales inner join " +
                " dProdSal on mSales.IdSal = dProdSal.IdSal left outer join " +
                " ExpoMR.dbo.mProductos on dProdSal.IdProducto = mProductos.IdProducto " +
                " Where UPPER(LEFT(mSales.Sal, 1)) = UPPER( CASE WHEN '" + letra + "' = '' THEN NULL ELSE '" + letra + "' END) " +
                " Or mSales.Sal Like Case When '" + palabra + "' = '' Then Null Else '%" + palabra + "%' End" +
                " Group by mSales.IdSal, mSales.Sal Order by mSales.Sal").ToList();
            return Json(result);
        }

        [HttpPost]
        public JsonResult GetLaboratorioBySal(int Sal) {
            //using var dbExpo = new ApplicationDbContextExpo();
            //return Json(dbExpo.Set<StandImage>()
            //    .FromSqlRaw("Select mProveedores.IdPersona as Proveedor, mProveedores.Nombre," +
            //    " mStands.Logo" +
            //    " From mProveedores inner join" +
            //    " mLaboratorios on mProveedores.IdPersona = mLaboratorios.Proveedor inner join" +
            //    " mProductos on mLaboratorios.IdLaboratorio = mProductos.IdLaboratorio inner join" +
            //    " mStands on mStands.Proveedor = mProveedores.IdPersona inner join" +
            //    " Indigo.dbo.dProdsal on mProductos.IdProducto = dProdSal.IdProducto inner join" +
            //    " Indigo.dbo.mSales on dProdSal.IdSal = mSales.IdSal" +
            //    " Where mSales.IdSal = " + Sal + "" +
            //    " Group by mProveedores.IdPersona, mProveedores.Nombre, mStands.Logo" +
            //    " Order by mProveedores.Nombre").ToList());

            return Json("");
        }
    }
}
