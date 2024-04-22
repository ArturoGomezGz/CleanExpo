#define TESTMODE

using ExpoMR.Clases;
using ExpoMR.Datos;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Policy;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdministracionController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private static IConfiguration _configuration;

        //Variables para verificar La sesión Y el Usuario
        public AdministracionController(UserManager<IdentityUser> userManager, IEmailSender emailSender, IConfiguration configuration)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        private readonly string avisoConfidencialidad = "AVISO DE CONFIDENCIALIDAD: Este mensaje es confidencial y/o puede contener información privada.<br/>"
                                            + "Si usted no es el destinatario o no es alguna persona autorizada por éste para recibir el mensaje,<br/>"
                                            + "usted no deberá utilizar, copiar, revelar o tomar ninguna acción basada en este mensaje.<br/>"
                                            + "Si recibe este mensaje por error, por favor notifíquelo de inmediato al remitente y bórrelo de su computadora.<br/>";

    #region USUARIOS

        public IActionResult Usuarios()
        {
            var Administracion = new AdminstracionViewModel
            {
                IdTipoUsuario = 1, //Cliente
                Ruta = 0,  //General
                IdSucrsal = 1, //Gdl

                Rutas = GetRutas(),
                TipoUsuario = GetTipoUsuarios(),
                Sucursales = GetCmbSucursales()
            };

            return View(Administracion);
        }

        [HttpPost]
        public JsonResult GetUsuarios(int IdTipoUsuario, int IdRuta = 0)
        {
            List<UsuarioExpo> Usuarios = new();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@IdTipoPersona", IdTipoUsuario);
            parameters[1] = new SqlParameter("@IdRuta", IdRuta);

            SqlDataReader dr = dB.ExecuteReader("GetUsuarios", ref parameters);

            while (dr.Read())
            {
                UsuarioExpo Usuario = new UsuarioExpo();

                Usuario.IdEmail = (int)dr["IdEmail"];
                Usuario.Nombre = (string)dr["Nombre"];
                Usuario.Email = (string)dr["Email"];
                Usuario.IdTipoPersona = (int)dr["IdTipoPersona"];
                Usuario.TipoPersona = (string)dr["TipoPersona"];
                Usuario.Vip = (bool)dr["Vip"];
                Usuario.Usuario = (bool)dr["Usuario"];
                Usuario.Lock = (bool)dr["Lock"];
                Usuario.Sucursal = (string)dr["Sucursal"];
                Usuario.Ruta = (string)dr["Ruta"];
                Usuario.Invitacion = (string)dr["Invitacion"];

                Usuarios.Add(Usuario);
            }

            return Json(Usuarios);
        }

        public JsonResult CambiarSucursal(int IdEmail, int Sucursal)
        {
            var rm = new ResponseModel();

            try
            {
                WebSite.AddSucursalUser(IdEmail, Sucursal);

                rm.Response = true;
                rm.Message = "se cambio la sucursal";

            }
            catch (Exception ex)
            {
                rm.Response = false;
                rm.Message = ex.Message;
            }

            return Json(rm);
        }

        public async Task<IActionResult> EditarUsuario(int IdEmail)
        {
            EditarUsuarioViewModel Usuario = new EditarUsuarioViewModel();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdEmail", IdEmail);

            SqlDataReader dr = dB.ExecuteReader("GetUsuario", ref parameters);

            while (dr.Read())
            {
                Usuario.IdPersona = (int)dr["IdPersona"];
                Usuario.Nombre = (string)dr["Nombre"];
                Usuario.IdEmail = (int)dr["IdEmail"];
                Usuario.Email = (string)dr["Email"];
                Usuario.IdTipoUsuario = (int)dr["IdTipoUsuario"];
                Usuario.TipoUsuario = (string)dr["TipoUsuario"];
                Usuario.VIP = (bool)dr["VIP"];
                Usuario.Usuario = (bool)dr["Usuario"];
                Usuario.Lock = (bool)dr["Lock"];
                Usuario.Sucursal = (string)dr["Sucursal"];
                Usuario.IdSucursal = (byte)dr["IdSucursal"];
                Usuario.Invitacion = (dr["Invitacion"] != DBNull.Value ? (string)dr["Invitacion"] : "");
            }

            var user = await _userManager.FindByNameAsync(Usuario.Email);
            Usuario.Sucursales = GetCmbSucursales();

            if (user != null)
            {
                Usuario.Usuario = true;
                var isBlocked = await _userManager.IsLockedOutAsync(user);
                Usuario.isLock = isBlocked ? "fa-lock" : "fa-unlock-alt";
            }
            else
            {
                Usuario.Usuario = false;
                Usuario.isLock = "fa-unlock-alt";
            }

            return View(Usuario);
        }

        [HttpPost]
        public async Task<ActionResult> ChangeVip(int IdEmail, bool Vip)
        {
            WebSite.ChangeVIP(IdEmail, Vip);
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> EliminarUsuario(int IdEmail, string UserName)
        {
            var Result = WebSite.UsuarioEliminar(IdEmail);
            var rm = new ResponseModel();

            if (Result == 1)
            {
                try
                {
                    var user = await _userManager.FindByEmailAsync(UserName);

                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);

                        foreach (var role in roles)
                        {
                            // Elimina al usuario de cada rol al que pertenece
                            await _userManager.RemoveFromRoleAsync(user, role);
                        }

                        // Elimina al usuario
                        await _userManager.DeleteAsync(user);
                    }
                }
                catch (Exception ex)
                {
                    rm.Response = false;
                    rm.Message = ex.Message;
                }
            }

            return Json(rm);
        }

        [HttpPost]
        public async Task<JsonResult> AddUserAsync(int idEmail, string UserName, int TipoCliente, bool Vip, int Sucursal)
        {
            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[4];
            parameters[0] = new SqlParameter("@idemail", idEmail);
            parameters[1] = new SqlParameter("@tipousuario", TipoCliente);
            parameters[2] = new SqlParameter("@sucursal", Sucursal);
            parameters[3] = new SqlParameter("@VIP", Vip);

            SqlDataReader dr = dB.ExecuteReader("UsuariosAgregar", ref parameters);

            ResponseModel rm = new();

            try
            {
                var usuario = new AppUsuario
                {
                    UserName = UserName,
                    Email = UserName
                };

                var user = await _userManager.FindByNameAsync(usuario.Email);

                if (user == null)
                {
                    //Se crea el usuario en la tabla AspNetUsers
                    var resultado = await _userManager.CreateAsync(usuario, TipoCliente == 4 ? "ExpoEmp2024" : "Expo2024"); // TipoCliente 4 = Empleado

                    if (resultado.Succeeded)
                    {
                        string Rol = "";

                        if (TipoCliente == 1) //Cliente
                        {
                            Rol = Vip ? "ClienteVIP" : "Cliente";

                        }
                        else if (TipoCliente == 2) //Proveedor
                        {
                            Rol = "Proveedor";
                        }
                        else //Empleado - Capturista
                        {
                            Rol = "Capturista";
                        }

                        //Se asigna el rol del usuario en la tabla AspNetUserRoles
                        await _userManager.AddToRoleAsync(usuario, Rol);

                        string linkExpo = Vip ? "https://expomr.online" : "https://expomr.online";

                        string body = "<br/>Hola " + UserName + "<br/><br/>"
                                + "Medicinas Rosario te da la bienvenida a su Expo Virtual 2022<br/><br/>"
                                + "Ingresa en <a href='" + linkExpo + "'>" + linkExpo + "</a><br/><br/>"
                                + "Esta es la información de tu cuenta y solo podrás utilizarla los días que dure el evento:<br/><br/>"
                                + "Usuario: " + UserName + "<br/>"
                                + "Contraseña: " + (TipoCliente == 4 ? "ExpoEmp2024" : "Expo2024") + "<br/>"
                                + "(Esta es una contraseña generada automáticamente, te sugerimos cambiarla la primera vez que ingreses)<br/><br/><br/><br/>"
                                + "Si tienes alguna duda o comentario, te solicitamos nos envíes un correo electrónico a la siguiente dirección "
                                + "<a href='mailto:'>Soporte ExpoMR</a><br/><br/><br/>"
                                + avisoConfidencialidad;

                        try
                        {
                            #if (TESTMODE)
                                await _emailSender.SendEmailAsync("carlos.rivera@medicinasrosario.com.mx", "Bienvenido a Expo Virtual Medicinas Rosario", body);
                            #else
                                await _emailSender.SendEmailAsync(UserName, "Confirmar su cuenta - ExpoMR",
                                    "Por favor confirme su cuenta dando click aquí: <a href=\"" + urlRetorno + "\"> enlace </a>");
                                await _signInManager.SignInAsync(usuario, isPersistent: false);
                            #endif

                            WebSite.InsertFeNotificacion(idEmail);
                            WebSite.InsertIinvitacion(idEmail, Vip, "SUCCESS");
                        }
                        catch (Exception e)
                        {
                            WebSite.InsertIinvitacion(idEmail, Vip, e.Message);
                        }

                        rm.Message = "El usuario de: " + UserName + " se creo correctamente";
					}
                    else
                    {
                        rm.Message = resultado.Errors.ToString();
                    }

                    rm.Response = resultado.Succeeded;
                }
                else
                {
                    rm.Response = true;
                    rm.Message = "El usuario de: " + UserName + " se creo correctamente";
                }
            }
            catch (Exception e)
            {
                rm.Response = false;
                rm.Message = e.Message;
            }

            return Json(rm);
        }

        public async Task<JsonResult> Bloquear(string Email)
        {
            var rm = new ResponseModel();
            var user = await _userManager.FindByNameAsync(Email);

            if (user != null)
            {
                rm.Response = true;
                var isBlocked = await _userManager.IsLockedOutAsync(user);

                if (isBlocked) // Usuario bloqueado
                {
                    rm.Result = true;
                    await _userManager.SetLockoutEndDateAsync(user, null);
                    rm.Message = "El usuario ha sido desbloqueado.";
                }
                else // Usuario no bloqueado
                {
                    rm.Result = false;
                    await _userManager.SetLockoutEndDateAsync(user, DateTime.Parse(WebSite.GetLockEndUsers()));
                    rm.Message = "El usuario ha sido bloqueado.";
                }
            }
            else
            {
                rm.Response = false;
                rm.Message = "No se encontro el usuario en el sistema";
            }

            return Json(rm);
        }

        [HttpPost]
        public async Task<JsonResult> ReenviarInvitacion(int IdEmail, string UserName, bool Vip)
        {
            var rm = new ResponseModel();

            try
            {
                var usuario = await _userManager.FindByEmailAsync(UserName);

                if (usuario != null)
                {

                    string token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                    var resultado = await _userManager.ResetPasswordAsync(usuario, token, "Expo2024");

                    if (resultado.Succeeded)
                    {
                        string linkExpo = Vip ? "https://expomr.online" : "https://expomr.online";

                        string body = "<br/>Hola " + UserName + "<br/><br/>"
                                + "Medicinas Rosario te da la bienvenida a su Expo Virtual 2022<br/><br/>"
                                + "Ingresa en <a href='" + linkExpo + "'>" + linkExpo + "</a><br/><br/>"
                                + "Esta es la información de tu cuenta y solo podrás utilizarla los días que dure el evento:<br/><br/>"
                                + "Usuario: " + UserName + "<br/>"
                                + "Contraseña:  Expo2024 <br/>"
                                + "(Esta es una contraseña generada automáticamente, te sugerimos cambiarla la primera vez que ingreses)<br/><br/><br/><br/>"
                                + "Si tienes alguna duda o comentario, te solicitamos nos envíes un correo electrónico a la siguiente dirección "
                                + "<a href='mailto:'>Soporte ExpoMR</a><br/><br/><br/>"
                            + avisoConfidencialidad;
                        
                        #if (TESTMODE)
                            await _emailSender.SendEmailAsync("carlos.rivera@medicinasrosario.com.mx", "Bienvenido a Expo Virtual Medicinas Rosario", body);
                        #else

                            await _emailSender.SendEmailAsync(UserName, "Confirmar su cuenta - ExpoMR",
                                "Por favor confirme su cuenta dando click aquí: <a href=\"" + urlRetorno + "\"> enlace </a>");
                            await _signInManager.SignInAsync(usuario, isPersistent: false);
                        #endif
                        
                        WebSite.InsertFeNotificacion(IdEmail);
                        WebSite.InsertIinvitacion(IdEmail, Vip, "SUCCESS");

                        rm.Response = true;
                        rm.Message = "Se envio la invitacion correctamente";
                    }
                    else
                    {
                        rm.Response = false;
                        rm.Message = "Hubo un error al cambiar la contraseña y al enviar la invitación";
                    }
				}
			}
            catch (Exception e)
            {
                WebSite.InsertIinvitacion(IdEmail, Vip, e.Message);
                rm.Response = false;
                rm.Message = e.Message;
            }

            return Json(rm);
        }

        [HttpPost]
        public async Task<JsonResult> InvitacionMasivaClientes(int IdEmail, string UserName, bool Vip)
        {
            var rm = new ResponseModel();

            try
            {
				var usuario = new AppUsuario
				{
					UserName = UserName,
					Email = UserName
				};

			    var user = await _userManager.FindByEmailAsync(UserName);

                if (user == null)
                {
					//Se crea el usuario en la tabla AspNetUsers
					var resultado = await _userManager.CreateAsync(usuario, "Expo2024");

                    if (resultado.Succeeded)
                    {
						string Rol = "";
                        Rol = Vip ? "ClienteVIP" : "Cliente";

						//Se asigna el rol del usuario en la tabla AspNetUserRoles
						await _userManager.AddToRoleAsync(usuario, Rol);
					}
					else
					{
						rm.Message = resultado.Errors.ToString();
					}
				}

				string linkExpo = Vip ? "https://expomr.online" : "https://expomr.online";

				string body = "<br/>Hola " + UserName + "<br/><br/>"
						+ "Medicinas Rosario te da la bienvenida a su Expo Virtual 2022<br/><br/>"
						+ "Ingresa en <a href='" + linkExpo + "'>" + linkExpo + "</a><br/><br/>"
						+ "Esta es la información de tu cuenta y solo podrás utilizarla los días que dure el evento:<br/><br/>"
						+ "Usuario: " + UserName + "<br/>"
						+ "Contraseña: Expo2024" + "<br/>"
						+ "(Esta es una contraseña generada automáticamente, te sugerimos cambiarla la primera vez que ingreses)<br/><br/><br/><br/>"
						+ "Si tienes alguna duda o comentario, te solicitamos nos envíes un correo electrónico a la siguiente dirección "
						+ "<a href='mailto:'>Soporte ExpoMR</a><br/><br/><br/>"
						+ avisoConfidencialidad;

				try
				{
                    #if (TESTMODE)
					    await _emailSender.SendEmailAsync("carlos.rivera@medicinasrosario.com.mx", "Bienvenido a Expo Virtual Medicinas Rosario", body);
                    #else

                         await _emailSender.SendEmailAsync(UserName, "Confirmar su cuenta - ExpoMR",
                            "Por favor confirme su cuenta dando click aquí: <a href=\"" + urlRetorno + "\"> enlace </a>");
                         await _signInManager.SignInAsync(usuario, isPersistent: false);
                    #endif

					WebSite.InsertFeNotificacion(IdEmail);
					WebSite.InsertIinvitacion(IdEmail, Vip, "SUCCESS");
                    
				}
				catch (Exception e)
				{
					WebSite.InsertIinvitacion(IdEmail, Vip, e.Message);
				}

				rm.Response = true;
                rm.Message = "Se envio la invitacion correctamente al cliente: " + UserName;
			}
			catch (Exception e)
			{
				rm.Response = false;
				rm.Message = e.Message;
			}

			return Json(rm);
		}
        
        [HttpPost]
        public async Task<JsonResult> CambiarPassword(CambiarPasswordViewModel model, string email)
        {
            var rm = new ResponseModel();

            if (ModelState.IsValid)
            {
                var usuario = await _userManager.FindByEmailAsync(email);

                if (usuario == null)
                {
                    rm.Response = false;
                    rm.Message = "No se encontro el usuario en el sistema";
                }
                else
                {
					string token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
					var resultado = await _userManager.ResetPasswordAsync(usuario, token, model.Password);

					if (resultado.Succeeded)
					{
						rm.Response = true;
						rm.Message = "la contraseña se cambio correctamente";
					}
					else
					{
						rm.Response = false;
						rm.Message = "No se pudo cambiar la contraseña";
					}
				}
            }

            return Json(rm);
        }

        public List<mTipoUsuario> GetTipoUsuarios()
        {
            List<mTipoUsuario> TiposUsuarios = new();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[0];

            SqlDataReader dr = dB.ExecuteReader("GetTiposUsuario", ref parameters);

            while (dr.Read())
            {
                mTipoUsuario TipoUsuario = new mTipoUsuario();

                TipoUsuario.IdTipoUsuario = (byte)dr["IdTipoUsuario"];
                TipoUsuario.TipoUsuario = (string)dr["TipoUsuario"];

                TiposUsuarios.Add(TipoUsuario);
            }

            return TiposUsuarios;
        }

        public List<RutasList> GetRutas()
        {
            List<RutasList> RutasVenta = new();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[0];

            SqlDataReader dr = dB.ExecuteReader("GetRutasVenta", ref parameters);

            while (dr.Read())
            {
                RutasList RutaVenta = new RutasList();

                RutaVenta.IdRuta = (Int32)dr["IdRuta"];
                RutaVenta.Descripcion = (string)dr["Descripcion"];

                RutasVenta.Add(RutaVenta);
            }

            return RutasVenta;
        }

        public List<SucursalUsuario> GetCmbSucursales()
        {
            List<SucursalUsuario> vmSucursal = new()
            {
                new SucursalUsuario { IdSucursal = 0, Sucursal = ". . ." },
                new SucursalUsuario { IdSucursal = 1, Sucursal = "Guadalajara" },
                new SucursalUsuario { IdSucursal = 2, Sucursal = "Querétaro" },
                new SucursalUsuario { IdSucursal = 3, Sucursal = "México" }
            };

            return vmSucursal;
        }

    #endregion

    #region PROVEEDORES

        public IActionResult Proveedor()
        {
            return View();
        }

		public IActionResult EditProveedor(int IdProveedor = 0)
		{
			using var dbExpo = new ApplicationDbContextExpo();

			EditarProveedorViewModel proveedor = dbExpo.Set<EditarProveedorViewModel>()
				.FromSqlRaw("SELECT mProveedores.IdPersona, mProveedores.Nombre, mProveedores.Descripcion, ISNULL(mMonedero.Saldo,0) AS Saldo, COUNT(mLaboratorios.IdLaboratorio) AS NumLabAsignados"
                          + " FROM mProveedores LEFT OUTER JOIN"
						  + " mMonedero ON mProveedores.IdPersona = mMonedero.IdPersona LEFT OUTER JOIN"
                          + " mLaboratorios ON mProveedores.IdPersona = mLaboratorios.Proveedor"
                          + " WHERE mProveedores.IdPersona = " + IdProveedor
                          + " GROUP BY mProveedores.IdPersona, mProveedores.Nombre, mProveedores.Descripcion, mMonedero.Saldo")
				.FirstOrDefault();

			if (proveedor == null)
			{
				proveedor = new EditarProveedorViewModel();
				proveedor.IdPersona = IdProveedor;
				proveedor.Nombre = "";
				proveedor.Descripcion = "";
				proveedor.Saldo = 0;
			}

			return View(proveedor);
		}

		[HttpPost]
        public JsonResult GetProveedores()
        {
            using (var dbExpo = new ApplicationDbContextExpo())
            {
                var parameters = new object[]
                {
                    1
                };

                List<mProveedores> proveedor = dbExpo.Set<mProveedores>()
                    .FromSqlRaw("EXEC GetProveedores @IdEmpresa = {0}", parameters)
                    .ToList();

                return Json(proveedor);
            }
        }

        [HttpPost]
        public JsonResult GetLaboratorios(int IdPersona)
        {
            using var dbExpo = new ApplicationDbContextExpo();
            return Json(dbExpo.Set<mLaboratorio>()
                .FromSqlRaw("SELECT mLaboratorios.IdLaboratorio, mLaboratorios.Laboratorio"
                          + " FROM mLaboratorios"
                          + " WHERE mLaboratorios.Proveedor =" + IdPersona).ToList());
        }

        [HttpPost]
        public JsonResult GetAddLabs()
        {
            using var dbExpo = new ApplicationDbContextExpo();
            return Json(dbExpo.Set<mLaboratorioI>()
            .FromSqlRaw("SELECT mLaboratorios.IdLaboratorio, mLaboratorios.Laboratorio"
                      + "  FROM Indigo.dbo.mLaboratorios"
                      + "  WHERE mLaboratorios.IdLaboratorio NOT IN (SELECT mLaboratorios.IdLaboratorio FROM mLaboratorios)"
                      + "  ORDER BY mLaboratorios.Laboratorio").ToList());
        }

        [HttpPost]
        public JsonResult SaveProvedor(string nombre, string descripcion, int IdPersona)
        {
            using var dbExpo = new ApplicationDbContextExpo();
            var rm = new ResponseModel();

            try
            {
                dbExpo.Database.ExecuteSqlRaw("UPDATE mProveedores " +
                                              "SET Nombre = CASE WHEN '" + nombre + "' = '' THEN " +
                                              "mProveedores.Nombre ELSE UPPER('" + nombre + "') END, " +
                                              "Descripcion = CASE WHEN '" + descripcion + "' = '' THEN NULL ELSE UPPER('" + descripcion + "') END " +
                                              "WHERE mProveedores.IdPersona =" + IdPersona);

                rm.Response = true;
                rm.Message = "Sus datos se guardaron correctamente";
            }
            catch (Exception e)
            {
                rm.Response = false;
                rm.Message = e.Message;
            }

            return Json(rm);
        }

        [HttpPost]
        public JsonResult SaveMonedas(int IdPersona, int monedas)
        {
            var rm = new ResponseModel();
            var dbExpo = new ApplicationDbContextExpo();
            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            int IdUsuario = GeneralesController.GetIdPersonas(username);

            List<SqlParameter> parms = new List<SqlParameter>
            { 
                new SqlParameter { ParameterName = "@origen", Value = 0 }, //0 = MR
                new SqlParameter { ParameterName = "@destino", Value = IdPersona },
                new SqlParameter { ParameterName = "@idmotivo", Value = 11 }, //11 = Compra Monedas
                new SqlParameter { ParameterName = "@monedas", Value = monedas },
                new SqlParameter { ParameterName = "@usuario", Value = IdUsuario }
            };
            var resultadoParameter = new SqlParameter("@success", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            try
            {
                dbExpo.Database.ExecuteSqlRaw($"EXEC @success = MonedasAsignar @origen={parms[0].Value}, @destino={parms[1].Value}, @idmotivo={parms[2].Value}, @monedas={parms[3].Value}, @usuario={parms[4].Value}", resultadoParameter);

                bool success = Convert.ToBoolean(resultadoParameter.Value);

                if (success)
                {
                    Saldos saldo = dbExpo.Set<Saldos>()
                        .FromSqlRaw("SELECT ISNULL(mMonedero.Saldo,0) AS Saldo"
                       + " FROM mProveedores LEFT OUTER JOIN"
                       + " mMonedero ON mProveedores.IdPersona = mMonedero.IdPersona"
                       + "  WHERE mProveedores.IdPersona = " + IdPersona).FirstOrDefault();

                    rm.Result = saldo;
                    rm.Response = true;
                    rm.Message = "Sus datos se guardaron correctamente";
                }
                else
                {
                    rm.Response = false;
                    rm.Message = "Ocurrió un error al ejecutar el procedimiento almacenado.";
                }

            }
            catch (Exception e)
            {
                rm.Response = false;
                rm.Message = e.Message;
            }

            return Json(rm);
        }

        [HttpPost]
        public JsonResult AddLab(int IdPersona, int idLaboratorio)
        {
            var rm = new ResponseModel();
            var dbExpo = new ApplicationDbContextExpo();
           
            try
            {
                dbExpo.Database.ExecuteSqlRaw("EXEC InicializaLaboratorio " + IdPersona + ", " + idLaboratorio);
                rm.Response = true;
                rm.Message = "Sus datos se guardaron correctamente";
            }
            catch (Exception e)
            {
                rm.Response = false;
                rm.Message = e.Message;
            }

            return Json(rm);
        }

        [HttpPost]
        public JsonResult DeleteLab(int idLaboratorio)
        {
            var rm = new ResponseModel();
            var dbExpo = new ApplicationDbContextExpo();
            var resultadoParameter = new SqlParameter("@result", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            try
            {
                dbExpo.Database.ExecuteSqlRaw("EXEC DeleteLabProv " + idLaboratorio + ", @result OUTPUT", resultadoParameter);             

                if (Convert.ToBoolean(resultadoParameter.Value))
                {
                    rm.Response = true;
                    rm.Message = "El laboratorio fue eliminado correctamente";
                }
                else
                {
                    rm.Response = false;
                    rm.Message = "El Laboratorio no fue eliminado, debido a que tiene producto(s) usados en otros procesos";
                }
            }
            catch (Exception e)
            {
                rm.Response = false;
                rm.Message = e.Message;
            }

            return Json(rm);
        }

        [HttpPost]
        public JsonResult AddProveedor(int IdPersona)
        {
            var dbExpo = new ApplicationDbContextExpo();
            var rm = new ResponseModel();
            var parameters = new object[]
                {
                    IdPersona
                };

            try
            {
                dbExpo.Database.ExecuteSqlRaw("EXEC AddProv @IdPersona = {0}", parameters);

                rm.Response = true;
                rm.Message = "El Proveedor se dio de alta correctamente";
            }
            catch (Exception e)
            {
                rm.Response = false;
                rm.Message = e.Message;
            }

            return Json(rm);
        }

        [HttpPost]
        public JsonResult DeleteProveedor(int IdPersona)
        {
            var dbExpo = new ApplicationDbContextExpo();
            var rm = new ResponseModel();
            var resultadoParameter = new SqlParameter("@result", SqlDbType.TinyInt)
            {
                Direction = ParameterDirection.Output
            };

            try
            {
                dbExpo.Database.ExecuteSqlRaw("EXEC DeleteProv " + IdPersona + " , @result OUTPUT", resultadoParameter);              

                if (Convert.ToBoolean(resultadoParameter.Value))
                {
                    rm.Response = true;
                    rm.Message = "BAJA CORRECTA";
                }
                else
                {
                    rm.Response = false;
                    rm.Message = "EL PROVEEDOR TIENE USUARIO PARA INGREAR AL SISTEMA O TIENE LABORATORIOS ASIGNADOS";
                }
            }
            catch (Exception e)
            {
                rm.Response = false;
                rm.Message = e.Message;
            }

            return Json(rm);
        }

    #endregion

    #region EMPLEADOS

        public IActionResult Empleados()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetEmpleados()
        {
            using (var dbExpo = new ApplicationDbContextExpo())
            {
                List<Empleados> empleados = dbExpo.Set<Empleados>()
                    .FromSqlRaw("EXEC GetEmpleados")
                    .ToList();

                return Json(empleados);
            }
        }

		public List<Roles> Roles()
		{
			var dbExpo = new ApplicationDbContextExpo();         

            List<Roles> roles = dbExpo.Set<Roles>()
				.FromSqlRaw("SELECT '' AS IdRol, '' AS Role "
                + "UNION "
                + "SELECT AspNetRoles.Id AS IdRol, AspNetRoles.Name AS Role "
                + "FROM AspNetRoles "
                + "WHERE AspNetRoles.Name IN ('Administrador','Capturista','Vendedor') " 
                + "ORDER BY Role ").ToList();

            return roles;
		}

		public async Task<IActionResult> EditarEmpleado(int IdPersona = 0)
        {
            var dbExpo = new ApplicationDbContextExpo();

            Empleados empleado = dbExpo.Set<Empleados>()
                .FromSqlRaw("SELECT vPersonas.IdPersona, vPersonas.Nombre, mEmails.Email, AspNetRoles.Id AS IdRol, AspNetRoles.Name AS Rol "
                              + "FROM mUsuarios INNER JOIN "
                              + "Indigo.dbo.mEmails ON mUsuarios.IdEmail = mEmails.IdEmail INNER JOIN "
                              + "Indigo.dbo.vPersonas ON mUsuarios.IdPersona = vPersonas.IdPersona LEFT OUTER JOIN "
                              + "AspNetUsers ON mEmails.Email = AspNetUsers.Email LEFT OUTER JOIN "
                              + "AspNetUserRoles ON AspNetUsers.Id = AspNetUserRoles.UserId LEFT OUTER JOIN "
                              + "AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id "
                              + "WHERE mUsuarios.IdTipoUsuario IN (3,4) "
                              + "AND mUsuarios.IdPersona = " + IdPersona)
                .FirstOrDefault();

            empleado.Roles = Roles();  //Se obtienen los roles para el usuario         
           
            return View(empleado);
        }

        public async Task<JsonResult> CambiarRolEmpleado(string UserName, string Rol)
        {
            var rm = new ResponseModel();
            rm.Response = false;

            var user = await _userManager.FindByNameAsync(UserName);

            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Count()  != 0)
                    await _userManager.RemoveFromRolesAsync(user, roles);

                if(Rol != null)
                    await _userManager.AddToRoleAsync(user, Rol);

                rm.Response = true;
                rm.Message = "El ROL se Cambio Correctamente";

                RedirectToAction("EditarEmpleado");
            }
            else
            {
                rm.Message = "No hay un Usuario registrado con ese Correo";
            }

            return Json(rm);
        }

    #endregion

    #region NOTIFICACIONES

        public IActionResult Notificaciones()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetUsusariosNotificacion()
        {
            var dbExpo = new ApplicationDbContextExpo();
            List<UsuariosNotificacion> usuariosNotificacion = dbExpo.Set<UsuariosNotificacion>().FromSqlRaw("SELECT mClientes.IdPersona, mClientes.Nombre, "
                                + "ISNULL(lNotificaciones.IdTipoNotificacion, 0) AS Notificado, mUsuarios.UserName AS Email "
                                + "FROM mClientes INNER JOIN "
                                + "mUsuarios ON mClientes.IdPersona = mUsuarios.IdPersona INNER JOIN "
                                + "dPromoCliente ON mClientes.IdPersona = dPromoCliente.Cliente LEFT OUTER JOIN "
                                + "lNotificaciones ON mClientes.IdPersona = lNotificaciones.IdPersona AND IdTipoNotificacion = 1 "
                                + "GROUP BY mClientes.IdPersona, mClientes.Nombre, mUsuarios.UserName, Lnotificaciones.IdTipoNotificacion "
                                + "ORDER BY Lnotificaciones.IdTipoNotificacion ASC, mClientes.Nombre").ToList();

            return Json(usuariosNotificacion);
        }

    #endregion
    }
}