using ExpoMR.Datos;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Collections.Specialized;
using System.Web;

namespace ExpoMR.Clases
{
	public class WebSite
	{
        public static HttpRequest Request { get; private set; }
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
            return ret;
        }
        public static int GetRuleta(int cliente)
        {
            DB dB = new DB();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@cliente", cliente);
            int ret = Convert.ToInt32(dB.ExecuteScalar("Select ISNULL(dbo.GetRuleta(@cliente), 0) as Ruleta", ref parameters, System.Data.CommandType.Text));
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
        //public static int CreateUser(int tipo, string vip, string eMail, int idPersona, int sucursal, ref string Message, bool VIP)
        //{
        //    int ret = 0;
        //    string MensageError = "";

        //    SqlParameter[] parameters = new SqlParameter[5];
        //    parameters[0] = new SqlParameter("@idpersona", idPersona);
        //    parameters[1] = new SqlParameter("@email", eMail);
        //    parameters[2] = new SqlParameter("@tipousuario", tipo);
        //    parameters[3] = new SqlParameter("@sucursal", sucursal);
        //    parameters[4] = new SqlParameter("@VIP", VIP);

        //    DB dB = new DB();
        //    SqlDataReader dr = dB.ExecuteReader("UsuariosAgregar", ref parameters);
        //    dr.Read();

        //    if ((int)dr["Agregado"] == 1)
        //    {
        //        string Rol = "";
        //        string newPwd = Membership.GeneratePassword(9, 3);
        //        bool TipoVip = tipo == 1 ? true : false;
        //        ApplicationDbContext context = new ApplicationDbContext();
        //        var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
        //        UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager) { AllowOnlyAlphanumericUserNames = false }; //se deshabilita la validacion del username para que este solo contenga letras y numeros
        //        var user = new ApplicationUser() { UserName = eMail, PasswordHash = newPwd, Email = eMail, EmailConfirmed = true, TwoFactorEnabled = false, LockoutEnabled = false, LockoutEndDateUtc = Convert.ToDateTime("1754-01-01 00:00:00.000"), AccessFailedCount = 0 };

        //        UserManager.Create(user, newPwd);

        //        if (tipo == 1)     //Cliente
        //            Rol = "Cliente";
        //        else if (tipo == 2)    //Proveedor
        //            Rol = "Proveedor";
        //        else                   //Capturista - Empleados
        //            Rol = "Capturista";

        //        if (tipo == 1 && VIP)  //Cliente VIP
        //        {
        //            Rol = "ClienteVIP";
        //        }

        //        UserManager.AddToRole(user.Id, Rol);

        //        try
        //        {
        //            Notifications notification = new Notifications();
        //            if (notification.SendWelcome(dr["Nombre"].ToString(), eMail, newPwd, VIP))
        //            {
        //                ret = 2; //se envia correctamente la notificación en a los correos
        //                MensageError = notification.Message;
        //                InsertFeNotificacion(idPersona);
        //                InsertIinvitacion(idPersona, true, "SUCCESS");
        //            }
        //            else
        //            {
        //                ret = 1; //Hubo algún problema al enviar la notificación a los correos
        //                MensageError = notification.Message;
        //                InsertIinvitacion(idPersona, VIP, MensageError);
        //            }

        //            Message = notification.Message;
        //        }
        //        catch (Exception Ex)
        //        {
        //            InsertIinvitacion(idPersona, VIP, Ex.Message);
        //        }
        //    }
        //    else
        //    {
        //        Message = "Ya existe un Usuario en ExpoMR con ese Correo Electronico";
        //    }

        //    return ret;
        //}
        //public static void CrearPassword(string Email)
        //{

        //    string newPwd = Membership.GeneratePassword(9, 3);

        //    ApplicationDbContext context = new ApplicationDbContext();
        //    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
        //    UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager) { AllowOnlyAlphanumericUserNames = false }; //se deshabilita la validacion del username para que este solo contenga letras y numeros
        //    var user = new ApplicationUser() { UserName = Email, PasswordHash = newPwd, Email = Email, EmailConfirmed = true, TwoFactorEnabled = false, LockoutEnabled = false, LockoutEndDateUtc = Convert.ToDateTime("1754-01-01 00:00:00.000"), AccessFailedCount = 0 };

        //    UserManager.Create(user, newPwd);
        //    UserManager.AddToRole(user.Id, "Cliente");

        //}
        //public static int ResetPasswordUser(string eMail, string NewPwd, string ConfirmPwd)
        //{
        //    int ret = 0;

        //    ApplicationDbContext context = new ApplicationDbContext();
        //    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
        //    UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager) { AllowOnlyAlphanumericUserNames = false }; //se deshabilita la validacion del username para que este solo contenga letras y numeros
        //    var user = UserManager.FindByName(eMail);

        //    if (user != null)
        //    {
        //        user.PasswordHash = UserManager.PasswordHasher.HashPassword(NewPwd);
        //        var res = UserManager.Update(user);

        //        if (res.Succeeded)
        //        {
        //            ret = 1;
        //        }
        //    }

        //    return ret;
        //}
        //public static void AddRoleUser(string eMail, string Rol)
        //{
        //    ApplicationDbContext context = new ApplicationDbContext();
        //    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
        //    UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager) { AllowOnlyAlphanumericUserNames = false }; //se deshabilita la validacion del username para que este solo contenga letras y numeros
        //    var user = UserManager.FindByName(eMail);
        //    try
        //    {
        //        UserManager.AddToRole(user.Id, Rol);
        //    }
        //    catch (Exception e)
        //    {
        //        string message = e.Message;
        //    }
        //}
        //public static void DeleteRoleUser(string eMail, string Rol)
        //{
        //    ApplicationDbContext context = new ApplicationDbContext();
        //    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
        //    UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager) { AllowOnlyAlphanumericUserNames = false }; //se deshabilita la validacion del username para que este solo contenga letras y numeros
        //    var user = UserManager.FindByName(eMail);

        //    UserManager.RemoveFromRole(user.Id, Rol);
        //}
        //public static string[] GetRoleUser(string eMail)
        //{
        //    ApplicationDbContext context = new ApplicationDbContext();
        //    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
        //    UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager) { AllowOnlyAlphanumericUserNames = false }; //se deshabilita la validacion del username para que este solo contenga letras y numeros
        //    string[] UserRoles = UserManager.GetRoles(UserManager.FindByName(eMail).Id).ToArray();

        //    return UserRoles;
        //}
        //public static int DeleteUser(string eMail)
        //{
        //    int ret = 0;

        //    ApplicationDbContext context = new ApplicationDbContext();
        //    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
        //    UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager) { AllowOnlyAlphanumericUserNames = false }; //se deshabilita la validacion del username para que este solo contenga letras y numeros
        //    var user = UserManager.FindByName(eMail);
        //    string[] UserRoles = UserManager.GetRoles(user.Id).ToArray();

        //    if (user != null)
        //    {
        //        var res = UserManager.RemoveFromRoles(user.Id, UserRoles);

        //        if (res.Succeeded)
        //        {
        //            UserManager.Delete(user);
        //            ret = 1;
        //        }
        //    }
        //    else
        //    {
        //        UserManager.Delete(user);
        //        ret = 1;
        //    }

        //    return ret;
        //}
        //public static int unlock_lockUser(string eMail)
        //{
        //    int ret = 0;

        //    ApplicationDbContext context = new ApplicationDbContext();
        //    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
        //    UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager) { AllowOnlyAlphanumericUserNames = false }; //se deshabilita la validacion del username para que este solo contenga letras y numeros
        //    var user = UserManager.FindByName(eMail);

        //    if (user != null)
        //    {
        //        user.PasswordHash.ToString();



        //        var lockuser = UserManager.IsLockedOut(user.Id);

        //        if (lockuser == true)
        //        {
        //            user.LockoutEndDateUtc = Convert.ToDateTime("1754-01-01 00:00:00.000");
        //            user.LockoutEnabled = false;
        //            user.AccessFailedCount = 0;
        //            UserManager.Update(user);

        //            ret = 1;
        //        }
        //        else
        //        {
        //            user.LockoutEndDateUtc = Convert.ToDateTime(DateTimeOffset.Now.AddDays(5).ToString());
        //            user.LockoutEnabled = true;
        //            UserManager.Update(user);

        //            ret = 2;
        //        }
        //    }


        //    return ret;
        //}
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
        //public static int GeneratePassword(string eMail, int TipoUsuario, ref string Message, int IdPersona, bool TipoVIp)
        //{
        //    int result = 0;
        //    string Rol = "";
        //    string newPwd = Membership.GeneratePassword(9, 3);
        //    string MensageError = "";
        //    ApplicationUser user;

        //    ApplicationDbContext context = new ApplicationDbContext();
        //    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
        //    UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager) { AllowOnlyAlphanumericUserNames = false }; //se deshabilita la validacion del username para que este solo contenga letras y numeros

        //    user = UserManager.FindByEmail(eMail);
        //    if (user == null)
        //    {
        //        user = new ApplicationUser() { UserName = eMail, PasswordHash = newPwd, Email = eMail, EmailConfirmed = true, TwoFactorEnabled = false, LockoutEnabled = false, LockoutEndDateUtc = Convert.ToDateTime("1754-01-01 00:00:00.000"), AccessFailedCount = 0 };

        //        UserManager.Create(user, newPwd);

        //        if (TipoUsuario == 1)     //Cliente
        //            Rol = "Cliente";
        //        else if (TipoUsuario == 2)    //Proveedor
        //            Rol = "Proveedor";
        //        else if (TipoUsuario == 3)
        //            Rol = "Administrador";
        //        else                   //Capturista - Empleados
        //            Rol = "Capturista";

        //        UserManager.AddToRole(user.Id, Rol);
        //    }

        //    Notifications notification = new Notifications();
        //    if (notification.SendWelcome(user.UserName, eMail, newPwd, TipoVIp))
        //    {
        //        result = 2; //se envia correctamente la notificación en a los correos
        //        InsertFeNotificacion(IdPersona);
        //        MensageError = "SUCCESS";
        //    }
        //    else
        //    {
        //        result = 1; //Hubo algún problema al enviar la notificación a los correos
        //        MensageError = notification.Message;
        //    }

        //    InsertIinvitacion(IdPersona, TipoVIp, MensageError);
        //    Message = notification.Message;

        //    return result;
        //}
        public static void InsertFeNotificacion(int IdPersona)
        {
            DB dB = new DB();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdPersona", IdPersona);
            int ret = dB.ExecuteReturnValue("UpdateFeUltimaNotificacion", ref parameters);
        }
        public static void InsertIinvitacion(int IdPersona, bool Tipo, string Msg)
        {
            DB dB = new DB();
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@IdPersona", IdPersona);
            parameters[1] = new SqlParameter("@Tipo", Tipo);
            parameters[2] = new SqlParameter("@Msg", Msg);
            int ret = dB.ExecuteReturnValue("LogInvitacion", ref parameters);
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
        public static int AddSucursalUser(int idUsuario, int Sucursal)
        {
            int ret = 0;

            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@idSucursal", Sucursal);
            parameters[1] = new SqlParameter("@idPersona", idUsuario);

            DB dB = new DB();
            SqlDataReader dr = dB.ExecuteReader("SucursalAgregar", ref parameters);
            dr.Read();

            ret = (int)dr["Agregado"];

            return ret;
        }
        public static int ChangeVIP(int idUsuario, int vip)
        {
            int ret = 0;

            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@VIP", vip);
            parameters[1] = new SqlParameter("@idPersona", idUsuario);

            DB dB = new DB();
            SqlDataReader dr = dB.ExecuteReader("ChangeVIP", ref parameters);
            dr.Read();

            ret = (int)dr["Agregado"];

            return ret;
        }
    }
}
