using ExpoMR.Clases;
using ExpoMR.Datos;
using ExpoMR.Models;
using MailKit;
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
        private readonly RoleManager<IdentityRole> _rolManager;
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
                    Sucursales = GetSucursales()
                };

                return View(Administracion);
            }

            public async Task<IActionResult> EditarUsuario(int IdEmail)
            {
                EditarUsuarioViewModel Usuario = new EditarUsuarioViewModel();

                string query = "SELECT vPersonas.IdPersona, vPersonas.Nombre, vPersonas.IdEmail, vPersonas.Email, vPersonas.IdTipoUsuario, " +
                                "(SELECT mTipoUsuario.TipoUsuario FROM mTipoUsuario WHERE mTipoUsuario.IdTipoUsuario = vPersonas.IdTipoUsuario) AS TipoUsuario, " +
                                "CAST(vPersonas.Usuario AS BIT) AS Usuario, CAST(vPersonas.Lock AS BIT) AS Lock, " +
                                "CASE WHEN vPersonas.Sucursal = 0 THEN '' " +
                                "	 ELSE CASE WHEN vPersonas.Sucursal = 1 THEN 'GUADALAJARA' " +
                                "			   ELSE CASE WHEN vPersonas.Sucursal = 2  THEN 'QUERETARO' " +
                                "						 ELSE 'MEXICO' " +
                                "					END " +
                                "		  END " +
                                "END AS Sucursal, " +
                                "vPersonas.Sucursal AS IdSucursal, " +
                                "CAST(mUsuarios.FeUltimaNotificacion AS NVARCHAR) AS Invitacion " +
                                "FROM vPersonas LEFT OUTER JOIN " +
                                "mUsuarios ON vPersonas.IdEmail = mUsuarios.IdEmail " +
                                "WHERE vPersonas.IdEmail = @IdEmail";

                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@IdEmail", IdEmail);

                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

                while (dr.Read())
                {
                    Usuario.IdPersona = (int)dr["IdPersona"];
                    Usuario.Nombre = (string)dr["Nombre"];
                    Usuario.IdEmail = (int)dr["IdEmail"];
                    Usuario.Email = (string)dr["Email"];
                    Usuario.IdTipoUsuario = (int)dr["IdTipoUsuario"];
                    Usuario.TipoUsuario = (string)dr["TipoUsuario"];
                    Usuario.Usuario = (bool)dr["Usuario"];
                    Usuario.Lock = (bool)dr["Lock"];
                    Usuario.Sucursal = (string)dr["Sucursal"];
                    Usuario.IdSucursal = (byte)dr["IdSucursal"];
                    Usuario.Invitacion = (dr["Invitacion"] != DBNull.Value ? (string)dr["Invitacion"] : "");
                }

			    dB.Close();

			    var user = await _userManager.FindByNameAsync(Usuario.Email);
                Usuario.Sucursales = GetSucursales();

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

            public List<RutasList> GetRutas()
            {
                List<RutasList> RutasVenta = new();

                string query = "SELECT Rutas.IdRuta, Rutas.Descripcion " +
                                "FROM ( " +
								"		SELECT mRutas.IdRuta, UPPER(mRutas.Descripcion) AS Descripcion " +
								"       FROM Indigo.dbo.mRutas" +
                                "		UNION " +
								"		SELECT 0 AS IdRuta, 'GENERAL' AS Descripcion " +
                                "	) AS Rutas " +
								"ORDER BY CASE WHEN Rutas.IdRuta = 0 THEN 1 ELSE 0 END, Rutas.Descripcion";

                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[0];

                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

                while (dr.Read())
                {
                    RutasList RutaVenta = new RutasList();

                    RutaVenta.IdRuta = (Int32)dr["IdRuta"];
                    RutaVenta.Descripcion = (string)dr["Descripcion"];

                    RutasVenta.Add(RutaVenta);
                }

			    dB.Close();

			    return RutasVenta;
            }

            public List<SucursalUsuario> GetSucursales()
            {
                List<SucursalUsuario> Sucursales = new();

                string query = "SELECT 0 AS IdSucursal, '...' AS Sucursal " +
                                "UNION " +
                                "SELECT 1 AS IdSucursal, 'Guadalajara' AS Sucursal " +
                                "UNION " +
                                "SELECT 2 AS IdSucursal, 'Querétaro' AS Sucursal " +
                                "UNION " +
                                "SELECT 3 AS IdSucursal, 'México' AS Sucursal";

                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[0];

                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

                while (dr.Read())
                {
                    SucursalUsuario Sucursal = new SucursalUsuario();

                    Sucursal.IdSucursal = (int)dr["IdSucursal"];
                    Sucursal.Sucursal = (string)dr["Sucursal"];

                    Sucursales.Add(Sucursal);
                }

			    dB.Close();

			    return Sucursales;
            }

            public List<mTipoUsuario> GetTipoUsuarios()
            {
                List<mTipoUsuario> TiposUsuarios = new();

                string query = "SELECT mTipoUsuario.IdTipoUsuario, mTipoUsuario.TipoUsuario " +
                                "FROM mTipoUsuario " +
                                "WHERE mTipoUsuario.IdTipoUsuario <> 3 " +
                                "ORDER BY mTipoUsuario.TipoUsuario";

                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[0];

                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

                while (dr.Read())
                {
                    mTipoUsuario TipoUsuario = new mTipoUsuario();

                    TipoUsuario.IdTipoUsuario = (byte)dr["IdTipoUsuario"];
                    TipoUsuario.TipoUsuario = (string)dr["TipoUsuario"];

                    TiposUsuarios.Add(TipoUsuario);
                }

			    dB.Close();

			    return TiposUsuarios;
            }

            [HttpPost]
            public async Task<JsonResult> AddUserAsync(int idEmail, string UserName, int TipoCliente, int Sucursal)
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[3];
                parameters[0] = new SqlParameter("@idemail", idEmail);
                parameters[1] = new SqlParameter("@tipousuario", TipoCliente);
                parameters[2] = new SqlParameter("@sucursal", Sucursal);

                SqlDataReader dr = dB.ExecuteReader("UsuariosAgregar", ref parameters);

			    ResponseModel rm = new();

			    dB.Close();

			    try
                    {
                    var usuario = new AppUsuario
                    {
                        UserName = UserName,
                        Email = UserName,
                        EmailConfirmed = false
                    };

                    var user = await _userManager.FindByNameAsync(usuario.Email);

                    if (user == null)
                    {
                        //Se crea el usuario en la tabla AspNetUsers
                        var resultado = await _userManager.CreateAsync(usuario, TipoCliente == 4 ? "ExpoEmp2025" : "Expo2025"); // TipoCliente 4 = Empleado

                        if (resultado.Succeeded)
                        {
                            string Rol = "";

                            if (TipoCliente == 1) //Cliente
                            {
                                Rol = "Cliente";

                            }
                            else if (TipoCliente == 2) //Proveedor
                            {
                                Rol = "Proveedor";
                            }
                            else //Empleado - Capturista
                            {
                                Rol = "Capturista";
							    usuario.EmailConfirmed = true;
							}

                            //Se asigna el rol del usuario en la tabla AspNetUserRoles
                            await _userManager.AddToRoleAsync(usuario, Rol);

                            string linkExpo = "https://expomr.online";

                            string body = "<br/>Hola " + UserName + "<br/><br/>"
                                    + "Bienvenido a Bordo expo Medicinas Rosario 2025<br/><br/>"
                                    + "Ingresa en <a href='" + linkExpo + "'>" + linkExpo + "</a><br/><br/>"
                                    + "Esta es la información de tu cuenta y solo podrás utilizarla los días que dure el evento:<br/><br/>"
                                    + "Usuario: " + UserName + "<br/>"
                                    + "Contraseña: " + (TipoCliente == 4 ? "ExpoEmp2025" : "Expo2025") + "<br/>"
                                    + "(Esta es una contraseña generada automáticamente, te sugerimos cambiarla la primera vez que ingreses)<br/><br/><br/><br/>"
                                    + "Si tienes alguna duda o comentario, te solicitamos nos envíes un correo electrónico a la siguiente dirección "
                                    + "<a href='mailto:'>Soporte ExpoMR</a><br/><br/><br/>"
                                    + avisoConfidencialidad;

                            try
                            {
                                await _emailSender.SendEmailAsync(UserName, "Bienvenido a Expo Virtual Medicinas Rosario", body);
                            //await _emailSender.SendEmailAsync(UserName, "Confirmar su cuenta - ExpoMR",
                            //    "Por favor confirme su cuenta dando click aquí: <a href=\"" + urlRetorno + "\"> enlace </a>");
                            //await _signInManager.SignInAsync(usuario, isPersistent: false);

                                WebSite.InsertFeNotificacion(idEmail);
                                WebSite.InsertIinvitacion(idEmail, "SUCCESS");
                            }
                            catch (Exception e)
                            {
                                WebSite.InsertIinvitacion(idEmail, e.Message);
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
                        WebSite.InsertFeNotificacion(idEmail);
                        WebSite.InsertIinvitacion(idEmail, "SUCCESS");
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
                rm.Response = true;

                var user = await _userManager.FindByNameAsync(Email);

                if (user != null)
                {
                    if (await _userManager.IsLockedOutAsync(user)) //Usuario bloqueado
                    {
                        await _userManager.SetLockoutEndDateAsync(user, null);
                        rm.Message = "El usuario ha sido desbloqueado.";
                    }
                    else
                    {
                        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddDays(1)); //Se bloquea al usuario por 1 dia
                        rm.Message = "El usuario ha sido bloqueado.";
                        rm.Result = false;
                    }
                }
                else
                {
                    rm.Message = "No se encontro el usuario en el sistema";
                    rm.Response = false;
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
					    usuario.EmailConfirmed = true;

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
            public async Task<JsonResult> InvitacionMasivaClientes(int IdEmail, string UserName, int idTipoUsuario)
            {
                var rm = new ResponseModel();

                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[3];
                parameters[0] = new SqlParameter("@idemail", IdEmail);
                parameters[1] = new SqlParameter("@tipousuario", idTipoUsuario);
                parameters[2] = new SqlParameter("@sucursal", 1);

                SqlDataReader dr = dB.ExecuteReader("UsuariosAgregar", ref parameters);

                dB.Close();

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
                        var resultado = await _userManager.CreateAsync(usuario, "Expo2025");
                        
                        //Se asigna el rol dependiendo del tipo del usuario
                        string Rol = (idTipoUsuario == 1 ? "Cliente" : "Proveedor");

                        if (resultado.Succeeded)
                        {
                            //Se asigna el rol del usuario en la tabla AspNetUserRoles
                            await _userManager.AddToRoleAsync(usuario, Rol);
                        }
                        else
                        {
                            rm.Message = resultado.Errors.ToString();
                        }
                    }

                    string linkExpo = "https://expomr.online";

                    string body = "<br/>Hola " + UserName + "<br/><br/>"
                            + "Bienvenido a Bordo expo Medicinas Rosario 2025<br/><br/>"
                            + "Ingresa en <a href='" + linkExpo + "'>" + linkExpo + "</a><br/><br/>"
                            + "Esta es la información de tu cuenta y solo podrás utilizarla los días que dure el evento:<br/><br/>"
                            + "Usuario: " + UserName + "<br/>"
                            + "Contraseña: Expo2025" + "<br/>"
                            + "(Esta es una contraseña generada automáticamente, te sugerimos cambiarla la primera vez que ingreses)<br/><br/><br/><br/>"
                            + "Si tienes alguna duda o comentario, te solicitamos nos envíes un correo electrónico a la siguiente dirección "
                            + "<a href='mailto:'>Soporte ExpoMR</a><br/><br/><br/>"
                            + avisoConfidencialidad;

                    try
                    {
                        await _emailSender.SendEmailAsync(UserName, "Bienvenido a Expo Virtual Medicinas Rosario", body);

                        WebSite.InsertFeNotificacion(IdEmail);
                        WebSite.InsertIinvitacion(IdEmail, "SUCCESS");

                    }
                    catch (Exception e)
                    {
                        WebSite.InsertIinvitacion(IdEmail, e.Message);
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
                    Usuario.Usuario = (bool)dr["Usuario"];
                    Usuario.Lock = (bool)dr["Lock"];
                    Usuario.Sucursal = (string)dr["Sucursal"];
                    Usuario.Ruta = (string)dr["Ruta"];
                    Usuario.Invitacion = (string)dr["Invitacion"];
                    Usuario.InvMasiva = (bool)dr["InvMasiva"];

                    Usuarios.Add(Usuario);
                }

			    dB.Close();

			    return Json(Usuarios);
            }

        #endregion

        #region PROVEEDORES

		public IActionResult EditProveedor(int IdProveedor = 0)
		{
            EditarProveedorViewModel Proveedor = new EditarProveedorViewModel();

            string query = "SELECT mProveedores.IdPersona, mProveedores.Nombre, mProveedores.Descripcion, ISNULL(mMonedero.Saldo,0) AS Saldo, COUNT(mLaboratorios.IdLaboratorio) AS NumLabAsignados " +
                            "FROM mProveedores LEFT OUTER JOIN " +
                            "mMonedero ON mProveedores.IdPersona = mMonedero.IdPersona LEFT OUTER JOIN " +
                            "mLaboratorios ON mProveedores.IdPersona = mLaboratorios.Proveedor " +
                            "WHERE mProveedores.IdPersona = @IdProveedor " +
                            "GROUP BY mProveedores.IdPersona, mProveedores.Nombre, mProveedores.Descripcion, mMonedero.Saldo";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProveedor", IdProveedor);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                if (dr["IdPersona"] != null)
                {
                    Proveedor.IdPersona = (int)dr["IdPersona"];
                    Proveedor.Nombre = (string)dr["Nombre"];
                    Proveedor.Descripcion = (string)dr["Descripcion"];
                    Proveedor.Saldo = (int)dr["Saldo"];
                    Proveedor.NumLabAsignados = (int)dr["NumLabAsignados"];
                }
                else
                {
                    Proveedor = new EditarProveedorViewModel();
                    Proveedor.IdPersona = IdProveedor;
                    Proveedor.Nombre = "";
                    Proveedor.Descripcion = "";
                    Proveedor.Saldo = 0;
                }
            }
			    
            dB.Close();
			
            return View(Proveedor);
		}

        public IActionResult Proveedor()
        {
            return View();
        }

        [HttpPost]
        public JsonResult AddLab(int IdPersona, int idLaboratorio)
        {
            var rm = new ResponseModel();

            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@proveedor", IdPersona);
                parameters[1] = new SqlParameter("@laboratorio", idLaboratorio);

                dB.ExecuteNonQuery("InicializaLaboratorio", ref parameters);

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
        public JsonResult addProdProv(int idProducto, int idLaboratorio, int iva)
        {
            var rm = new ResponseModel();

            string query = "INSERT mProductos (IdProducto, IdLaboratorio, Iva) "
            + "VALUES(@idProducto, @idLaboratorio, @iva)";

        try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[3];
                parameters[0] = new SqlParameter("@idProducto", idProducto);
                parameters[1] = new SqlParameter("@idLaboratorio", idLaboratorio);
                parameters[2] = new SqlParameter("@iva", iva);

                dB.ExecuteNonQuery(query, ref parameters, commandType: CommandType.Text);

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
        public JsonResult AddProveedor(int IdPersona)
        {
            var rm = new ResponseModel();

            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@IdPersona", IdPersona);

                dB.ExecuteNonQuery("AddProv", ref parameters);

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
        public JsonResult DeleteLab(int idLaboratorio)
        {
            var rm = new ResponseModel();

            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@idLaboratorio", idLaboratorio);

                if (Convert.ToBoolean(dB.ExecuteReturnValue("DeleteLabProv", ref parameters)))
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
        public JsonResult DeleteProveedor(int IdPersona)
        {
            var rm = new ResponseModel();

            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@proveedor", IdPersona);

                if (Convert.ToBoolean(dB.ExecuteReturnValue("DeleteProv", ref parameters)))
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

        [HttpPost]
        public JsonResult GetAddLabs()
        {
            List<mLaboratorio> laboratorios = new();
                
            string query = "SELECT Lab.IdLaboratorio, Lab.Laboratorio FROM GetLaboratoriosProv(@IdLaboratorio, @Asignados) AS Lab";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@IdLaboratorio", "0");
            parameters[1] = new SqlParameter("@Asignados", false);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                mLaboratorio laboratorio = new mLaboratorio();

                laboratorio.IdLaboratorio = (Int16)dr["IdLaboratorio"];
                laboratorio.Laboratorio = (string)dr["Laboratorio"];

                laboratorios.Add(laboratorio);
            }

			dB.Close();

			return Json(laboratorios);
        }

        [HttpPost]
        public JsonResult GetAddProductosProv(int IdProveedor)
        {
            List<AddProductoProv> addProductos = new();

            string query = "SELECT IndigoProd.IdProducto, IndigoProd.Nombre + ' ' + IndigoProd.Presentacion + ' - ' + IndigoProd.Unidades AS Producto, IndigoProd.IdLaboratorio, IndigoProd.Iva "
                            + "FROM Indigo.dbo.vProductos AS IndigoProd "
                            + "WHERE IndigoProd.Status IN (0, 3, 4, 5) " // --0 = Activo, 3 = Nuevo, 4 = Liquidación, 5 = Pre Nuevo
                            + "AND IndigoProd.IdLaboratorio IN (SELECT mLaboratorios.IdLaboratorio FROM ExpoMR.dbo.mLaboratorios WHERE Proveedor = @IdProveedor) "
                            + "AND IndigoProd.IdProducto NOT IN (SELECT mProductos.IdProducto FROM ExpoMR.dbo.mProductos)";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProveedor", IdProveedor);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                AddProductoProv producto = new AddProductoProv();

                producto.IdProducto = (Int16)dr["IdProducto"];
                producto.Producto = (string)dr["Producto"];
                producto.IdLaboratorio = (Int16)dr["IdLaboratorio"];
                producto.Iva = (bool)dr["Iva"];

                addProductos.Add(producto);
            }

            dB.Close();

            return Json(addProductos);
        }

        [HttpPost]
        public JsonResult GetLaboratorios(int IdPersona)
        {
            List<mLaboratorio> laboratorios = new();

            string query = "SELECT Lab.IdLaboratorio, Lab.Laboratorio FROM GetLaboratoriosProv(@IdLaboratorio, @Asignados) AS Lab";

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@IdLaboratorio", IdPersona);
            parameters[1] = new SqlParameter("@Asignados", true);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                mLaboratorio laboratorio = new mLaboratorio();

                laboratorio.IdLaboratorio = (Int16)dr["IdLaboratorio"];
                laboratorio.Laboratorio = (string)dr["Laboratorio"];

                laboratorios.Add(laboratorio);
            }

			dB.Close();

			return Json(laboratorios);
        }

        [HttpPost]
        public JsonResult GetProveedores()
        {
            List<mProveedores> Proveedores = new();

            string query = "SELECT Proveedores.IdPersona, UPPER(Proveedores.Nombre) AS Nombre, UPPER(Proveedores.Descripcion) AS Descripcion, Proveedores.Alta " +
                            "FROM ( " +
                            "		SELECT mPerEmp.IdPersona, LTRIM(mPersonas.Paterno + ' ' + mPersonas.Materno + ' ' + mPersonas.Nombre) AS Nombre, ISNULL(mPersonas.Descripcion, '') AS Descripcion, CAST(0 AS BIT) AS Alta " +
                            "		FROM Indigo.dbo.mPersonas AS mPersonas INNER JOIN " +
                            "		Indigo.dbo.mPerEmp AS mPerEmp on mPersonas.Persona = mPerEmp.Persona INNER JOIN " +
                            "		Indigo.dbo.vRoles AS vRoles on mPerEmp.IdPersona = vRoles.IdPersona " +
                            "		WHERE mPerEmp.IdEmpresa = @IdEmpresa " +
                            "		AND vRoles.Proveedor = 1 " +
                            "		AND mPerEmp.IdPersona NOT IN (SELECT mProveedores.IdPersona FROM mProveedores) " +
                            "		UNION " +
                            "		SELECT mProveedores.IdPersona, mProveedores.Nombre, mProveedores.Descripcion, CAST(1 AS BIT) AS Alta " +
                            "		FROM ExpoMR.dbo.mProveedores " +
                            "	)Proveedores " +
                            "ORDER BY Proveedores.Alta DESC, Proveedores.Nombre";

        DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdEmpresa", 1);

            SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

            while (dr.Read())
            {
                mProveedores Proveedor = new mProveedores();

                Proveedor.IdPersona = (int)dr["IdPersona"];
                Proveedor.Nombre = (string)dr["Nombre"];
                Proveedor.Descripcion = (string)dr["Descripcion"];
                Proveedor.Alta = (bool)dr["Alta"];

                Proveedores.Add(Proveedor);
            }

			dB.Close();

			return Json(Proveedores);
        }

		[HttpPost]
		public JsonResult GetOfertasProv(int IdPersona)
		{
			List<OfertasProv> OfertasProv = new();

			string query = "SELECT Ofertas.IdProducto, Ofertas.Producto, Ofertas.Descuento, Ofertas.Laboratorio FROM GetOfertasProv (@IdProveedor) AS Ofertas";

			DB dB = new();
			SqlParameter[] parameters = new SqlParameter[1];
			parameters[0] = new SqlParameter("@IdProveedor", IdPersona);


			SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

			while (dr.Read())
			{
				OfertasProv Oferta = new OfertasProv();

                Oferta.IdProducto = (Int16)dr["IdProducto"];
                Oferta.Producto = (string)dr["Producto"];
                Oferta.Descuento = (decimal)dr["Descuento"];
                Oferta.Laboratorio = (string)dr["Laboratorio"];

				OfertasProv.Add(Oferta);
			}

			dB.Close();

			return Json(OfertasProv);
		}

        [HttpPost]
        public JsonResult ActualizaDescProv(int IdProducto, decimal DescProvNew) 
        {
            var rm = new ResponseModel();

            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[3];
                parameters[0] = new SqlParameter("@IdProducto", IdProducto);
                parameters[1] = new SqlParameter("@Descuento", DescProvNew);
                parameters[2] = new SqlParameter("@IdEmail", HttpContext.Session.GetString("Session_IdEmail"));

                dB.ExecuteNonQuery("ActualizaDescProv", ref parameters);
                rm.Result = true;
                rm.Message = "Descuento agregado con exito";
            }
            catch (Exception ex)
            {
                rm.Message = ex.Message;
            }

            return Json(rm);
        }

        [HttpPost]
        public JsonResult SaveMonedas(int IdPersona, int monedas)
        {
            var rm = new ResponseModel();

            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[5];
                parameters[0] = new SqlParameter("@origen", "0"); //0 = MR
                parameters[1] = new SqlParameter("@destino", IdPersona);
                parameters[2] = new SqlParameter("@idmotivo", 11); //11 = Compra Monedas
                parameters[3] = new SqlParameter("@monedas", monedas);
                parameters[4] = new SqlParameter("@usuario", HttpContext.Session.GetString("ClienteSel"));

                if (Convert.ToBoolean(dB.ExecuteReturnValue("MonedasAsignar", ref parameters)))
                {
                    DB dB2 = new();
                    SqlParameter[] parameters2 = new SqlParameter[1];
                    parameters2[0] = new SqlParameter("@IdProveedor", IdPersona);

                    rm.Result = dB2.ExecuteReturnValue("GetMonedasProv", ref parameters2);
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
        public JsonResult SaveProvedor(string nombre, string descripcion, int IdPersona)
        {
            var rm = new ResponseModel();

            try
            {
                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[3];
                parameters[0] = new SqlParameter("@IdProveedor", IdPersona);
                parameters[1] = new SqlParameter("@Nombre", nombre);
                parameters[2] = new SqlParameter("@Descripcion", descripcion);

                dB.ExecuteNonQuery("EditProv", ref parameters);

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

        #endregion

        #region EMPLEADOS

            public IActionResult Empleados()
            {
                return View();
            }

            public List<Roles> Roles()
            {
                List<Roles> Roles = new();

                string query = "SELECT '' AS IdRol, '' AS Role " +
                                "UNION " +
                                "SELECT AspNetRoles.Id AS IdRol, AspNetRoles.Name AS Role " +
                                "FROM AspNetRoles " +
                                "WHERE AspNetRoles.Name IN ('Administrador','Capturista','Vendedor')  " +
                                "ORDER BY Role";

            DB dB = new();
                SqlParameter[] parameters = new SqlParameter[0];

                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

                while (dr.Read())
                {
                    Roles Rol = new Roles();

                    Rol.IdRol = (string)dr["IdRol"];
                    Rol.Role = (string)dr["Role"];

                    Roles.Add(Rol);
                }

			    dB.Close();

			    return Roles;
            }

            public async Task<JsonResult> CambiarRolEmpleado(string UserName, string Rol)
            {
                var rm = new ResponseModel();
                rm.Response = false;

                var user = await _userManager.FindByNameAsync(UserName);

                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Count() != 0)
                        await _userManager.RemoveFromRolesAsync(user, roles);

                    if (Rol != null)
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

            public async Task<IActionResult> EditarEmpleado(int IdPersona = 0)
            {
                Empleados Empleado = new Empleados();

                string query = "SELECT Empleado.IdPersona, Empleado.Nombre, Empleado.Email, Empleado.IdRol, Empleado.Rol FROM GetEmpleado(@IdPersona) AS Empleado";

                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@IdPersona", IdPersona);

                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

                while (dr.Read())
                {
                    Empleado.IdPersona = (int)dr["IdPersona"];
                    Empleado.Nombre = (string)dr["Nombre"];
                    Empleado.Email = (string)dr["Email"];
                    Empleado.IdRol = (string)dr["IdRol"];
                    Empleado.Rol = (string)dr["Rol"];
                }

			    dB.Close();

			    Empleado.Roles = Roles();  //Se obtienen los roles para el usuario         
                
                return View(Empleado);
            }

            [HttpPost]
            public JsonResult GetEmpleados()
            {
                List<Empleados> Empleados = new();

                string query = "SELECT Empleado.IdPersona, Empleado.Nombre, Empleado.Email, Empleado.IdRol, Empleado.Rol FROM GetEmpleado(@IdPersona) AS Empleado";

                DB dB = new();
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@IdPersona", "0");

                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

                while (dr.Read())
                {
                    Empleados Empleado = new Empleados();

                    Empleado.IdPersona = (int)dr["IdPersona"];
                    Empleado.Nombre = (string)dr["Nombre"];
                    Empleado.Email = (string)dr["Email"];
                    Empleado.IdRol = (string)dr["IdRol"];
                    Empleado.Rol = (string)dr["Rol"];

                    Empleados.Add(Empleado);
                }

                return Json(Empleados);
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
                List<UsuariosNotificacion> Notificaciones = new();

                string query = "SELECT mClientes.IdPersona, mClientes.Nombre, ISNULL(lNotificaciones.IdTipoNotificacion, 0) AS Notificado, mEmails.IdEmail, LOWER(mEmails.Email) AS Email " +
                                "FROM mClientes INNER JOIN " +
                                "mUsuarios ON mClientes.IdPersona = mUsuarios.IdPersona INNER JOIN " +
                                "Indigo.dbo.mEmails ON mUsuarios.IdEmail = mEmails.IdEmail INNER JOIN " +
                                "dPromoCliente ON mClientes.IdPersona = dPromoCliente.Cliente LEFT OUTER JOIN " +
                                "lNotificaciones ON mClientes.IdPersona = lNotificaciones.IdPersona AND IdTipoNotificacion = 1 " +
                                "GROUP BY mClientes.IdPersona, mClientes.Nombre, mEmails.IdEmail, mEmails.Email, Lnotificaciones.IdTipoNotificacion " +
                                "ORDER BY Lnotificaciones.IdTipoNotificacion ASC, mClientes.Nombre";

            DB dB = new();
                SqlParameter[] parameters = new SqlParameter[0];

                SqlDataReader dr = dB.ExecuteReader(query, ref parameters, commandType: CommandType.Text);

                while (dr.Read())
                {
                    UsuariosNotificacion Notificacion = new UsuariosNotificacion();

                    Notificacion.IdPersona = (int)dr["IdPersona"];
                    Notificacion.Nombre = (string)dr["Nombre"];
                    Notificacion.Notificado = (byte)dr["Notificado"];
				    Notificacion.IdEmail = (int)dr["IdEmail"];
				    Notificacion.Email = (string)dr["Email"];

                    Notificaciones.Add(Notificacion);
                }

			    dB.Close();

			    return Json(Notificaciones);
            }

        #endregion

    }
}