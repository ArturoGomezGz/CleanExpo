using ExpoMR.Clases;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Org.BouncyCastle.Bcpg;
using System.Data;

namespace ExpoMR.Controllers
{
    [Authorize]
    public class StandController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _rolManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private static IConfiguration _configuration;

        //Variables para verificar La sesión Y el Usuario
        public StandController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender, RoleManager<IdentityRole> rolManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _rolManager = rolManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _configuration = configuration;
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
            List<string> roles = new List<string>();

            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            int IdPersona = GeneralesController.GetIdPersonas(username);

            roles = await GetRoles();
            foreach (string role in roles)
            {
                if (role == "Cliente")
                    Log.SaveVisita(IdPersona, idProveedor, 0);
            }

            string query = "Select mProveedores.IdPersona as Proveedor, mProveedores.Nombre, mStands.Logo, mStands.Background, mProveedores.Descripcion, CONVERT(int, mStands.Paleta) AS Paleta" +
                                " From mStands inner join " +
                                " mProveedores on mStands.Proveedor = mProveedores.IdPersona " +
                                " where mProveedores.IdPersona =" + idProveedor + " " +
                                " Order by mStands.Rank asc ";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            List<Img> ModelStands = new List<Img>();


            foreach (DataRow row in dataTable.Rows)
            {
                Img stand = new Img();
                stand.idUsuario = IdPersona;
                stand.Proveedor = row.Field<int>(0);
                stand.Nombre = row.Field<string>(1);
                stand.img = row.Field<byte[]>(2);
                stand.fondo = row.Field<byte[]>(3);
                stand.Description = row.Field<string>(4);
                stand.color = row.Field<int>(5);
                ModelStands.Add(stand);
            }
            return View(ModelStands[0]);
        }
        [HttpPost]
        public JsonResult GetFotos(int idProducto)
        {
            string query = "SELECT Foto FROM mFotos where IdProducto =" + idProducto;

            DataTable dataTable = GeneralesController.sqlQueryDT("FotosCS", query);

            byte[] fotos = new byte[dataTable.Rows.Count];

            foreach (DataRow row in dataTable.Rows)
            {
                fotos = row.Field<byte[]>(0);
            }

            return Json(fotos);
        }

        public async Task<List<string>> GetRoles()
        {
            var userId = _userManager.GetUserId(User); // Obtiene el ID del usuario actualmente autenticado
            var user = await _userManager.FindByIdAsync(userId); // Encuentra el usuario por su ID

            var roles = await _userManager.GetRolesAsync(user); // Obtiene los roles del usuario


            return roles.ToList();
        }

        [HttpPost]
        public async Task<JsonResult> GetProductosBusqueda(int IdProveedor, string Busqueda, int idUsuario)
        {
            ResponseModel rm = new ResponseModel();

            List<string> roles = new List<string>();
            try
            {
                string querySearhTerms = "";
                string[] busqueda = Busqueda.Split(' ');
                int n = 1;
                string[] fields = { "vProductos.Nombre", "vProductos.Presentacion", "vProductos.Formula" };

                roles = await GetRoles();

                foreach (string role in roles)
                {
                    if (role == "Cliente")
                        Log.SaveBusqueda(idUsuario, IdProveedor, Busqueda);
                }

                foreach (string w in busqueda)
                {
                    querySearhTerms += "And (";
                    foreach (string f in fields)
                    {
                        querySearhTerms += f + " like '%" + w + "%' Or ";
                    }
                    querySearhTerms = querySearhTerms.Substring(0, querySearhTerms.Length - 3) + ") ";
                    n++;
                }

                string query = "Declare @search TTCotizar"
                               + " INSERT @search(IdPersona, IdProducto)"
                               + "  SELECT " + idUsuario + ",vProductos.IdProducto"
                               + "         FROM ExpoMR.dbo.mLaboratorios"
                               + "  inner join ExpoMR.dbo.vProductos on mLaboratorios.IdLaboratorio = vProductos.IdLaboratorio"
                               + "  WHERE mLaboratorios.Proveedor = " + IdProveedor
                               + " " + querySearhTerms
                               + " SELECT vProductos.IdProducto, vProductos.Nombre, vProductos.Presentacion, vProductos.Formula, "
                               + "  ISNULL(dProductosDescuento.Descuento, CAST(vProductos.Costo * vProductos.Descuento / 100.0 as decimal(9, 2))) as Descuento, "
                               + "  Case When dProductosDescuento.Descuento is null Then 0 Else 1 End as DescuentoCliente, "
                               + "  Precios.Precio - ISNULL(dProductosDescuento.Descuento, CAST(vProductos.Costo * vProductos.Descuento / 100.0 as decimal(9, 2))) as PrecioFinal"
                               + "  FROM dbo.GetMultiPrecioExpo(@search) as Precios inner join"
                               + "  ExpoMR.dbo.vProductos on Precios.IdProducto = vProductos.IdProducto left outer join"
                               + "  ExpoMR.dbo.dProductosDescuento on vProductos.IdProducto = dProductosDescuento.IdProducto And dProductosDescuento.Cliente =" + idUsuario
                               + "  ORDER BY vProductos.Nombre asc";

                DataTable dt = GeneralesController.sqlQueryDT("IndigoCS", query);

                var jsonData = new
                {
                    rows = (
                       from c in dt.AsEnumerable()
                       select new
                       {
                           IdProducto = c[0],
                           Nombre = c[1],
                           Presentacion = c[2],
                           Formula = c[3],
                           Descuento = c[4],
                           DescuentoCliente = c[5],
                           PrecioFinal = c[6]

                       }).ToArray()
                };

                rm.response = true;
                rm.message = "ok";
                rm.result = jsonData.rows;

            }
            catch (Exception e)
            {
                rm.response = false;
                rm.message = e.Message;
            }

            return Json(rm);
        }
        public static List<PromocionArticulo> GetPromocionArticulo(int idProveedor, int IdUsuario)
        {
            List<PromocionArticulo> PromocionArticulo = new List<PromocionArticulo>();
            string query = "Select mArticulos.IdArticulo, mArticulos.Nombre, mArticulos.Descripcion,"
                            + " dArticulos.IDdArticulos, dArticulos.Foto"
                            + " From mArticulos inner join "
                            + " dArticulos on mArticulos.IdArticulo = dArticulos.IdArticulo And dArticulos.NumFoto = 1"
                            + " Where mArticulos.Proveedor =" + idProveedor
                            + " Order by mArticulos.IdArticulo";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            foreach (DataRow row in dataTable.Rows)
            {
                PromocionArticulo Promocion = new PromocionArticulo();
                Promocion.IdArticulo = Convert.ToInt32(row.Field<Int16>(0));
                Promocion.Nombre = row.Field<string>(1);
                Promocion.Descripcion = row.Field<string>(2); //GetFotos(producto.IdProducto);
                Promocion.IdArticulos = row.Field<int>(3);
                Promocion.imgPromocion = row.Field<byte[]>(4);
                PromocionArticulo.Add(Promocion);
            }

            return PromocionArticulo;
        }
        public static List<Promocion> GetPromociones(int idProveedor, int IdUsuario)
        {
            List<Promocion> PromocionArticulo = new List<Promocion>();
            try
            {
                SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
                SqlCommand cmd = new SqlCommand("PromocionesActivasCliente", cnn);
                DataTable dataTable = new DataTable();
                SqlDataAdapter sqlDA;
                cnn.Open();

                // 2. set the command object so it knows to execute a stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                //// 3. add parameter to command, which will be passed to the stored procedure
                cmd.Parameters.Add(new SqlParameter("@proveedor", idProveedor));
                cmd.Parameters.Add(new SqlParameter("@cliente", IdUsuario));

                cmd.Connection = cnn;
                sqlDA = new SqlDataAdapter(cmd);
                sqlDA.Fill(dataTable);
                cnn.Close();

                foreach (DataRow row in dataTable.Rows)
                {
                    Promocion Promocion = new Promocion();
                    Promocion.Condicion = Convert.ToInt32(row["IdTipoPromocion"]); //GetPromocionCondicionResumen(row["IdTipoPromocion"], row["Monto"], row["Piezas"], row["Producto"]);
                    Promocion.Premio = row["Premios"].ToString();
                    Promocion.ImgDisponibilidad = GetImageUrlDisponibilidad(row["Obtenida"], row["Disponibilidad"]);
                    Promocion.Monto = row["Monto"] == DBNull.Value ? null : Convert.ToInt32(row["Monto"]);
                    Promocion.Piezas = row["Piezas"] == DBNull.Value ? null : Convert.ToInt32(row["Piezas"]);
                    Promocion.Producto = row["Producto"] == DBNull.Value ? "" : row["Producto"].ToString();
                    PromocionArticulo.Add(Promocion);
                }
            }
            catch (Exception e)
            {

            }
            return PromocionArticulo;
        }
        public static List<Promocion> GetPromocionesEscalonadas(int idProveedor, int IdUsuario)
        {
            List<Promocion> PromocionArticulo = new List<Promocion>();
            try
            {
                SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
                SqlCommand cmd = new SqlCommand("PromocionesActivasCliente", cnn);
                DataTable dataTable = new DataTable();
                SqlDataAdapter sqlDA;
                cnn.Open();

                // 2. set the command object so it knows to execute a stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                //// 3. add parameter to command, which will be passed to the stored procedure
                cmd.Parameters.Add(new SqlParameter("@proveedor", idProveedor));
                cmd.Parameters.Add(new SqlParameter("@cliente", IdUsuario));
                cmd.Parameters.Add(new SqlParameter("@idtipopromo", 4));

                cmd.Connection = cnn;
                sqlDA = new SqlDataAdapter(cmd);
                sqlDA.Fill(dataTable);
                cnn.Close();

                foreach (DataRow row in dataTable.Rows)
                {
                    Promocion Promocion = new Promocion();
                    Promocion.Condicion = Convert.ToInt32(row["IdTipoPromocion"]); //GetPromocionCondicionResumen(row["IdTipoPromocion"], row["Monto"], row["Piezas"], row["Producto"]);
                    Promocion.Premio = row["Premios"].ToString();
                    Promocion.ImgDisponibilidad = GetImageUrlDisponibilidad(row["Obtenida"], row["Disponibilidad"]);
                    Promocion.Monto = row["Monto"] == DBNull.Value ? null : Convert.ToInt32(row["Monto"]);
                    Promocion.Piezas = row["Piezas"] == DBNull.Value ? null : Convert.ToInt32(row["Piezas"]);
                    Promocion.Producto = row["Producto"] == DBNull.Value ? "" : row["Producto"].ToString();
                    PromocionArticulo.Add(Promocion);
                }
            }
            catch (Exception e)
            {

            }
            return PromocionArticulo;
        }
        public static string GetPromocionCondicionResumen(object idtipopromocion, object monto, object piezas, object producto)
        {
            int Idtipopromocion;
            int Piezasint;
            string Condicion = "";
            string Productostr = producto.ToString();
            string Monto = "";

            int.TryParse(idtipopromocion.ToString(), out Idtipopromocion);
            int.TryParse(piezas.ToString(), out Piezasint);

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
        public static string GetImageUrlDisponibilidad(object obtenida, object disponible)
        {
            string imageUrl = "../Imagenes/agotada.png";
            int disponibilidad;
            bool Obtenida;

            bool.TryParse(obtenida.ToString(), out Obtenida);
            int.TryParse(disponible.ToString(), out disponibilidad);

            if (Obtenida)
            {
                imageUrl = "../Imagenes/ganada.png";
            }
            else if (disponibilidad >= 50)
            {
                imageUrl = "../Imagenes/disponible.png";
            }
            else if (disponibilidad > 0)
            {
                imageUrl = "../Imagenes/pocasPiezas.png";
            }

            return imageUrl;
        }

        [HttpPost]
        public List<string> GetAnuncios(int idProveedor)
        {
            List<string> anuncios = new List<string>();

            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand cmd = new SqlCommand("StandAnuncios", cnn);
            SqlDataReader dr;
            cnn.Open();

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@stand", idProveedor));
            dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                anuncios.Add(dr.GetString(0));
            }
            cnn.Close();


            return anuncios;
        }
    }
}
