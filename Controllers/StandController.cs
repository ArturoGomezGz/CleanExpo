using ExpoMR.Clases;
using ExpoMR.Datos;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using System.Data;
using System.Drawing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Org.BouncyCastle.Utilities;
using System.Runtime.CompilerServices;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Cliente,ClienteVIP,Proveedor")]

    public class StandController : Controller
    {
        //Variables para obtener (la sesión, el Usuario, path de carpeta wwwroot)
        private readonly UserManager<IdentityUser> _userManager;
        private static IConfiguration _configuration;
        private readonly IWebHostEnvironment webHostEnvironment;

        public StandController(UserManager<IdentityUser> userManager, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _configuration = configuration;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Editar(int idProveedor)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "MiCuenta");
            }
            else
            {
                int Cliente = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));

                //Se guarda el registro de vista del cliente al stand
                if (ExpoMR.Clases.WebSite.ValidaRolUsuario(currentUser.Email, "Cliente") == true
                    || ExpoMR.Clases.WebSite.ValidaRolUsuario(currentUser.Email, "ClienteVIP") == true)
                {
                    Log.SaveVisita(Cliente, idProveedor, 0);
                }

                StandVIewModel Stand = new StandVIewModel();

                string query = "SELECT Stand.Proveedor, Stand.Nombre, Stand.Descripcion, Stand.Template, Stand.Rank, Stand.Chat, Stand.Paleta, Stand.Logo, Stand.Background FROM GetStandProv(@IdProveedor) AS Stand";

                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@IdProveedor", idProveedor);

                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

                while (dr.Read())
                {
                    Stand.Proveedor = (int)dr["Proveedor"];
                    Stand.Nombre = (string)dr["Nombre"];
                    Stand.Descripcion = (string)dr["Descripcion"];
                    Stand.Rank = (byte)dr["Rank"];
                    Stand.Chat = (string)dr["Chat"];
                    Stand.Paleta = (byte)dr["Paleta"];
                    Stand.Logo = (dr["Logo"] != DBNull.Value ? (byte[])dr["Logo"] : Array.Empty<byte>());
                    Stand.Background = (dr["Background"] != DBNull.Value ? (byte[])dr["Background"] : Array.Empty<byte>());
                    Stand.IdUsuario = Cliente;
                    Stand.Activedhcp = "active";
                }

				dB.Close();

				bool isFechaExpoVIP = ExpoMR.Clases.WebSite.ValidaExpoVIP();

                if (ExpoMR.Clases.WebSite.ValidaRolUsuario(currentUser.Email, "ClienteVIP") == true)
                {
                    if (isFechaExpoVIP == true)
                    {
                        return View(Stand);
                    }
                    else
                    {
                        return RedirectToAction("Index", "MiCuenta");
                    }
                }
                else
                {
                    if (isFechaExpoVIP == true)
                    {
                        return RedirectToAction("Index", "MiCuenta");
                    }
                    else
                    {
                        return View(Stand);
                    }
                }                
            }
        }

        public async Task<IActionResult> EditarIndex(int idProveedor, int IdSal)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "MiCuenta");
            }
            else
            {
                //Se guarda el registro de vista del cliente al stand
                if (ExpoMR.Clases.WebSite.ValidaRolUsuario(currentUser.Email, "Cliente") == true
                    || ExpoMR.Clases.WebSite.ValidaRolUsuario(currentUser.Email, "ClienteVIP") == true)
                {                    
                    Log.SaveVisita(Convert.ToInt32(HttpContext.Session.GetString("ClienteSel")), idProveedor, 0);
                }

                StandVIewModel Stand = new StandVIewModel();

                string query = "SELECT Stand.Proveedor, Stand.Nombre, Stand.Descripcion, Stand.Template, Stand.Rank, Stand.Chat, Stand.Paleta, Stand.Logo, Stand.Background FROM GetStandProv(@IdProveedor) AS Stand";

                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@IdProveedor", idProveedor);

                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

                while (dr.Read())
                {                   
                    Stand.Proveedor = (int)dr["Proveedor"];
                    Stand.Nombre = (string)dr["Nombre"];
                    Stand.Descripcion = (string)dr["Descripcion"];
                    Stand.Rank = (byte)dr["Rank"];
                    Stand.Chat = (string)dr["Chat"];
                    Stand.Paleta = (byte)dr["Paleta"];
                    Stand.Logo = (dr["Logo"] != DBNull.Value ? (byte[])dr["Logo"] : Array.Empty<byte>());
                    Stand.Background = (dr["Background"] != DBNull.Value ? (byte[])dr["Background"] : Array.Empty<byte>());
                    Stand.IdUsuario = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));
                    Stand.Activedhcp = null;
                    Stand.Activestatic = "active";
                }

				dB.Close();

				string query2 = "SELECT Sal.IdSal, Sal.Sal, Sal.Habilitado FROM GetSales(@IdSal, @letra, @sal) AS Sal";

                DB dB2 = new();
                SqlParameter[] parameters2 = new SqlParameter[3];
                parameters2[0] = new SqlParameter("@IdSal", IdSal);
                parameters2[1] = new SqlParameter("@letra", "");
                parameters2[2] = new SqlParameter("@sal", "");

                SqlDataReader dr2 = dB2.ExecuteReader(query2, ref parameters2, commandType: CommandType.Text);

                while (dr2.Read())
                { 
                    Stand.Sal = (string)dr2["Sal"];
                }

				dB2.Close();

				bool isFechaExpoVIP = ExpoMR.Clases.WebSite.ValidaExpoVIP();

                if (ExpoMR.Clases.WebSite.ValidaRolUsuario(currentUser.Email, "ClienteVIP") == true)
                {
                    if (isFechaExpoVIP == true)
                    {
                        return View("~/Views/Stand/Editar.cshtml", Stand);
                    }
                    else
                    {
                        return RedirectToAction("Index", "MiCuenta");
                    }
                }
                else
                {
                    if (isFechaExpoVIP == true)
                    {
                        return RedirectToAction("Index", "MiCuenta");
                    }
                    else
                    {
                        return View("~/Views/Stand/Editar.cshtml", Stand);
                    }
                }
            }
        }

        [HttpPost]
        public JsonResult GetFotos(int idProducto)
        {
            FotosViewModel FotoProducto = new FotosViewModel();

            string query = "SELECT Foto.IdFoto, Foto.IdProducto, Foto.Foto FROM GetFotoProd(@IdProducto) AS Foto";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProducto", idProducto);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                FotoProducto.IdFoto = (int)dr["IdFoto"];
                FotoProducto.IdProducto = (int)dr["IdProducto"];
                FotoProducto.Foto = (byte[])dr["Foto"];
            }

			dB.Close();

			return Json(FotoProducto.Foto);
        }

        public JsonResult GetImageProductByLab(int idProveedor, int IdUsuario)
        {
            List<Producto> productos = new List<Producto>();          

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@proveedor", idProveedor);
            parameters[1] = new SqlParameter("@cliente", IdUsuario);

            SqlDataReader dr = dB.ExecuteReader("ExpoStandProductos", ref parameters, CommandType.StoredProcedure, "IndigoCS");

            while (dr.Read())
            {
                Producto producto = new Producto();

                producto.PrecioMR = (decimal)dr["PrecioMR"];
                producto.IdProducto = (Int16)dr["IdProducto"];
                producto.Nombre = (string)dr["Nombre"];
                producto.Presentacion = (string)dr["Presentacion"];
                producto.Formula = (dr["Formula"] != DBNull.Value ? (string)dr["Formula"] : "");
                producto.Descuento = (decimal)dr["Descuento"];
                producto.DescuentoCLiente = (int)(dr["DescuentoCLiente"]);
                producto.PrecioFinal = (decimal)(dr["PrecioFinal"]);

                productos.Add(producto);
            }

			dB.Close();

			return Json(productos);
        }

        [HttpPost]
        public async Task<JsonResult> GetProductosBusqueda(int IdProveedor, string Busqueda, int IdUsuario)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            ResponseModel rm = new();
			DB dB = new();

			try
            {
                string querySearhTerms = "";
                string[] busqueda = Busqueda.Split(' ');
                int n = 1;
                string[] fields = { "vProductos.Nombre", "vProductos.Presentacion", "vProductos.Formula" };

                //Se guarda el registro de vista del cliente al stand
                if (ExpoMR.Clases.WebSite.ValidaRolUsuario(currentUser.Email, "Cliente") == true
                    || ExpoMR.Clases.WebSite.ValidaRolUsuario(currentUser.Email, "ClienteVIP") == true)
                {
                    Log.SaveBusqueda(IdUsuario, IdProveedor, Busqueda);
                }

                foreach (string w in busqueda)
                {
                    querySearhTerms += "And (";
                    foreach (string f in fields)
                    {
                        querySearhTerms += f + " like '%" + w + "%' Or ";
                    }
                    querySearhTerms = querySearhTerms[..^3] + ") ";
                    n++;
                }

                string query = "DECLARE @search TTCotizar"
                               + " INSERT @search(IdPersona, IdProducto)"
                               + "  SELECT " + IdUsuario + ", vProductos.IdProducto"
                               + "  FROM ExpoMR.dbo.mLaboratorios INNER JOIN"
                               + "  ExpoMR.dbo.vProductos ON mLaboratorios.IdLaboratorio = vProductos.IdLaboratorio"
                               + "  WHERE mLaboratorios.Proveedor = " + IdProveedor
                               + " " + querySearhTerms
                               + " SELECT CAST(Precios.Precio AS DECIMAL(9,2)) AS PrecioMR,"
                               + " vProductos.IdProducto, vProductos.Nombre, vProductos.Presentacion, ISNULL(vProductos.Formula, '') AS Formula,"
                               + " CAST(Precios.Precio * vProductos.Descuento / 100.0 AS DECIMAL(9, 2)) +" //Descuento General
                               + " CAST(((Precios.Precio - CAST(Precios.Precio * vProductos.Descuento / 100.0 AS DECIMAL(9, 2))) *" //Precio del Cliente
                               + " ISNULL(dProductosDescuento.Descuento,0)) / 100.0 AS DECIMAL(9,2)) AS Descuento," //Descuento Especifico del Cliente
                               + " CASE WHEN dProductosDescuento.Descuento IS NULL THEN 0 ELSE 1 END AS DescuentoCliente,"
                               + " CAST(Precios.Precio - (CAST(Precios.Precio * vProductos.Descuento / 100.0 AS DECIMAL(9, 2)) +" //Precio menos Descuento General
                               + " CAST(((Precios.Precio - CAST(Precios.Precio * vProductos.Descuento / 100.0 AS DECIMAL(9, 2))) *" //Precio del Cliente
                               + " ISNULL(dProductosDescuento.Descuento,0)) / 100.0 AS DECIMAL(9,2))) AS DECIMAL(9,2)) AS PrecioFinal" //Descuento Especifico del Cliente
                               + " FROM dbo.GetMultiPrecioExpo(@search) AS Precios INNER JOIN"
                               + " ExpoMR.dbo.vProductos ON Precios.IdProducto = vProductos.IdProducto LEFT OUTER JOIN"
                               + " ExpoMR.dbo.dProductosDescuento ON vProductos.IdProducto = dProductosDescuento.IdProducto AND dProductosDescuento.Cliente =" + IdUsuario
                               + " ORDER BY vProductos.Nombre, vProductos.Presentacion";

                SqlParameter[] parameters = new SqlParameter[0];

                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, CommandType.Text, "IndigoCS");

                var jsonData = new
                {
                    rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           IdProducto = row[1],
                           Nombre = row[2],
                           Presentacion = row[3],
                           Formula = row[4],
                           Descuento = row[5],
                           DescuentoCliente = row[6],
                           PrecioFinal = row[7],
                           imgProducto = GeneralesController.GetFoto(Convert.ToInt32(row[1]))
                       }).ToArray()
                };

                rm.Response = true;
                rm.Message = "ok";
                rm.Result = jsonData.rows;

            }
            catch (Exception e)
            {
                rm.Response = false;
                rm.Message = e.Message;
            }

			dB.Close();
			
            return Json(rm);
        }

        public static List<ArticulosProveedores> GetPromocionArticulo(int idProveedor)
        {
            List<ArticulosProveedores> ArticulosProveedor = new();

            string query = "SELECT Articulos.IdArticulo, Articulos.Nombre, Articulos.Descripcion, Articulos.IDdArticulos, Articulos.Foto, Articulos.NumFoto FROM GetArticulosProv (@IdProveedor) AS Articulos";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProveedor", idProveedor);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                ArticulosProveedores Articulo = new ArticulosProveedores();

                Articulo.IdArticulo = (Int16)dr["IdArticulo"];
                Articulo.Nombre = (string)dr["Nombre"];
                Articulo.Descripcion = (string)dr["Descripcion"];
                Articulo.IDdArticulos = (dr["IDdArticulos"] != DBNull.Value ? (int)dr["IDdArticulos"] : 0);
                Articulo.Foto = (dr["Foto"] != DBNull.Value ? (byte[])dr["Foto"] : Array.Empty<byte>());

                ArticulosProveedor.Add(Articulo);
            }

            dB.Close();

            return ArticulosProveedor;
        }

        public static List<Promocion> GetPromociones(int idProveedor, int IdUsuario)
        {
            List<Promocion> Promociones = new();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@proveedor", idProveedor);
            parameters[1] = new SqlParameter("@cliente", IdUsuario);

            SqlDataReader dr = dB.ExecuteReader("PromocionesActivasCliente", ref parameters);

            while (dr.Read())
            {
                Promocion Promocion = new Promocion();

                Promocion.IdPromocion = (Int16)dr["IdPromocion"];
                Promocion.Monto = (float)dr["Monto"];
                Promocion.Piezas = (Int16)dr["Piezas"];
                Promocion.Producto = (string)dr["Producto"];
                Promocion.Disponibilidad = (int)dr["Disponibilidad"];
                Promocion.Obtenida = (bool)dr["Obtenida"];
                Promocion.IdTipoPromocion = (byte)dr["IdTipoPromocion"];
                Promocion.Premios = (string)dr["Premios"];

                Promociones.Add(Promocion);
            }

			dB.Close();

			foreach (var p in Promociones)
            {
                p.ImgDisponibilidad = GetImageUrlDisponibilidad(p.Obtenida, p.Disponibilidad);
            }

            return Promociones;
        }

        public static List<Promocion> GetPromocionesEscalonadas(int idProveedor, int IdUsuario)
        {
            List<Promocion> PromocionesEscalonadas = new();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@proveedor", idProveedor);
            parameters[1] = new SqlParameter("@cliente", IdUsuario);
            parameters[2] = new SqlParameter("@idtipopromo", 4);

            SqlDataReader dr = dB.ExecuteReader("PromocionesActivasCliente", ref parameters);

            while (dr.Read())
            {
                Promocion PromocionEscalonada = new Promocion();

                PromocionEscalonada.IdPromocion = (Int16)dr["IdPromocion"];
                PromocionEscalonada.Monto = (float)dr["Monto"];
                PromocionEscalonada.Piezas = (Int16)dr["Piezas"];
                PromocionEscalonada.Producto = (string)dr["Producto"];
                PromocionEscalonada.Disponibilidad = (int)dr["Disponibilidad"];
                PromocionEscalonada.Obtenida = (bool)dr["Obtenida"];
                PromocionEscalonada.IdTipoPromocion = (byte)dr["IdTipoPromocion"];
                PromocionEscalonada.Premios = (string)dr["Premios"];

                PromocionesEscalonadas.Add(PromocionEscalonada);
            }

			dB.Close();

			foreach (var p in PromocionesEscalonadas)
            {
                p.ImgDisponibilidad = GetImageUrlDisponibilidad(p.Obtenida, p.Disponibilidad);
            }

            return PromocionesEscalonadas;
        }

        public static string GetPromocionCondicionResumen(object idtipopromocion, object monto, object piezas, object producto)
        {
            string Condicion = "";
            string Productostr = producto.ToString();
            string Monto = "";

            _ = int.TryParse(idtipopromocion.ToString(), out int Idtipopromocion);
            _ = int.TryParse(piezas.ToString(), out int Piezasint);

            if (string.IsNullOrEmpty(Productostr))
                Productostr = "Productos de este proveedor";

            if (!string.IsNullOrEmpty(monto.ToString()))
                Monto = string.Format("{0:C}", Convert.ToDecimal(monto));

            switch (Idtipopromocion)
            {
                case 1:
                    Condicion = "Por haber visitado este Stand, llévate";
                    break;
                case 2:
                    if (string.IsNullOrEmpty(Monto))
                    {
                        Condicion = "Por cada <strong>" + Piezasint + " Piezas</strong> de compra de " + Productostr + " llévate";
                    }
                    else
                    {
                        Condicion = "Por cada <strong>" + Monto + " Pesos</strong> de compra de " + Productostr + " llévate";
                    }
                    break;
                case 3:
                    if (string.IsNullOrEmpty(Monto))
                    {
                        Condicion = "En la compra de <strong>" + Piezasint + " Piezas</strong> de " + Productostr + " llévate";
                    }
                    else
                    {
                        Condicion = "En la compra de <strong>" + Monto + " Pesos</strong> de " + Productostr + " llévate";
                    }
                    break;
                case 4:
                    if (string.IsNullOrEmpty(Monto))
                    {
                        Condicion = "Compra <strong>" + Piezasint + " Piezas</strong> de " + Productostr + " y llévate";
                    }
                    else
                    {
                        Condicion = "Compra <strong>" + Monto + " Pesos</strong> de " + Productostr + " y llévate";
                    }
                    break;
            }

            return Condicion;
        }

        public static string GetImageUrlDisponibilidad(bool obtenida, int disponible)
        {
            string imageUrl = "../Imagenes/agotada.png";

            if (obtenida)
            {
                imageUrl = "../Imagenes/ganada.png";
            }
            else if (disponible >= 50)
            {
                imageUrl = "../Imagenes/disponible.png";
            }
            else if (disponible > 0)
            {
                imageUrl = "../Imagenes/pocasPiezas.png";
            }
            return imageUrl;
        }

        [HttpPost]
        public List<Anuncios> GetAnuncios(int idProveedor)
        {
            List<Anuncios> Anuncios = new();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@stand", idProveedor);

            SqlDataReader dr = dB.ExecuteReader("StandAnuncios", ref parameters);

            while (dr.Read())
            {
                Anuncios Anuncio = new Anuncios();

                Anuncio.Anuncio = (string)dr["Anuncio"];

                Anuncios.Add(Anuncio);
            }

			dB.Close();

			return Anuncios;
        }

        [HttpPost]
        public List<ArticulosProveedores> GetAnuncio(int IdArticulo)
        {
            List<ArticulosProveedores> Articulos = new();

            string query = "SELECT Articulo.IdArticulo, Articulo.Nombre, Articulo.Descripcion, Articulo.IDdArticulos, Articulo.Foto, Articulo.Proveedor, Articulo.Cantidad, Articulo.NumFoto FROM GetArticuloProv(@IdArticulo) AS Articulo";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdArticulo", IdArticulo);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                ArticulosProveedores Articulo = new ArticulosProveedores();

                Articulo.IdArticulo = (Int16)dr["IdArticulo"];
                Articulo.Nombre = (string)dr["Nombre"];
                Articulo.Descripcion = (string)dr["Descripcion"];
                Articulo.IDdArticulos = (int)dr["IDdArticulos"];
                Articulo.Foto = (byte[])dr["Foto"];

                Articulos.Add(Articulo);
            }

			dB.Close();

			return Articulos;
        }

        [HttpPost]
        public JsonResult DetalleProducto(int IdProducto, int Idproveedor)
        {
            Log.SaveVisita(Convert.ToInt32(HttpContext.Session.GetString("ClienteSel")), Idproveedor, IdProducto);

            CotizaProducto ProdCotizado = new CotizaProducto();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@cliente", HttpContext.Session.GetString("ClienteSel"));
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
            }

			dB.Close();

			return Json(ProdCotizado);
        }

        [HttpPost]
        public JsonResult GetFotoProducto(int IdProducto)
        {
            byte[] foto = GeneralesController.GetFoto(IdProducto);

            return Json(foto);
        }
    }
}