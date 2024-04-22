using ExpoMR.Clases;
using ExpoMR.Models;
using ExpoMR.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace ExpoMR.Controllers
{
    //[Authorize]
    public class GeneralesController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _rolManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private static IConfiguration _configuration;

        //Variables para verificar La sesión Y el Usuario
        public GeneralesController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender, RoleManager<IdentityRole> rolManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _rolManager = rolManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _configuration = configuration;
        }
        public int GetMonedas() {
            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            int IdPersona = GetIdPersona(username);

            DB dB = new DB();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@idpersona", IdPersona);
            int ret = dB.ExecuteReturnValue("MonedasSaldo", ref parameters);

            return ret;
        }
        public int GetCarrito() {
            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            int IdPersona = GetIdPersona(username);
            int cliente = GetCliente(username);

            DB dB = new DB();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@cliente", cliente);
            parameters[1] = new SqlParameter("@usuario", IdPersona);
            int ret = (int)dB.ExecuteScalar("ExpoCountProducts", ref parameters);

            return ret;
        }
        public int GetIdPersona(string UserName) {
            string query = "Select IdPersona" +
                " from mUsuarios" +
                " where UserName ='"+UserName+"'";
            DataTable dataTable = sqlQueryDT("ExpoMRCS", query);

            int IdPersona = dataTable.Rows[0].Field<int>(0);
            return IdPersona;
        }
        public int GetCliente(string UserName) {
            string query = "Select Referencia" +
                " from mUsuarios" +
                " where UserName ='" + UserName+"'";
            DataTable dataTable = sqlQueryDT("ExpoMRCS", query);

            int IdPersona = dataTable.Rows[0].Field<int>(0);
            return IdPersona;
        }
        public static int GetIdPersonas(string UserName) {
            string query = "Select IdPersona" +
                " from mUsuarios" +
                " where UserName ='"+UserName+"'";
            DataTable dataTable = sqlQueryDT("ExpoMRCS", query);

            int IdPersona = dataTable.Rows[0].Field<int>(0);
            return IdPersona;
        }
        public static int GetIdProveedor(string UserName)
        {
            string query = "Select Referencia" +
                " from mUsuarios" +
                " where UserName ='" + UserName + "'";
            DataTable dataTable = sqlQueryDT("ExpoMRCS", query);

            int IdPersona = dataTable.Rows[0].Field<int>(0);
            return IdPersona;
        }
        public static int GetClientes(string UserName) {
            string query = "Select Referencia" +
                " from mUsuarios" +
                " where UserName ='" + UserName+"'";
            DataTable dataTable = sqlQueryDT("ExpoMRCS", query);

            int IdPersona = dataTable.Rows[0].Field<int>(0);
            return IdPersona;
        }

        [HttpPost]
        public JsonResult GetRegistroMonedas()
        {
            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            int IdPersona = GetIdPersona(username);

            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand cmd = new SqlCommand("MonedasSaldoDetalle", cnn);
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA;
            cnn.Open();
            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;
            //// 3. add parameter to command, which will be passed to the stored procedure
            cmd.Parameters.Add(new SqlParameter("@idpersona", IdPersona));
            //cmd.Parameters.Add("@NewId", SqlDbType.Int).Direction = ParameterDirection.Output;
            SqlParameter pvSaldo = new SqlParameter();
            pvSaldo.ParameterName = "@saldo";
            pvSaldo.DbType = DbType.Int32;
            pvSaldo.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(pvSaldo);

            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dataTable);
            cnn.Close();

            RegistroMonedasModel model = new RegistroMonedasModel();

            var jsonData = new
            {
                rows = (
                       from c in dataTable.AsEnumerable()
                       select new
                       {
                           fecha = c[1],
                           concepto = c[5],
                           importe = c[3],
                           saldo = c[2]
                       }).ToArray()
            };


            return Json(jsonData.rows);
        }
        public static DataTable sqlQueryDT(string dataBase, string query)
        {
            DataTable dt = new DataTable();
            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:" + dataBase]);
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter sqlDA; cnn.Open();
            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dt);
            cnn.Close();
            return dt;
        }
        public static void sqlQuery(string dataBase, string query)
        {
            DataTable dt = new DataTable();
            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:" + dataBase]);
            SqlCommand cmd = new SqlCommand();
            cnn.Open();
            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;
            cmd.ExecuteNonQuery();
            cnn.Close();
        }
        public static DataTable sqlStoreProcedure(string dataBase, string StoreProcedure, SqlParameter[] parameter)
        {
            DataTable dt = new DataTable();
            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:" + dataBase]);
            SqlCommand cmd = new SqlCommand();
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
            DB dB = new DB();
            SqlDataReader dr = dB.ExecuteReader("ExpoGetImage", ref parameters);
            byte[] f = new byte[0];

            return f;
        }
    }
}
