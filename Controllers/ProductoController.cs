using ExpoMR.Clases;
using ExpoMR.Datos;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Cliente,ClienteVIP")]
    public class ProductoController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private static IConfiguration _configuration;

        //Variables para verificar La sesión Y el Usuario
        public ProductoController(UserManager<IdentityUser> userManager,IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(int IdProducto = 0, int Idproveedor = 0)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "MiCuenta");
            }
            {
                int Cliente = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));

                //Se guarda el registro de vista del cliente al stand
                if (ExpoMR.Clases.WebSite.ValidaRolUsuario(currentUser.Email, "Cliente") == true
                    || ExpoMR.Clases.WebSite.ValidaRolUsuario(currentUser.Email, "ClienteVIP") == true)
                {
                    Log.SaveVisita(Cliente, Idproveedor, IdProducto);
                }

                CotizaProducto ProdCotizado = new CotizaProducto();

                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@cliente", Cliente);
                parameters[1] = new SqlParameter("@idproducto", IdProducto);

                SqlDataReader dr = dB.ExecuteReader("ExpoCotizarVerProducto", ref parameters, CommandType.StoredProcedure, "IndigoCS");

                while (dr.Read())
                {
                    ProdCotizado.IdProducto = (Int16)dr["IdProducto"];
                    ProdCotizado.Nombre = (string)dr["Nombre"];
                    ProdCotizado.Presentacion = (string)dr["Presentacion"];
                    ProdCotizado.Unidades = (string)dr["Unidades"];
                    ProdCotizado.Publico = (decimal)dr["Publico"];
                    ProdCotizado.Neto = (decimal)dr["Neto"];
                    ProdCotizado.Iva = (bool)(dr["Iva"]);
                    ProdCotizado.Laboratorio = (string)(dr["Laboratorio"]);
                    ProdCotizado.Caja = (Int16)dr["Caja"];
                    ProdCotizado.Codigo = (string)dr["Codigo"];
                    ProdCotizado.Restringido = (bool)(dr["Restringido"]);
                    ProdCotizado.Cantidad = (int)dr["Cantidad"];
                    ProdCotizado.Formula = (dr["Formula"] != DBNull.Value ? (string)dr["Formula"] : "");
                    ProdCotizado.Controlado = (bool)(dr["Controlado"]);
                    ProdCotizado.PrecioMR = (decimal)dr["PrecioMR"];
                    ProdCotizado.DescuentoExpo = (decimal)dr["DescuentoExpo"];
                    ProdCotizado.DescuentoCliente = (bool)(dr["DescuentoCliente"]);
                    ProdCotizado.ValorIva = (decimal)dr["ValorIva"];
                    ProdCotizado.PrecioFinal = (decimal)dr["PrecioFinal"];
                    ProdCotizado.IdProveedor = Idproveedor;
                    ProdCotizado.imgProductos = GeneralesController.GetFoto(IdProducto);
                }

                bool isFechaExpoVIP = ExpoMR.Clases.WebSite.ValidaExpoVIP();

                if (ExpoMR.Clases.WebSite.ValidaRolUsuario(currentUser.Email, "ClienteVIP") == true)
                {
                    if (isFechaExpoVIP)
                    {
                        return View(ProdCotizado);
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
                        return View(ProdCotizado);
                    }
                }                
            }           
        }

        [HttpPost]
        public JsonResult AddCarrito(int idProducto, int cantidad, decimal unitario) {
            
            ResponseModel rm = new ResponseModel();
            
            try 
            {
                int Cliente = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));

                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[5];
                parameters[0] = new SqlParameter("@cliente", Cliente);
                parameters[1] = new SqlParameter("@idproducto", idProducto);
                parameters[2] = new SqlParameter("@unitario", unitario);
                parameters[3] = new SqlParameter("@cantidad", cantidad);
                parameters[4] = new SqlParameter("@usuario", Cliente);

                dB.ExecuteNonQuery("CotizarAgregar", ref parameters);

                rm.Response = true;
                rm.Message = "El producto se agrego al carrito";

            } 
            catch (Exception e)
            {
                rm.Message = e.Message;
            }

            return Json(rm);
        }
    }
}
