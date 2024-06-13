using ExpoMR.Clases;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using System.Globalization;
using ExpoMR.Datos;
using Microsoft.EntityFrameworkCore;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Cliente,ClienteVIP")]
    public class RuletaController : Controller
    {
        //Variables para verificar La sesión Y el Usuario
        private static IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;

        public RuletaController(UserManager<IdentityUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _configuration = config;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            List<InfoRuletas> ListaInforuletas = new();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@cliente", HttpContext.Session.GetString("ClienteSel"));

            SqlDataReader dr = dB.ExecuteReader("RuletaInfo", ref parameters);

            while (dr.Read())
            {
                InfoRuletas Ruleta = new InfoRuletas();

                Ruleta.IdRuleta = (byte)dr["IdRuleta"];
                Ruleta.Ruleta = (string)dr["Ruleta"];
                Ruleta.Monedas = (int)dr["Monedas"];
                Ruleta.Activa = (int)dr["Activa"];

                ListaInforuletas.Add(Ruleta);
            }

			dB.Close();

			ListaInforuletas.ForEach(i =>  i.RuletaNormalized = RemoveAccents(i.Ruleta));

            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "MiCuenta");
            }
            else
            {
                bool isFechaExpoVIP = ExpoMR.Clases.WebSite.ValidaExpoVIP();

                if (ExpoMR.Clases.WebSite.ValidaRolUsuario(currentUser.Email, "ClienteVIP") == true)
                {
                    if (isFechaExpoVIP == true)
                    {
                        return View(ListaInforuletas);
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
                        return View(ListaInforuletas);
                    }
                }
            }
        }

        public async Task<IActionResult> PlayRuleta(int IdRuleta)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "MiCuenta");
            }
            else
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@IdRuleta", IdRuleta);

                int monedasRuleta = dB.ExecuteReturnValue("GetMonedasRuleta", ref parameters);
                int monedasCliente = WebSite.SaldoMonedas(Convert.ToInt32(HttpContext.Session.GetString("ClienteSel")));

                if (monedasCliente < monedasRuleta)
                {
                    return RedirectToAction("Index", "Ruleta");
                }
                else
                {
                    PlayRuletaViewModel Ruleta = new()
                    { 
                        IdRuleta = IdRuleta
                    };

                    bool isFechaExpoVIP = ExpoMR.Clases.WebSite.ValidaExpoVIP();

                    if (ExpoMR.Clases.WebSite.ValidaRolUsuario(currentUser.Email, "ClienteVIP") == true)
                    {
                        if (isFechaExpoVIP == true)
                        {
                            return View(Ruleta);
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
                            return View(Ruleta);
                        }
                    }
                }
            }
        }

        public static List<PremioRuleta> GetImageRuleta(int idRuleta)
        {
            List<PremioRuleta> PremiosRuleta = new();

            string query = "SELECT Ruleta.IdPremio, Ruleta.Premio, Ruleta.Descripcion, Ruleta.IdRuleta, Ruleta.Seccion, Ruleta.Foto, Ruleta.Cantidad FROM GetPremiosRuleta(@IdRuleta) AS Ruleta";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdRuleta", idRuleta);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                PremioRuleta Premio = new PremioRuleta();

                Premio.IdPremio = (Int16)dr["IdPremio"];
                Premio.Premio = (string)dr["Premio"];
                Premio.Descripcion = (string)dr["Descripcion"];
                Premio.IdRuleta = (byte)dr["IdRuleta"];
                Premio.Seccion = (byte)dr["Seccion"];
                Premio.Foto = (byte[])dr["Foto"];
                Premio.Cantidad = (Int16)dr["Cantidad"];

                PremiosRuleta.Add(Premio);
            }

			dB.Close();

			return PremiosRuleta;
        }

        public static List<PremioRuleta> GetPremios(int idRuleta)
        {
            List<PremioRuleta> PremiosRuleta = new();

            string query = "SELECT Ruleta.IdPremio, Ruleta.Premio, Ruleta.Descripcion, Ruleta.IdRuleta, Ruleta.Seccion, Ruleta.Foto, Ruleta.Cantidad FROM GetPremiosRuleta(@IdRuleta) AS Ruleta";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdRuleta", idRuleta);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                PremioRuleta Premio = new PremioRuleta();

                Premio.IdPremio = (Int16)dr["IdPremio"];
                Premio.Premio = (string)dr["Premio"];
                Premio.Descripcion = (string)dr["Descripcion"];
                Premio.IdRuleta = (byte)dr["IdRuleta"];
                Premio.Seccion = (byte)dr["Seccion"];
                Premio.Foto = (byte[])dr["Foto"];
                Premio.Cantidad = (Int16)dr["Cantidad"];

                PremiosRuleta.Add(Premio);
            }

			dB.Close();

			return PremiosRuleta;
        }

        public JsonResult GetData(int IdRuleta)
        {
            ResponseModel rm = new();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdRuleta", IdRuleta);

            int monedasRuleta = dB.ExecuteReturnValue("GetMonedasRuleta", ref parameters);
            int monedasCliente = WebSite.SaldoMonedas(Convert.ToInt32(HttpContext.Session.GetString("ClienteSel")));

            if (monedasCliente < monedasRuleta)
            {
                rm.Response = false;
                rm.Result = "0,,,";
                rm.Message = "No tienes monedas sufucientes para girar la Ruleta";
            }
            else
            {
                List<RuletaGiro> ruletaGiro = new();

                DB dB2 = new();
                SqlParameter[] parameters2 = new SqlParameter[2];
                parameters2[0] = new SqlParameter("@Cliente", HttpContext.Session.GetString("ClienteSel"));
                parameters2[1] = new SqlParameter("@usuario", HttpContext.Session.GetString("ClienteSel"));
                SqlDataReader dr = dB2.ExecuteReader("RuletaGiro", ref parameters2);

                while (dr.Read())
                {
                    RuletaGiro Giro = new RuletaGiro();

                    Giro.GradosGiro = (decimal)dr["GradosGiro"];
                    Giro.IdPremio = (Int16)dr["IdPremio"];
                    Giro.Premio = (string)dr["Premio"];
                    Giro.Descripcion = (string)dr["Descripcion"];

                    ruletaGiro.Add(Giro);
                }

                if (ruletaGiro != null && ruletaGiro.Count() > 0)
                {
                    string foto = "";
                    string query = "SELECT mPremios.IdPremio, mPremios.IdRuleta, mPremios.Seccion, mPremios.Foto " +
                                    "FROM mPremios " +
                                    "WHERE mPremios.IdPremio = @IdPremio";

                    DB dB3 = new();
                    SqlParameter[] parameters3 = new SqlParameter[1];
                    parameters3[0] = new SqlParameter("@IdPremio", ruletaGiro[0].IdPremio);

                    SqlDataReader dr2 = dB3.ExecuteReader(query, ref parameters3, commandType: CommandType.Text);

                    while (dr2.Read())
                    {
                        foto = WebSite.ConvertToBase64((dr2["Foto"] != DBNull.Value ? (byte[])dr2["Foto"] : Array.Empty<byte>()));
                    }

                    rm.Response = true;
                    rm.Result = ruletaGiro[0].GradosGiro + "," + ruletaGiro[0].IdPremio + "," + ruletaGiro[0].Premio + "," + ruletaGiro[0].Descripcion + "," + foto;
                }
                else
                {
                    rm.Response = false;
                    rm.Result = "0,,,";
                }
            }

            return Json(rm);
        }

        static string RemoveAccents(string text)
        {
            string normalized = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString();
        }
    }
}
