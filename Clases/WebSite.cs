using ExpoMR.Datos;
using ExpoMR.Clases;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Collections.Specialized;
using System.Web;
using ExpoMR.Models;
using Microsoft.AspNetCore.Hosting;
using System.Drawing;
using System.Drawing.Imaging;

namespace ExpoMR.Clases
{
	public class WebSite
	{
        public static HttpRequest Request { get; private set; }

        public static byte[] GetImageDefault(IWebHostEnvironment environment, string nameImage)
        {          
            var path = environment.WebRootFileProvider.GetFileInfo("Imagenes/" + nameImage).PhysicalPath;
            var fileStream = System.IO.File.OpenRead(path);
            byte[] bytes;

            using (var binaryReader = new BinaryReader(fileStream))
            {
                bytes = binaryReader.ReadBytes((int)fileStream.Length);
            }

            return bytes;
        }

        public static byte[] ConvertToByteArray(IFormFile imagen)
        {
            if (imagen != null && imagen.Length > 0)
            {
                var ms = new MemoryStream();

                Image image = Image.FromStream(imagen.OpenReadStream());
                image.Save(ms, ImageFormat.Jpeg);

                return ms.ToArray();
            }

            return Array.Empty<byte>();
        }

        public static string ConvertToBase64(byte[] Objeto)
        {
            if (Objeto == null || Objeto.Length == 0)
            {
                return null;
            }

            return Convert.ToBase64String(Objeto);
        }

        public enum TipoPromocion
        {
            Visita_Stand = 1,
            Acumulable = 2,
            No_Acumulable = 3,
            Escalonada = 4
        }

        public static int SaldoMonedas(int idPersona)
        {
            DB dB = new DB();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@idpersona", idPersona);

            int ret = dB.ExecuteReturnValue("MonedasSaldo", ref parameters);

            return ret < 0 ? 0 : ret;
        }

        public static string GetRuleta(int cliente)
        {
            string CssClass ="";
            DB dB = new DB();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@cliente", cliente);

            string ret = dB.ExecuteScalar("Select dbo.GetRuleta(@cliente) as Ruleta", ref parameters, System.Data.CommandType.Text).ToString();

            switch (ret) {
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

        public static string GetLockEndUsers() 
        {

            DB dB = new DB();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@variable", "LockEndUsers");         

            string ret = dB.ExecuteScalar("SELECT mSysVar.Valor FROM mSysVar WHERE mSysVar.Variable = @Variable", ref parameters, System.Data.CommandType.Text).ToString();

            return ret;
        }

        public static int CountProducts(int cliente, int usuario)
        {
            DB dB = new DB();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@cliente", cliente);
            parameters[1] = new SqlParameter("@usuario", usuario);
            int ret = (int)dB.ExecuteScalar("ExpoCountProducts", ref parameters);

            return ret;
        }

        public static string GetSearchUrl(string uri, string parameter, string value)
        {
            UriBuilder uribuilder = new UriBuilder(uri);
            if (!uribuilder.Path.EndsWith("/Stand.aspx"))
            {
                uribuilder.Path = uribuilder.Path.Substring(0, uribuilder.Path.LastIndexOf('/')) + "/Stand.aspx";
                uribuilder.Query = "";
            }

            NameValueCollection parameters = HttpUtility.ParseQueryString(uribuilder.Query);
            if (parameters.Get(parameter) == null & value != "")
                parameters.Add(parameter, value);
            else if (parameters.Get(parameter) != null & value != "")
                parameters[parameter] = value;
            else if (parameters.Get(parameter) != null & value == "")
                parameters.Remove(parameter);

            uribuilder.Query = parameters.ToString();

            return uribuilder.Uri.ToString();
        }

        public static string TimeFormat(string input)
        {
            string result = input;
            int time = 0;
            Int32.TryParse(input.Substring(0, 2), out time);

            if (time > 12)
            {
                time = time - 12;
                result = time.ToString() + input.Substring(2);
                result = result.Replace('a', 'p');
            }

            return result;
        }

        public static void InsertFeNotificacion(int IdEmail)
        {
            DB dB = new DB();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdEmail", IdEmail);

            dB.ExecuteNonQuery("UpdateFeUltimaNotificacion", ref parameters);
        }

        public static void InsertIinvitacion(int IdEmail, bool Tipo, string Msg)
        {
            DB dB = new DB();
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@IdEmail", IdEmail);
            parameters[1] = new SqlParameter("@Tipo", Tipo);
            parameters[2] = new SqlParameter("@Msg", Msg);

            dB.ExecuteNonQuery("LogInvitacion", ref parameters);
        }

        public static int InicioExpo(string UserName = "")
        {

            int Result = 0;
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@username", UserName);
            DB dB = new DB();
            SqlDataReader dr = dB.ExecuteReader("ExpoInicio", ref parameters);
            if (dr.Read())
            {
                if ((bool)dr["Acceso"])
                {
                    Result = 3; //La Expo para este Usuario ha concluido
                }
                else
                {
                    switch (Convert.ToInt32(dr["IdTipoUsuario"]))
                    {
                        case 1://cliente
                            Result = 2; //Solo Se Perimte el Cambio de contraseña
                            break;
                        case 2://proveedor;
                            Result = 2; //Solo se permite el cambio de Contraseña
                            break;
                        case 3://administrador
                            Result = 1;
                            break;
                        case 4://empleado
                            Result = 1;
                            break;
                    }
                }
            }
            else
            {
                Result = 0; // Hubo un error al consultar
            }

            return Result;
        }

        public static bool FechaExpoActiva(string usuario)
        {
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@username", usuario);
            DB dB = new DB();
            SqlDataReader dr = dB.ExecuteReader("ExpoInicio", ref parameters);

            if (dr.Read())
            {
                if ((bool)dr["Acceso"])
                {
                    dr.Close();
                    dB.Close();
                    return true;
                }
                else
                {
                    dr.Close();
                    dB.Close();
                    return false;
                }
            }

            return false;
        }

        public static void AddSucursalUser(int idEmail, int Sucursal)
        {
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@idSucursal", Sucursal);
            parameters[1] = new SqlParameter("@idEmail", idEmail);

            DB dB = new DB();
            SqlDataReader dr = dB.ExecuteReader("AddSucursalUsuario", ref parameters);
        }

        public static void ChangeVIP(int idEmail, bool vip)
        {
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@IdEmail", idEmail);
            parameters[1] = new SqlParameter("@VIP", vip);

            DB dB = new DB();
            SqlDataReader dr = dB.ExecuteReader("ChangeUsuarioVIP", ref parameters);
        }

        public static int UsuarioEliminar(int idEmail)
        {
            DB dB = new DB();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdEmail", idEmail);

            int ret = dB.ExecuteReturnValue("DeleteUsuario", ref parameters);

            return ret;
        }

        public static bool ValidaExpoVIP()
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[0];

            bool ret = Convert.ToBoolean(dB.ExecuteReturnValue("ValidaExpoVIP", ref parameters));

            return ret;
        }

        public static bool ValidaExpo()
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[0];

            bool ret = Convert.ToBoolean(dB.ExecuteReturnValue("GetFechaExpo", ref parameters));

            return ret;
        }

        public static bool ValidaRolUsuario(string Username, string Rol) 
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@UserName", Username);
            parameters[1] = new SqlParameter("@Rol", Rol);

            bool ret = Convert.ToBoolean(dB.ExecuteReturnValue("ValidaRolUsuario", ref parameters));

            return ret;
        }
    }
}