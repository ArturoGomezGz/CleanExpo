using ExpoMR.Clases;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Data;
using System.Drawing.Printing;
using System.Text.RegularExpressions;

namespace ExpoMR.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdministracionController : Controller
    {
        private static IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public AdministracionController(UserManager<IdentityUser> userManager, IEmailSender emailSender, IConfiguration config)
        {
            _emailSender = emailSender;
            _userManager = userManager;
            _configuration = config;
        }


        #region USUARIOS
        public async Task<IActionResult> Usuarios()
        {
            var Administracion = new AdminstracionViewModel();
            Administracion.IdTipoUsuario = 1;
            Administracion.TipoUsuario = GetTipoUsuarios();
            Administracion.Ruta = 51;
            Administracion.Rutas = GetRutas();
            Administracion.IdSucrsal = 1;
            Administracion.Sucursales = GetCmbSucursales();


            return View(Administracion);
        }
        [HttpPost]
        public JsonResult GetUsuarios(int IdTipoUsuario, int IdRuta = 0)
        {
            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand cmd = new SqlCommand("GetUsuariosExpo", cnn);
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA;
            cnn.Open();
            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;
            //// 3. add parameter to command, which will be passed to the stored procedure
            cmd.Parameters.Add(new SqlParameter("@IdTipoPersona", IdTipoUsuario));
            cmd.Parameters.Add(new SqlParameter("@Filtro", ""));
            cmd.Parameters.Add(new SqlParameter("@IdRuta", IdRuta));
            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dataTable);
            cnn.Close();

            if (IdTipoUsuario == 1)
            {
                var jsonData = new
                {
                    rows = (
                      from col in dataTable.AsEnumerable()
                      select new
                      {
                          idpersona = col[0],
                          acceder = col[0],
                          nombre = col[1],
                          email = col[2],
                          tipo = col[3],
                          sucursal = col[7],
                          invitacion = col[9],
                          estado = col[5],
                          usuario = col[5],
                          agregar = col[5]
                      }).ToArray()
                };

                return Json(jsonData.rows);
            }
            else {
                var jsonData = new
                {
                    rows = (
                          from col in dataTable.AsEnumerable()
                          select new
                          {
                              idpersona = col[0],
                              acceder = col[0],
                              nombre = col[1],
                              email = col[2],
                              tipo = col[3],
                              sucursal = col[7],
                              invitacion = "",
                              estado = col[6],
                              usuario = col[5],
                              agregar = col[0]
                          }).ToArray()
                };

                return Json(jsonData.rows);
            }
           
        }
        public IActionResult EditarUsuario(int IdPersona = 0) { 

            EditarUsuarioViewModel vmUser =new EditarUsuarioViewModel();

            //SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "SELECT vPersonas.IdPersona, vPersonas.Nombre, vPersonas.Email, "
            //                   +" CASE WHEN mUsuarios.IdTipoUsuario IS NOT NULL THEN mUsuarios.IdTipoUsuario"
            //                   + "      ELSE vPersonas.IdTipoUsuario END AS IdTipoUsuario,"
            //                   + " vPersonas.VIP, vPersonas.Usuario, CAST(vPersonas.Lock AS TINYINT) AS Lock,"
            //                   + " CASE WHEN vPersonas.Sucursal = 0 THEN ''"
            //                   + "      ELSE CASE WHEN vPersonas.Sucursal = 1 THEN 'GUADALAJARA'"
            //                   + "                ELSE CASE WHEN vPersonas.Sucursal = 2  THEN 'QUERETARO'"
            //                   + "                          ELSE 'MEXICO'"
            //                   + "                    END"
            //                   + "           END"
            //                   + " END AS Sucursal, mUsuarios.FeUltimaNotificacion AS Invitacion,vPersonas.Sucursal AS IdSucursal"
            //                   + " FROM vPersonas LEFT OUTER JOIN"
            //                   + " mUsuarios ON vPersonas.IdPersona = mUsuarios.IdPersona"
            //                   + " WHERE vPersonas.IdPersona = " + IdPersona;
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();
            string query = "SELECT vPersonas.IdPersona, vPersonas.Nombre, vPersonas.Email, "
                               + " CASE WHEN mUsuarios.IdTipoUsuario IS NOT NULL THEN mUsuarios.IdTipoUsuario"
                               + "      ELSE vPersonas.IdTipoUsuario END AS IdTipoUsuario,"
                               + " vPersonas.VIP, vPersonas.Usuario, CAST(vPersonas.Lock AS TINYINT) AS Lock,"
                               + " CASE WHEN vPersonas.Sucursal = 0 THEN ''"
                               + "      ELSE CASE WHEN vPersonas.Sucursal = 1 THEN 'GUADALAJARA'"
                               + "                ELSE CASE WHEN vPersonas.Sucursal = 2  THEN 'QUERETARO'"
                               + "                          ELSE 'MEXICO'"
                               + "                    END"
                               + "           END"
                               + " END AS Sucursal, mUsuarios.FeUltimaNotificacion AS Invitacion,vPersonas.Sucursal AS IdSucursal"
                               + " FROM vPersonas LEFT OUTER JOIN"
                               + " mUsuarios ON vPersonas.IdPersona = mUsuarios.IdPersona"
                               + " WHERE vPersonas.IdPersona = " + IdPersona;

            DataTable dataTable = sqlQueryDT("ExpoMRCS", query);


            vmUser.IdPersona = dataTable.Rows[0].Field<int>(0);
            vmUser.Usuario = dataTable.Rows[0].Field<string>(1);
            vmUser.Email = dataTable.Rows[0].Field<string>(2);
            vmUser.IdTipoUsuario = dataTable.Rows[0].Field<int>(3);
            vmUser.TipoUsuario = GetTipoUsuarios();
            vmUser.VIP = dataTable.Rows[0].Field<int>(4) == 0 ? "No VIP" : "VIP";
            vmUser.Lock = dataTable.Rows[0].Field<byte>(6) == 0 ? "Lock" : "Libre";
            vmUser.IdSucursal = dataTable.Rows[0].Field<byte>(9);
            vmUser.Sucursales = GetCmbSucursales();
            vmUser.Invitacion = dataTable.Rows[0].Field<string>(8) == null ? "" : "Success";

            return View(vmUser);
        }
        public List<TipoUsuario> GetTipoUsuarios() {
            List<TipoUsuario> vmTipo = new List<TipoUsuario>();

            string query = "SELECT mTipoUsuario.IdTipoUsuario, mTipoUsuario.TipoUsuario " +
                " FROM mTipoUsuario " +
                " WHERE mTipoUsuario.IdTipoUsuario <> 3";
            DataTable dataTable = sqlQueryDT("ExpoMRCS", query);

            foreach (DataRow row in dataTable.Rows)
            {
                TipoUsuario Tipo = new TipoUsuario();
                Tipo.IdTipoUsuario = Convert.ToInt16(row.Field<byte>(0));
                Tipo.TipoUsuarios = row.Field<string>(1);
                vmTipo.Add(Tipo);
            }
            return vmTipo;
        }
        public List<RutasList> GetRutas() {
            List<RutasList> vmRuta = new List<RutasList>();

            string query = "SELECT mRutas.IdRuta, mRutas.Descripcion " +
                "FROM mRutas ORDER BY " +
                "mRutas.Descripcion";
            DataTable dataTable = sqlQueryDT("IndigoCS", query);

            foreach (DataRow row in dataTable.Rows)
            {
                RutasList Ruta = new RutasList();
                Ruta.IdRuta = Convert.ToInt16(row.Field<byte>(0));
                Ruta.Ruta = row.Field<string>(1);
                vmRuta.Add(Ruta);
            }
            return vmRuta;
        }
        public List<SucursalUsuario> GetCmbSucursales()
        {
            List<SucursalUsuario> vmSucursal = new List<SucursalUsuario>();
            vmSucursal.Add(new SucursalUsuario { IdSucrsal = 0, Sucursal="Seleccione una Opcion..." });
            vmSucursal.Add(new SucursalUsuario { IdSucrsal = 1, Sucursal="Guadalajara" });
            vmSucursal.Add(new SucursalUsuario { IdSucrsal = 2, Sucursal="Queretaro" });
            vmSucursal.Add(new SucursalUsuario { IdSucrsal = 3, Sucursal="México" });
            return vmSucursal;
        }
        #endregion

        #region PROVEEDORES
        public IActionResult Proveedor()
        {
            return View();
        }
        [HttpPost]
        public JsonResult GetProveedores()
        {
            //SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "SELECT Proveedores.IdPersona, Proveedores.Nombre, Proveedores.Descripcion, Proveedores.Alta"
            //                  + "  FROM("
            //                  + "       SELECT mPerEmp.IdPersona, LTRIM(mPersonas.Paterno + ' ' + mPersonas.Materno + ' ' + mPersonas.Nombre) AS Nombre, mPersonas.Descripcion, 0 AS Alta"
            //                  + "         FROM Indigo.dbo.mPersonas AS mPersonas INNER JOIN"
            //                  + "         Indigo.dbo.mPerEmp AS mPerEmp on mPersonas.Persona = mPerEmp.Persona INNER JOIN"
            //                  + "         Indigo.dbo.vRoles AS vRoles on mPerEmp.IdPersona = vRoles.IdPersona"
            //                  + "         WHERE mPerEmp.IdEmpresa = 1"
            //                  + "             AND vRoles.Proveedor = 1"
            //                  + "              AND mPerEmp.IdPersona NOT IN(SELECT mProveedores.IdPersona FROM mProveedores)"
            //                  + "              UNION"
            //                  + "              SELECT mProveedores.IdPersona, mProveedores.Nombre, mProveedores.Descripcion, 1 AS Alta"
            //                  + "              FROM dbo.mProveedores"
            //                  + "          )Proveedores"
            //                  + "      --WHERE Proveedores.Nombre LIKE '%' + @Filtro + '%'"
            //                  + "      ORDER BY Proveedores.Alta DESC, Proveedores.Nombre";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();
            string query = "SELECT Proveedores.IdPersona, Proveedores.Nombre, Proveedores.Descripcion, Proveedores.Alta"
                              + "  FROM("
                              + "       SELECT mPerEmp.IdPersona, LTRIM(mPersonas.Paterno + ' ' + mPersonas.Materno + ' ' + mPersonas.Nombre) AS Nombre, mPersonas.Descripcion, 0 AS Alta"
                              + "         FROM Indigo.dbo.mPersonas AS mPersonas INNER JOIN"
                              + "         Indigo.dbo.mPerEmp AS mPerEmp on mPersonas.Persona = mPerEmp.Persona INNER JOIN"
                              + "         Indigo.dbo.vRoles AS vRoles on mPerEmp.IdPersona = vRoles.IdPersona"
                              + "         WHERE mPerEmp.IdEmpresa = 1"
                              + "             AND vRoles.Proveedor = 1"
                              + "              AND mPerEmp.IdPersona NOT IN(SELECT mProveedores.IdPersona FROM mProveedores)"
                              + "              UNION"
                              + "              SELECT mProveedores.IdPersona, mProveedores.Nombre, mProveedores.Descripcion, 1 AS Alta"
                              + "              FROM dbo.mProveedores"
                              + "          )Proveedores"
                              + "      --WHERE Proveedores.Nombre LIKE '%' + @Filtro + '%'"
                              + "      ORDER BY Proveedores.Alta DESC, Proveedores.Nombre";

            DataTable dataTable = sqlQueryDT("ExpoMRCS", query);


            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           acceder = col[0],
                           nombre = col[1],
                           email = col[2],
                           rol = col[3]
                       }).ToArray()
            };

            return Json(jsonData.rows);
        }
        [HttpPost]
        public JsonResult GetLaboratorios(int IdPersona)
        {
            //SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "SELECT mLaboratorios.IdLaboratorio, mLaboratorios.Laboratorio"
            //                  +"  FROM mLaboratorios"
            //                  +"  WHERE mLaboratorios.Proveedor =" + IdPersona;
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();

            string query = "SELECT mLaboratorios.IdLaboratorio, mLaboratorios.Laboratorio"
                              + "  FROM mLaboratorios"
                              + "  WHERE mLaboratorios.Proveedor =" + IdPersona;

            DataTable dataTable = sqlQueryDT("ExpoMRCS", query);

            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           acceder = col[0],
                           laboratorio = col[1]
                       }).ToArray()
            };

            return Json(jsonData.rows);
        }
        [HttpPost]
        public JsonResult GetAddLabs(int IdPersona=0)
        {
            //SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "SELECT mLaboratorios.IdLaboratorio, mLaboratorios.Laboratorio"
            //                  +"  FROM Indigo.dbo.mLaboratorios"
            //                  +"  WHERE mLaboratorios.IdLaboratorio NOT IN(SELECT mLaboratorios.IdLaboratorio FROM mLaboratorios)"
            //                  +"  ORDER BY mLaboratorios.Laboratorio";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();
            string query = "SELECT mLaboratorios.IdLaboratorio, mLaboratorios.Laboratorio"
                              + "  FROM Indigo.dbo.mLaboratorios"
                              + "  WHERE mLaboratorios.IdLaboratorio NOT IN(SELECT mLaboratorios.IdLaboratorio FROM mLaboratorios)"
                              + "  ORDER BY mLaboratorios.Laboratorio";

            DataTable dataTable = sqlQueryDT("ExpoMRCS", query);

            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           acceder = col[0],
                           laboratorio = col[1]
                       }).ToArray()
            };

            return Json(jsonData.rows);
        }
        public IActionResult EditProveedor(int IdProveedor = 0) {
            EditarProveedorViewModel Provedor = new EditarProveedorViewModel();
            //SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "SELECT mProveedores.IdPersona, mProveedores.Nombre, mProveedores.Descripcion, ISNULL(mMonedero.Saldo,0) AS Saldo"
            //                   +" FROM mProveedores LEFT OUTER JOIN"
            //                   + " mMonedero ON mProveedores.IdPersona = mMonedero.IdPersona"
            //                   + "  WHERE mProveedores.IdPersona = " + IdProveedor;
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();
            string query = "SELECT mProveedores.IdPersona, mProveedores.Nombre, mProveedores.Descripcion, ISNULL(mMonedero.Saldo,0) AS Saldo"
                               + " FROM mProveedores LEFT OUTER JOIN"
                               + " mMonedero ON mProveedores.IdPersona = mMonedero.IdPersona"
                               + "  WHERE mProveedores.IdPersona = " + IdProveedor;

            DataTable dataTable = sqlQueryDT("ExpoMRCS", query);

            if (dataTable.Rows.Count > 0) {
                Provedor.IdPersona = dataTable.Rows[0].Field<int>(0);
                Provedor.Proveedor = dataTable.Rows[0].Field<string>(1);
                Provedor.Descripcion = dataTable.Rows[0].Field<string>(2);
                Provedor.Monedas = dataTable.Rows[0].Field<int>(3);
            }
            else
            {
                Provedor.IdPersona = IdProveedor;
                Provedor.Proveedor = "";
                Provedor.Descripcion = "";
                Provedor.Monedas = 0;

            }
            

            return View(Provedor);
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
            //SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "SELECT mClientes.IdPersona, mClientes.Nombre, "
            //                    +" ISNULL(lNotificaciones.IdTipoNotificacion, 0) AS Notificado, mUsuarios.UserName AS Email"
            //                    + " FROM mClientes INNER JOIN"
            //                    + " mUsuarios ON mClientes.IdPersona = mUsuarios.IdPersona INNER JOIN"
            //                    + " dPromoCliente ON mClientes.IdPersona = dPromoCliente.Cliente LEFT OUTER JOIN"
            //                    + " lNotificaciones ON mClientes.IdPersona = lNotificaciones.IdPersona AND IdTipoNotificacion = 1"
            //                    + " GROUP BY mClientes.IdPersona, mClientes.Nombre, mUsuarios.UserName, Lnotificaciones.IdTipoNotificacion"
            //                    + " ORDER BY Lnotificaciones.IdTipoNotificacion ASC, mClientes.Nombre";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();
            string query = "SELECT mClientes.IdPersona, mClientes.Nombre, "
                                + " ISNULL(lNotificaciones.IdTipoNotificacion, 0) AS Notificado, mUsuarios.UserName AS Email"
                                + " FROM mClientes INNER JOIN"
                                + " mUsuarios ON mClientes.IdPersona = mUsuarios.IdPersona INNER JOIN"
                                + " dPromoCliente ON mClientes.IdPersona = dPromoCliente.Cliente LEFT OUTER JOIN"
                                + " lNotificaciones ON mClientes.IdPersona = lNotificaciones.IdPersona AND IdTipoNotificacion = 1"
                                + " GROUP BY mClientes.IdPersona, mClientes.Nombre, mUsuarios.UserName, Lnotificaciones.IdTipoNotificacion"
                                + " ORDER BY Lnotificaciones.IdTipoNotificacion ASC, mClientes.Nombre";

            DataTable dataTable = sqlQueryDT("ExpoMRCS", query);
            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           idpersona = col[0],
                           nombre = col[1],
                           email = col[3],
                           promociones = col[2]
                       }).ToArray()
            };

            return Json(jsonData.rows);
        }

        public async Task<IActionResult> Not() {

            //if (!await Notifications.SendWelcome("mario","hola","123456", true).Result())
            //{
            //    return BadRequest();
            //}

            return View();
        }
        #endregion

        #region EMPLEADOS
        public IActionResult Empleados()
        {
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> GetEmpleados()
        {
            //SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "SELECT vPersonas.IdPersona, vPersonas.Nombre, mUsuarios.UserName AS Email, AspNetRoles.Name AS Rol"
            //                   + " FROM Indigo.dbo.vPersonas INNER JOIN"
            //                   + " mUsuarios  ON vPersonas.IdPersona = mUsuarios.IdPersona LEFT OUTER JOIN"
            //                   + " AspNetUsers ON mUsuarios.UserName = AspNetUsers.UserName LEFT OUTER JOIN"
            //                   + " AspNetUserRoles ON AspNetUsers.Id = AspNetUserRoles.UserId LEFT OUTER JOIN"
            //                   + " AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id"
            //                   + " WHERE mUsuarios.IdTipoUsuario IN(3,4) "
            //                   + " ORDER BY vPersonas.Nombre ASC";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();
            string query = "SELECT vPersonas.IdPersona, vPersonas.Nombre, mUsuarios.UserName AS Email, AspNetRoles.Name AS Rol"
                               + " FROM Indigo.dbo.vPersonas INNER JOIN"
                               + " mUsuarios  ON vPersonas.IdPersona = mUsuarios.IdPersona LEFT OUTER JOIN"
                               + " AspNetUsers ON mUsuarios.UserName = AspNetUsers.UserName LEFT OUTER JOIN"
                               + " AspNetUserRoles ON AspNetUsers.Id = AspNetUserRoles.UserId LEFT OUTER JOIN"
                               + " AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id"
                               + " WHERE mUsuarios.IdTipoUsuario IN(3,4) "
                               + " ORDER BY vPersonas.Nombre ASC";

            DataTable dataTable = sqlQueryDT("ExpoMRCS", query);
            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           acceder = col[0],
                           nombre = col[1],
                           email = col[2],
                           rol = col[3] == null || string.IsNullOrEmpty(col[3].ToString()) ? "" : col[3].ToString()
                       }).ToArray()
            };


            //foreach (var row in jsonData.rows) {
            //    var usuario = new AppUsuario
            //    {
            //        UserName = row.email.ToString(),
            //        Email = row.email.ToString(),
            //        Telefono = "",
            //        Nombre = "",
            //        Ciudad = "",
            //        Estado = true
            //    };
            //    var result = await _userManager.CreateAsync(usuario, "123456");

            //    if (result.Succeeded)
            //    {
            //        switch (Convert.ToInt32(row.idtipo))
            //        {
            //            case 4:
            //                await _userManager.AddToRoleAsync(usuario, "Administrador");
            //                break;
            //            case 3:
            //                await _userManager.AddToRoleAsync(usuario, "Vendedor");
            //                break;
            //        }
            //        Console.WriteLine("Ok");
            //    }
            //    else
            //    {
            //        Console.WriteLine("Fail");
            //    }
            //}
            
            
            return Json(jsonData.rows);
        }
        public async Task<IActionResult> EditarEmpleado(int IdPersona = 0)
        {
            EditarEmpleadoViewModel vwModel = new EditarEmpleadoViewModel();

            //SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "SELECT PerEmp.IdPersona, Personas.Nombre + ' ' + Personas.Paterno + ' ' + Personas.Materno AS Nombre, mUsuarios.UserName AS Email, AspNetRoles.Name AS Rol,  AspNetRoles.Id as IdRoles"
            //                  + "  FROM indigo.dbo.mPersonas AS Personas INNER JOIN"
            //                  + "   Indigo.dbo.mPerEmp AS PerEmp ON Personas.Persona = PerEmp.Persona LEFT OUTER JOIN"
            //                  + "   mUsuarios ON PerEmp.IdPersona = mUsuarios.IdPersona LEFT OUTER JOIN"
            //                  + "   mTipoUsuario ON mUsuarios.IdTipoUsuario = mTipoUsuario.IdTipoUsuario LEFT OUTER JOIN"
            //                  + "   AspNetUsers ON mUsuarios.UserName = AspNetUsers.UserName LEFT OUTER JOIN"
            //                  + "   AspNetUserRoles ON AspNetUsers.Id = AspNetUserRoles.UserId LEFT OUTER JOIN"
            //                  + "   AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id LEFT OUTER JOIN"
            //                  + "   Indigo.dbo.mEmpleados AS GDLEmp ON PerEmp.IdPersona = GDLEmp.IdPersona AND GDLEmp.Activo = 1 LEFT OUTER JOIN"
            //                  + "   QtroIndigo.dbo.mEmpleados AS QTROEmp ON PerEmp.IdPersona = QTROEmp.IdPersona AND QTROEmp.Activo = 1 LEFT OUTER JOIN"
            //                  + "   EdomxIndigo.dbo.mEmpleados AS EDOMXEmp ON PerEmp.IdPersona = EDOMXEmp.IdPersona AND EDOMXEmp.Activo = 1"
            //                  + "  WHERE PerEmp.IdPersona = " + IdPersona;
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();

            string query = "SELECT PerEmp.IdPersona, Personas.Nombre + ' ' + Personas.Paterno + ' ' + Personas.Materno AS Nombre, mUsuarios.UserName AS Email, AspNetRoles.Name AS Rol,  AspNetRoles.Id as IdRoles"
                              + "  FROM indigo.dbo.mPersonas AS Personas INNER JOIN"
                              + "   Indigo.dbo.mPerEmp AS PerEmp ON Personas.Persona = PerEmp.Persona LEFT OUTER JOIN"
                              + "   mUsuarios ON PerEmp.IdPersona = mUsuarios.IdPersona LEFT OUTER JOIN"
                              + "   mTipoUsuario ON mUsuarios.IdTipoUsuario = mTipoUsuario.IdTipoUsuario LEFT OUTER JOIN"
                              + "   AspNetUsers ON mUsuarios.UserName = AspNetUsers.UserName LEFT OUTER JOIN"
                              + "   AspNetUserRoles ON AspNetUsers.Id = AspNetUserRoles.UserId LEFT OUTER JOIN"
                              + "   AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id LEFT OUTER JOIN"
                              + "   Indigo.dbo.mEmpleados AS GDLEmp ON PerEmp.IdPersona = GDLEmp.IdPersona AND GDLEmp.Activo = 1 LEFT OUTER JOIN"
                              + "   QtroIndigo.dbo.mEmpleados AS QTROEmp ON PerEmp.IdPersona = QTROEmp.IdPersona AND QTROEmp.Activo = 1 LEFT OUTER JOIN"
                              + "   EdomxIndigo.dbo.mEmpleados AS EDOMXEmp ON PerEmp.IdPersona = EDOMXEmp.IdPersona AND EDOMXEmp.Activo = 1"
                              + "  WHERE PerEmp.IdPersona = " + IdPersona;

            DataTable dataTable = sqlQueryDT("ExpoMRCS", query);

            vwModel.IdPersona = dataTable.Rows[0].Field<int>(0);
            vwModel.Empleado = dataTable.Rows[0].Field<string>(1);
            vwModel.Email = dataTable.Rows[0].Field<string>(2);
            vwModel.Rol = dataTable.Rows[0].Field<string>(3);
            vwModel.IdRol = dataTable.Rows[0].Field<string>(4) == null ? "": dataTable.Rows[0].Field<string>(4);
            vwModel.Roles = roles();

            return View(vwModel);
        }
        public List<RoleIdPersona> roles() {
            List<RoleIdPersona> roles = new List<RoleIdPersona>
            {
                new RoleIdPersona { IdRol = "", Role = "Seleccione un Rol" }
            };
            //SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "select Id as IdRoles, [Name] as Nombre" +
            //    " from AspNetRoles" +
            //    " order by Nombre desc ";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);

            string query = "select Id as IdRoles, [Name] as Nombre" +
                " from AspNetRoles" +
                " order by Nombre desc ";

            DataTable dataTable = sqlQueryDT("ExpoMRCS", query);

            foreach (DataRow row in dataTable.Rows)
            {
                RoleIdPersona GetRol = new RoleIdPersona();
                GetRol.IdRol = row.Field<string>(0);
                GetRol.Role = row.Field<string>(1);
                roles.Add(GetRol);
            }
            
            return roles;
        }
     #endregion

        public IActionResult Credito()
        {
            return View();
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
    }
}
