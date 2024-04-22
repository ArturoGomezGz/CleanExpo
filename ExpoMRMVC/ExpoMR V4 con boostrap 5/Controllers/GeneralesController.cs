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
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@idpersona", HttpContext.Session.GetString("ClienteSel"));

            int ret = dB.ExecuteReturnValue("MonedasSaldo", ref parameters);

            return ret;
        }

        public string GetRuleta(int IdPersona)
        {
            DB dB = new DB();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@cliente", IdPersona);

            string ret = dB.ExecuteScalar("Select dbo.GetRuleta(@cliente) as Ruleta", ref parameters, System.Data.CommandType.Text).ToString();
            string CssClass = "";

            switch (ret)
            {
                case "0":
                    CssClass = "btn-ruleta-inactiva";
                    break;
                case "1":
                    CssClass = "btn-ruleta-clasica";
                    break;
                case "2":
                    CssClass = "btn-ruleta-dorada";
                    break;
                case "3":
                    CssClass = "btn-ruleta-platinum";
                    break;
                default:
                    CssClass = "btn-ruleta-jugada";
                    break;
            }

            return CssClass;
        }

        public int GetCarrito(int IdPersona)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@cliente", IdPersona);
            parameters[1] = new SqlParameter("@usuario", IdPersona);

            int ret = (int)dB.ExecuteScalar("ExpoCountProducts", ref parameters);

            return ret;
        }

        public int GetIdPersona(string UserName)  //se tendra que eliminar puesto que ya no se utilizara
        {
            string query = "Select IdPersona" +
                " from mUsuarios" +
                " where UserName ='"+UserName+"'";
            DataTable dataTable = SqlQueryDT("ExpoMRCS", query);

            int IdPersona = dataTable.Rows[0].Field<int>(0);
            return IdPersona;
        }

        public int GetCliente(string UserName)
        {
            string query = "Select Referencia" +
                " from mUsuarios" +
                " where UserName ='" + UserName+"'";
            DataTable dataTable = SqlQueryDT("ExpoMRCS", query);

            int IdPersona = dataTable.Rows[0].Field<int>(0);

            return IdPersona;
        }

        public static int GetIdPersonas(string UserName)
        {
            string query = "Select IdPersona" +
                " from mUsuarios" +
                " where UserName ='"+UserName+"'";
            DataTable dataTable = SqlQueryDT("ExpoMRCS", query);

            int IdPersona = dataTable.Rows[0].Field<int>(0);

            return IdPersona;
        }

        public static int GetIdProveedor(string UserName)
        {
            string query = "Select Referencia" +
                " from mUsuarios" +
                " where UserName ='" + UserName + "'";
            DataTable dataTable = SqlQueryDT("ExpoMRCS", query);

            int IdPersona = dataTable.Rows[0].Field<int>(0);

            return IdPersona;
        }

        public static int GetClientes(string UserName)
        {
            string query = "Select Referencia" +
                " from mUsuarios" +
                " where UserName ='" + UserName+"'";
            DataTable dataTable = SqlQueryDT("ExpoMRCS", query);

            int IdPersona = dataTable.Rows[0].Field<int>(0);

            return IdPersona;
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

            return Json(jsonData.rows);
        }

        public static DataTable SqlQueryDT(string dataBase, string query)
        {
            string ConectionSql="";

            #if (TESTMODE)
                switch (dataBase)
                {
                    case "ExpoMRCS":
                        ConectionSql = "Server=localhost; Database=ExpoMR; User ID=usrapp;Password=tangeri;Trusted_Connection=false;MultipleActiveResultSets=true";
                        break;
                    case "IndigoCS":
                        ConectionSql = "Server=localhost; Database=Indigo; User ID=usrapp;Password=tangeri;Trusted_Connection=false;MultipleActiveResultSets=true";
                        break;
                    case "FotosCS":
                        ConectionSql = "Server=localhost; Database=Fotos; User ID=usrapp;Password=tangeri;Trusted_Connection=false;MultipleActiveResultSets=true";
                        break;
                }
            #else
                switch (dataBase)
                {
                    case "ExpoMRCS":
                        ConectionSql = "Server=192.168.100.10; Database=ExpoMR; User ID=usrapp;Password=tangeri;Trusted_Connection=false;MultipleActiveResultSets=true";
                        break;
                    case "IndigoCS":
                        ConectionSql = "Server=192.168.100.10; Database=Indigo; User ID=usrapp;Password=tangeri;Trusted_Connection=false;MultipleActiveResultSets=true";
                        break;
                    case "FotosCS":
                        ConectionSql = "Server=192.168.100.10; Database=Fotos; User ID=usrapp;Password=tangeri;Trusted_Connection=false;MultipleActiveResultSets=true";
                        break;
                }            
            #endif

            DataTable dt = new();
            SqlConnection cnn = new(ConectionSql);
            SqlCommand cmd = new();
            SqlDataAdapter sqlDA; cnn.Open();
            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dt);
            cnn.Close();

            return dt;
        }

        public static void SqlQuery(string dataBase, string query)
        {
            SqlConnection cnn = new(_configuration["ConnectionStrings:" + dataBase]);
            SqlCommand cmd = new();
            cnn.Open();
            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;
            cmd.ExecuteNonQuery();
            cnn.Close();
        }

        public static DataTable SqlStoreProcedure(string dataBase, string StoreProcedure, SqlParameter[] parameter)
        {
            DataTable dt = new();
            SqlConnection cnn = new(_configuration["ConnectionStrings:" + dataBase]);
            SqlCommand cmd = new();
            SqlDataAdapter sqlDA;
            cnn.Open();
            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;
            //// 3. add parameter to command, which will be passed to the stored procedure
            cmd.Parameters.Add(parameter);
            cmd.CommandText = StoreProcedure;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dt);
            cnn.Close();

            return dt;
        }

        public byte[] GetfotosPromo(string type, int id)
        {
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@type", type);
            parameters[1] = new SqlParameter("@id", id);
            DB dB = new();
            dB.ExecuteReader("ExpoGetImage", ref parameters);
            byte[] f = Array.Empty<byte>();

            return f;
        }

        public static byte[] GetFoto(int idProducto)
        {
            string query = "SELECT Foto FROM mFotos where IdProducto =" + idProducto;
            DataTable dataTable = SqlQueryDT("FotosCS", query);
            byte[] fotos;

            if (dataTable.Rows.Count > 0)
                fotos = dataTable.Rows[0].Field<byte[]>(0);
            else
                fotos = Array.Empty<byte>();

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

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@UserName", UserName);

            SqlDataReader dr = dB.ExecuteReader("GetPersonasByEmail", ref parameters);

            while (dr.Read())
            {
                Clientes Cliente = new Clientes();

                Cliente.IdPersona = (int)dr["IdPersona"];
                Cliente.Nombre = (string)dr["Nombre"];

                ClientesSeleccion.Add(Cliente);
            }

            return ClientesSeleccion;
        }
    }
}