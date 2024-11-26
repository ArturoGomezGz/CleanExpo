using ExpoMR.Clases;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X500;
using System.Data;
using System.Text.RegularExpressions;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Administrador,Proveedor")]
    public class EstadisticasController : Controller
    {
        private static IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;

        public EstadisticasController(UserManager<IdentityUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _configuration = config;
        }

        public IActionResult Estadisticas()
        {
            return View();
        }

        [HttpPost]
        public List<Graficas> GetDatosGraficaVisitas()
        {
            int idProveedor = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));
            List<Graficas> Graficas = new();

            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@proveedor", idProveedor);

           dr = dB.ExecuteReader("GraficaVisitas", ref parameters);

            while (dr.Read())
            {
                Graficas grafica = new()
                {
                    fecha = dr.GetDateTime(0).ToString(),
                    stand = dr.GetInt32(1),
                    producto = dr.GetInt32(2)
                };

                Graficas.Add(grafica);
            }

			dB.Close();

			return Graficas;
        }

        [HttpPost]
        public List<Graficas> GetDatosGraficaMonedas()
        {
            int idProveedor = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));
            List<Graficas> Graficas = new();

            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@proveedor", idProveedor);

            dr = dB.ExecuteReader("GraficaMonedas", ref parameters);

            while (dr.Read())
            {
                Graficas grafica = new ()
                {
                    fecha = dr.GetDateTime(0).ToShortDateString(),
                    monedas = dr.GetInt32(1)
                };

                Graficas.Add(grafica);
            }

			dB.Close();

			return Graficas;
        }

        [HttpPost]
        public List<Graficas> GetDatosGraficaVenta()
        {
            int idProveedor = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));
            List<Graficas> Graficas = new();

            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[4];
            parameters[0] = new SqlParameter("@proveedor", idProveedor);
            parameters[1] = new()
            {
                ParameterName = "@carrito",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            };
            parameters[2] = new()
            {
                ParameterName = "@confirmado",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            };
            parameters[3] = new()
            {
                ParameterName = "@ventaprov",
                DbType = DbType.Double,
                Direction = ParameterDirection.Output
            };

            dr = dB.ExecuteReader("GraficaVenta", ref parameters);

            while (dr.Read())
            {
                Graficas grafica = new ()
                {
                    fecha = dr.GetDateTime(0).ToShortDateString(),
                    piezasCarrito = dr.GetInt32(1),
                    piezasConfirmado = dr.GetInt32(2),
                    ventaProv = dr.GetDecimal(3)
                };

                Graficas.Add(grafica);
            }

			dB.Close();

			return Graficas;
        }

        [HttpPost]
        public JsonResult GetRegistroVisitas() 
        {
            int idProveedor = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));

            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@proveedor", idProveedor);
            parameters[1] = new()
            {
                ParameterName = "@total",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            };

            dr = dB.ExecuteReader("EstadisticaVisitaStand", ref parameters);

            var jsonData = new
            {
                rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           IdPersona = row["IdPersona"],
                           Cliente = row["Cliente"],
                           Hora = row["Hora"]
                       }).ToArray()
            };

			dB.Close();

			return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult GetProductosVisitados() 
        {
            int idProveedor = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));

            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@proveedor", idProveedor);
            parameters[1] = new()
            {
                ParameterName = "@total",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            };

            dr = dB.ExecuteReader("EstadisticaVisitaProd", ref parameters);

            var jsonData = new
            {
                rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           IdProducto = row["IdProducto"],
                           Producto = row["Producto"],
                           Visitas = row["Visitas"]
                       }).ToArray()
            };

			dB.Close();

			return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult GetProductosInfo() 
        {
            int idProveedor = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));

            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@proveedor", idProveedor);

            dr = dB.ExecuteReader("EstadisticaProductos", ref parameters);

            var jsonData = new
            {
                rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           IdProducto = row["IdProducto"],
                           Producto = row["Producto"],
                           Visitas = row["Visitas"],
                           Carrito = row["Carrito"],
                           Confirmado = row["Confirmado"],
                           VentaProv = row["VentaProv"]
                       }).ToArray()
            };

			dB.Close();

			return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult GetDetalleProducto(int IdProducto = 0)
        {
            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@idproducto", IdProducto);

            dr = dB.ExecuteReader("EstadisticaProdCliente", ref parameters);

            var jsonData = new
            {
                rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           nombre = row["nombre"],
                           visitas = row["visitas"],
                           carrito = row["carrito"],
                           confirmado = row["confirmado"],
                           ventaProv = row["ventaprov"]
                       }).ToArray()
            };

			dB.Close();

			return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult GetVentaCliente()
        {
            int idProveedor = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));
            string query = "SELECT vPedidos.Cliente, mClientes.Nombre	" +
                "  FROM vPedidos LEFT JOIN" +
                "  vProductos ON vProductos.IdProducto = vPedidos.IdProducto LEFT JOIN" +
                "  mLaboratorios ON mLaboratorios.IdLaboratorio = vProductos.IdLaboratorio LEFT JOIN" +
                "  mClientes ON mClientes.IdPersona = vPedidos.Cliente" +
                "  WHERE mLaboratorios.Proveedor = @IdProveedor" +
                "  GROUP BY vPedidos.Cliente, mClientes.Nombre" +
                "  ORDER BY mClientes.Nombre";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProveedor", idProveedor);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            var jsonData = new
            {
                rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           IdPersona = row["Cliente"],
                           Nombre = row["Nombre"]
                       }).ToArray()
            };

            dB.Close();

            return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult GetDetalleVentaCliente(int IdCliente = 0)
        {
            int idProveedor = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));
            string query = "SELECT vPedidos.IdPedido, vProductos.Nombre,vProductos.Codigo, vPedidos.Cantidad, vPedidos.Total " +
                " FROM  vPedidos LEFT OUTER JOIN " +
                "vProductos ON vProductos.IdProducto = vPedidos.IdProducto LEFT OUTER JOIN " +
                "mLaboratorios ON mLaboratorios.IdLaboratorio = vProductos.IdLaboratorio " +
                "WHERE vpedidos.Cliente = @IdCliente AND mLaboratorios.Proveedor = @IdProveedor";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@IdCliente", IdCliente);
            parameters[1] = new SqlParameter("@IdProveedor", idProveedor);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            var jsonData = new
            {
                rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           IdPedido = row["IdPedido"],
                           Nombre = row["Nombre"],
                           Codigo = row["Codigo"],
                           Cantidad = row["Cantidad"],
                           Total = row["Total"]
                       }).ToArray()
            };

            dB.Close();

            return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult GetMonedasAsignadas() 
        {
            List<MonedasAsignadas> monedas = new();

            int idProveedor = Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));

            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[6];
            parameters[0] = new SqlParameter("@proveedor", idProveedor);
            parameters[1] = new()
            {
                ParameterName = "@monedasadquiridas",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            };
            parameters[2] = new()
            {
                ParameterName = "@monedasrepartidas",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            };
            parameters[3] = new()
            {
                ParameterName = "@saldo",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            };
            parameters[4] = new()
            {
                ParameterName = "@monedasvisitastand",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            };
            parameters[5] = new()
            {
                ParameterName = "@monedasvisitaprod",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            };

            dr = dB.ExecuteReader("EstadisticaMonedas", ref parameters);

            while (dr.Read())
            {
                MonedasAsignadas moneda = new MonedasAsignadas();

                moneda.cliente = (string)dr["Cliente"];
                moneda.monedas = (int)dr["Monedas"];
                moneda.fecha = (DateTime)dr["Fecha"];
                moneda.motivo = (string)dr["Motivo"];

                monedas.Add(moneda);
            }

            dB.Close();
            SaldoMonedas saldoMonedas = new SaldoMonedas();

            saldoMonedas.MonedasAsignadas = monedas;
            saldoMonedas.monedasAdquiridas = (int)parameters[1].Value;
            saldoMonedas.monedasRepartidas = (int)parameters[2].Value;
            saldoMonedas.saldo = (int)parameters[3].Value;
            saldoMonedas.monedasVisitaStand = (int)parameters[4].Value;
            saldoMonedas.monedasVisitaProd = (int)parameters[5].Value;


            return Json(saldoMonedas);
        }

        [HttpPost]
        public JsonResult DetailProducto(int IdProducto = 0)
        {
            DB dB = new();
            SqlDataReader dr;
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@producto", IdProducto);

            dr = dB.ExecuteReader("EstadisticaProdDetalle", ref parameters);

            var jsonData = new
            {
                rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           imgProducto = GeneralesController.GetFoto(IdProducto),
                           producto = row["producto"],
                           codigo = row["codigo"],
                           formula = row["formula"],
                           visitas = row["visitas"],
                           visitasCliente = row["visitasCliente"],
                           carrito = row["carrito"],
                           carritoCliente = row["carritoCliente"],
                           confirmado = row["confirmado"],
                           confirmadoCliente = row["confirmadoCliente"],
                           ventaProv = row["ventaProv"]
                       }).ToArray()
            };

			dB.Close();

			return Json(jsonData.rows);
        }
    }
}
