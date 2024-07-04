#define TESTMODE

using ExpoMR.Clases;
using ExpoMR.Datos;
using ExpoMR.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace ExpoMR.Controllers
{
    //[Authorize]
    public class GeneralesController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private static IConfiguration _configuration;

        //Variables para verificar La sesión Y el Usuario
        public GeneralesController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public int GetMonedas() 
        {
            return WebSite.SaldoMonedas(Convert.ToInt32(HttpContext.Session.GetString("ClienteSel")));
        }

        public string GetRuleta(int IdPersona)
        {
            return WebSite.GetRuleta(IdPersona);
        }

        public int GetCarrito()
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@cliente", HttpContext.Session.GetString("ClienteSel"));
            parameters[1] = new SqlParameter("@usuario", HttpContext.Session.GetString("ClienteSel"));

            return (int)dB.ExecuteScalar("ExpoCountProducts", ref parameters);
        }

        [HttpPost]
        public JsonResult GetRegistroMonedas()
        {
             DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@idpersona", HttpContext.Session.GetString("ClienteSel"));
            parameters[1] = new()
            {
                ParameterName = "@saldo",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            };

            SqlDataReader dr = dB.ExecuteReader("MonedasSaldoDetalle", ref parameters);

            var jsonData = new
            {
                rows = (
                       from row in dr.Cast<System.Data.Common.DbDataRecord>()
                       select new
                       {
                           fecha = row["Fecha"],
                           concepto = row["Concepto"],
                           importe = row["Monedas"],
                           saldo = row["Saldo"]
                       }).ToArray()
            };

			dB.Close();

			return Json(jsonData.rows);
        }

        public static byte[] GetFoto(int idProducto)
        {
            byte[] fotos = Array.Empty<byte>();

            string query = "SELECT Foto.IdFoto, Foto.IdProducto, Foto.Foto FROM GetFotoProd(@IdProducto) AS Foto";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProducto", idProducto);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);
           
            while (dr.Read())
            {
                fotos = (dr["Foto"] != DBNull.Value ? (byte[])dr["Foto"] : Array.Empty<byte>());
            }

			dB.Close();

			return fotos;
        }

        public int SetSessionClienteSel(int IdPersona)
        {
            HttpContext.Session.SetString("ClienteSel", IdPersona.ToString());

            return Convert.ToInt32(HttpContext.Session.GetString("ClienteSel"));
        }

        public static List<Clientes> GetClientesSeleccion(string UserName)
        {
            List<Clientes> ClientesSeleccion = new();

            string query = "SELECT Persona.IdPersona, Persona.Nombre, Persona.IdEmailFuncion FROM GetPersonasByEmail(@UserName) AS Persona";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@UserName", UserName);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                Clientes Cliente = new Clientes();

                Cliente.IdPersona = (int)dr["IdPersona"];
                Cliente.Nombre = (string)dr["Nombre"];
                Cliente.IdTipoUsuario = (byte)dr["IdEmailFuncion"];

                ClientesSeleccion.Add(Cliente);
            }

			dB.Close();

			return ClientesSeleccion;
        }

        public bool AsignarMonedas(int Origen, int Destino, int IdMotivo, int Monedas, int IdProducto, int Usuario)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[6];
            parameters[0] = new SqlParameter("@origen", Origen);
            parameters[1] = new SqlParameter("@destino", Destino);
            parameters[2] = new SqlParameter("@idmotivo", IdMotivo);
            parameters[3] = new SqlParameter("@monedas", Monedas == 0 ? null : Monedas);
            parameters[4] = new SqlParameter("@idproducto", IdProducto == 0 ? null : IdProducto);
            parameters[5] = new SqlParameter("@usuario", Usuario);

            bool ret = Convert.ToBoolean(dB.ExecuteReturnValue("MonedasAsignar", ref parameters));
            return ret;
        }

        public bool AsignarMonedasVisitaStand(int Origen, int Destino, int Usuario)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@origen", Origen);
            parameters[1] = new SqlParameter("@destino", Destino);
            parameters[2] = new SqlParameter("@usuario", Usuario);

            bool ret = Convert.ToBoolean(dB.ExecuteReturnValue("MonedasStand", ref parameters));
            return ret;
        }

        public bool AsignarMonedasVisitaProducto(int Origen, int Destino, int IdProducto, int Usuario)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[4];
            parameters[0] = new SqlParameter("@origen", Origen);
            parameters[1] = new SqlParameter("@destino", Destino);
            parameters[2] = new SqlParameter("@idproducto", IdProducto);
            parameters[3] = new SqlParameter("@usuario", Usuario);

            bool ret = Convert.ToBoolean(dB.ExecuteReturnValue("MonedasProducto", ref parameters));

            return ret;
        }
    }
}