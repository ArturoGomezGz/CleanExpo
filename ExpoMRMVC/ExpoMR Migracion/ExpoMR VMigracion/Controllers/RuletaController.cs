using ExpoMR.Clases;
using ExpoMR.Models;
using ExpoMR.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ExpoMR.Controllers
{
    [Authorize]
    public class RuletaController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _rolManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private static IConfiguration _configuration;

        //Variables para verificar La sesión Y el Usuario
        public RuletaController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender, RoleManager<IdentityRole> rolManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _rolManager = rolManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _rolManager = rolManager;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "MiCuenta");
            }

            return View();
        }
        public async Task<IActionResult> PlayRuleta()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "MiCuenta");
            }

            return View();
        }
        public static string GetData()
        {
            //SqlParameter[] parameters = new SqlParameter[2];
            //parameters[0] = new SqlParameter("@cliente", 11942);
            //parameters[1] = new SqlParameter("@usuario", 11942);
            //DB dB = new DB();
            //SqlDataReader dr = dB.ExecuteReader("RuletaGiro", ref parameters);
            //if (dr.Read())
            //{
            //    return dr["GradosGiro"] + "," + dr["IdPremio"] + "," + dr["Premio"] + "," + dr["Descripcion"];
            //}
            //else
            //{
            //    return "0,,,";
            //}
            return "";

        }
        public static List<PremiosRuletaModel> GetPremios() {

            //SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA; cnn.Open();
            //cmd.CommandText = "Select mPremios.IdPremio, mPremios.Premio, mPremios.Descripcion, mPremios.Seccion, mPremios.Foto"
            //                   + " From mPremios"
            //                   + " Order by mPremios.Seccion";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();
            string  query = "Select mPremios.IdPremio, mPremios.Premio, mPremios.Descripcion, mPremios.Seccion, mPremios.Foto"
                               + " From mPremios"
                               + " Order by mPremios.Seccion";
            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            List<PremiosRuletaModel> Premios = new List<PremiosRuletaModel>();

            try {
                foreach (DataRow row in dataTable.Rows)
                {
                    PremiosRuletaModel premio = new PremiosRuletaModel();
                    premio.premio = row.Field<string>(1);
                    premio.descripcion = row.Field<string>(2);
                    premio.seccion = "../Imagenes/Seccion"+row.Field<byte>(3)+".png";
                    premio.fotoPremio = row.Field<byte[]>(4);
                    Premios.Add(premio);
                }
            } catch (Exception e) {
            
            }
            

            return Premios;
        }
    }
}
