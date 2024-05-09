using ExpoMR.Clases;
using ExpoMR.Datos;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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

        public IActionResult Contacto() 
        { 
            return View();
        }       

        public IActionResult Preguntas() 
        {
            return View();
        }

        public IActionResult Privacidad() 
        {
            return View();
        }

        public IActionResult CondicionesRuleta() 
        {
            return View();
        }

        public IActionResult TerminosyCondiciones() 
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetSales(string letra, string palabra)
        {
            List<mSales> Sales = new();

            string query = "SELECT Sal.IdSal, Sal.Sal, Sal.Habilitado FROM GetSales(@IdSal, @letra, @sal) AS Sal";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@IdSal", "0");
            parameters[1] = new SqlParameter("@letra", letra != null ? letra : "");
            parameters[2] = new SqlParameter("@sal", palabra != null ? palabra : "");

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                mSales sal = new mSales();

                sal.IdSal = (Int16)dr["IdSal"];
                sal.Sal = (string)dr["Sal"];
                sal.Habilitado = (bool)dr["Habilitado"];

                Sales.Add(sal);
            }

            return Json(Sales);
        }

        [HttpPost]
        public JsonResult GetLaboratorioBySal(int Sal) 
        {
            List<StandVIewModel> Proveedores = new();

            string query = "SELECT mProveedores.IdPersona AS Proveedor, mProveedores.Nombre, mStands.Logo " +
                            "FROM mProveedores INNER JOIN " +
                            "mLaboratorios ON mProveedores.IdPersona = mLaboratorios.Proveedor INNER JOIN " +
                            "mProductos ON mLaboratorios.IdLaboratorio = mProductos.IdLaboratorio INNER JOIN " +
                            "mStands ON mStands.Proveedor = mProveedores.IdPersona INNER JOIN " +
                            "Indigo.dbo.dProdsal ON mProductos.IdProducto = dProdSal.IdProducto INNER JOIN " +
                            "Indigo.dbo.mSales ON dProdSal.IdSal = mSales.IdSal " +
                            "WHERE mSales.IdSal =  @IdSal " +
                            "GROUP BY mProveedores.IdPersona, mProveedores.Nombre, mStands.Logo " +
                            "ORDER BY mProveedores.Nombre";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdSal", Sal);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                StandVIewModel proveedor = new StandVIewModel();

                proveedor.Proveedor = (int)dr["Proveedor"];
                proveedor.Nombre = (string)dr["Nombre"];
                proveedor.Logo = (dr["Logo"] != DBNull.Value ? (byte[])dr["Logo"] : Array.Empty<byte>());

                Proveedores.Add(proveedor);
            }

            return Json(Proveedores);
        }
    }
}