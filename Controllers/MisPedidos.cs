using ExpoMR.Clases;
using ExpoMR.Datos;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Nancy.Conventions;
using Org.BouncyCastle.Utilities;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class MisPedidos : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private static IConfiguration _configuration;
        private readonly IWebHostEnvironment webHostEnvironment;

        public MisPedidos(UserManager<IdentityUser> userManager, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "MiCuenta");
            }

            int IdCliente = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));
            int IdEmail = Convert.ToInt32(HttpContext.Session.GetString("Session_IdEmail"));
            int isFechaExpo = ExpoMR.Clases.WebSite.ValidaDiaExpo();

            Pedidos Pedidos = new Pedidos();
            Pedidos.IdCliente = IdCliente;
            Pedidos.IdEmail = IdEmail;
            Pedidos.TotalesPedidos = GetTotalesPedidos(IdCliente, IdEmail, true); //se inicializan los totales de los pedidos
            Pedidos.IdPedido = 0;
            Pedidos.PedidosCliente = GetPedidosCliente(IdCliente, IdEmail);

            if (isFechaExpo == 1) // 1 = Dia Expo
                return View(Pedidos);
            else
                return RedirectToAction("Index", "MiCuenta");
        }

        public List<ddlPedidos> GetPedidosCliente(int IdCliente, int IdEmail)
        {
            List<ddlPedidos> Pedidos = new List<ddlPedidos>();
            DB dB = new();

            try
            {
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@IdCliente", IdCliente);
                parameters[1] = new SqlParameter("@idemail", IdEmail);

                string query = "SELECT 0 AS IdPedido, 'TODOS' AS Pedido " +
                               "UNION " +
                               "SELECT vPedidos.IdPedido, CAST(vPedidos.IdPedido AS NVARCHAR) AS Pedido " +
                               "FROM vPedidos " +
                               "WHERE vPedidos.Cliente = @IdCliente " +
                               "AND vPedidos.IdEmail = @idemail " +
                               "GROUP BY vPedidos.IdPedido";

                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

                while (dr.Read())
                {
                    ddlPedidos Pedido = new ddlPedidos()
                    {
                        IdPedido = Convert.ToInt32(dr["IdPedido"]),
                        Pedido = dr["Pedido"].ToString()
                    };

                    Pedidos.Add(Pedido);
                }
            }
            catch (Exception e)
            {
                return new List<ddlPedidos>();
            }

            dB.Close();

            return Pedidos;
        }

        public static List<PedidosCarrito> GetPedidos(int IdCliente, int IdEmail, int IdPedido = 0)
        {
            List<PedidosCarrito> pedidos = new List<PedidosCarrito>();

            string query = "SELECT ROW_NUMBER() OVER(PARTITION BY vPedidos.IdPedido,vProductos.IdLaboratorio " +
                            "ORDER BY mProveedores.Nombre, vProductos.Nombre ASC) AS RegistroProveedor, mLaboratorios.Proveedor, " +
                            "ROW_NUMBER() OVER(PARTITION BY vProductos.IdLaboratorio, vPedidos.IdPedido ORDER BY mProveedores.Nombre, " +
                            "vProductos.Nombre ASC) AS RegistroPedido, vPedidos.IdPedido, vProductos.IdProducto, vProductos.Nombre, " +
                            "vProductos.Presentacion, vPedidos.Cantidad, vPedidos.PrecioFinal, vPedidos.Total, mLaboratorios.Laboratorio, vProductos.Iva " +
                            "FROM vPedidos INNER JOIN " +
                            "vProductos ON vPedidos.IdProducto = vProductos.IdProducto INNER JOIN " +
                            "mLaboratorios ON vProductos.IdLaboratorio = mLaboratorios.IdLaboratorio INNER JOIN " +
                            "mProveedores ON mLaboratorios.Proveedor = mProveedores.IdPersona " +
                            "WHERE vPedidos.Cliente = @IdCliente " +
                            "AND   vPedidos.IdEmail = @idemail " +
                            "AND ( vPedidos.IdPedido =  @IdPedido OR @IdPedido = 0) " +
                            "ORDER BY vPedidos.IdPedido, mProveedores.Nombre, mLaboratorios.Laboratorio, vProductos.Nombre";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[3];

            parameters[0] = new SqlParameter("@IdCliente", IdCliente);
            parameters[1] = new SqlParameter("@idemail", IdEmail);
            parameters[2] = new SqlParameter("@IdPedido", IdPedido);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                PedidosCarrito pedidoCarrito = new PedidosCarrito()
                {
                    RegistroProveedor = Convert.ToInt32(dr["RegistroProveedor"]),
                    Proveedor = Convert.ToInt32(dr["Proveedor"]),
                    RegistroPedido = Convert.ToInt32(dr["RegistroPedido"]),
                    IdPedido = Convert.ToInt32(dr["IdPedido"]),
                    IdProducto = Convert.ToInt32(dr["IdProducto"]),
                    Nombre = dr["Nombre"].ToString(),
                    Presentacion = dr["Presentacion"].ToString(),
                    Cantidad = Convert.ToInt32(dr["Cantidad"]),
                    PrecioFinal = Convert.ToDecimal(dr["PrecioFinal"]),
                    Total = Convert.ToDecimal(dr["Total"]),
                    Laboratorio = dr["Laboratorio"].ToString(),
                    Iva = Convert.ToBoolean((dr["Iva"]))
                };

                pedidos.Add(pedidoCarrito);
            }

            dB.Close();

            return pedidos;
        }

        public static TotalesPedidos GetTotalesPedidos(int IdCliente, int IdEmail, bool Origen)
        {
            TotalesPedidos totalesPedidos = new TotalesPedidos();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@cliente", IdCliente);
            parameters[1] = new SqlParameter("@idEmail", IdEmail);
            parameters[2] = new SqlParameter("@Origen", Origen);

            SqlDataReader dr = dB.ExecuteReader("GetTotales", ref parameters);

            if (dr.Read())
            {
                totalesPedidos.Productos = (dr["Productos"] != DBNull.Value ? (Int32)dr["Productos"] : 0);
                totalesPedidos.Subtotal = (dr["Subtotal"] != DBNull.Value ? (decimal)dr["Subtotal"] : 0);
                totalesPedidos.IVA = (dr["Iva"] != DBNull.Value ? (decimal)dr["Iva"] : 0);
                totalesPedidos.Total = (dr["Total"] != DBNull.Value ? (decimal)dr["Total"] : 0);
                totalesPedidos.Credito = (dr["Credito"] != DBNull.Value ? (decimal)dr["Credito"] : 0);
                totalesPedidos.HasProduct = (dr["Productos"] != DBNull.Value ? true : false);
            }

            dB.Close();

            return totalesPedidos;
        }

        public JsonResult GetTotalByPedido(int IdPedido)
        {
            ResumenPedido Pedido = new();

            string query = "SELECT mPedidos.IdPedido, COUNT(dbo.dPedidos.IdProducto) AS Productos, " +
                "CAST(SUM(dbo.dPedidos.Cantidad * (dbo.dPedidos.Unitario - dbo.dPedidos.Descuento)) AS DECIMAL(18, 2)) AS Subtotal, " +
                "CAST(SUM(CASE WHEN mProductos.Iva = 1 THEN dPedidos.Cantidad * (dPedidos.Unitario - dPedidos.Descuento) * Indigo.dbo.GetIva(0) ELSE 0 END) AS DECIMAL(18, 2)) AS IvaMonto, " +
                "CAST(SUM(dbo.dPedidos.Cantidad * (dbo.dPedidos.Unitario - dbo.dPedidos.Descuento)) + SUM(CASE WHEN mProductos.Iva = 1 THEN dPedidos.Cantidad * (dPedidos.Unitario - dPedidos.Descuento) * Indigo.dbo.GetIva(0)  ELSE 0 END) AS DECIMAL(18, 2)) AS Total " +
                "FROM dbo.mPedidos INNER JOIN " +
                "dbo.dPedidos ON dbo.mPedidos.IdPedido = dbo.dPedidos.IdPedido INNER JOIN " +
                "dbo.mProductos WITH (INDEX(IXmProductos)) ON dbo.dPedidos.IdProducto = dbo.mProductos.IdProducto " +
                "WHERE mPedidos.IdPedido = @IdPedido " +
                "GROUP BY mPedidos.IdPedido";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdPedido", IdPedido);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                Pedido.Pedido = (int)dr["IdPedido"];
                Pedido.Cantidad = (int)(dr["Productos"]);
                Pedido.Subtotal = Convert.ToDecimal(dr["Subtotal"]);
                Pedido.IVA = Convert.ToDecimal(dr["IvaMonto"]);
                Pedido.Total = Convert.ToDecimal(dr["Total"]);
            }

            dB.Close();

            return Json(Pedido);
        }

        public JsonResult GetResumenCompra(int IdCliente, bool TipoResumen)
        {
            var lista = new List<ResumenCompraCliente>();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@IdCliente", IdCliente);
            parameters[1] = new SqlParameter("@TipoDetalle", TipoResumen);
            parameters[2] = new SqlParameter("@Filtro", "");

            SqlDataReader dr = dB.ExecuteReader("GetDetailCompraCliente", ref parameters);

            while (dr.Read())
            {
                lista.Add(new ResumenCompraCliente
                {
                    Proveedor = dr["Proveedor"]?.ToString(),
                    Producto = dr["Producto"]?.ToString(),
                    NumPedidos = dr["NumPedidos"] == DBNull.Value ? 0 : Convert.ToInt32(dr["NumPedidos"]),
                    TotalProds = dr["TotalProds"] == DBNull.Value ? 0 : Convert.ToInt32(dr["TotalProds"]),
                    TotalPiezas = dr["TotalPiezas"] == DBNull.Value ? 0 : Convert.ToInt32(dr["TotalPiezas"]),
                    TotalCompra = dr["TotalCompra"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["TotalCompra"])
                });
            }

            dB.Close();

            return Json(lista);
        }
    }
}