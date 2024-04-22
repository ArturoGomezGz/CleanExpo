#define TESTMODE
using ExpoMR.Clases;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ExpoMR.Controllers
{
    [Authorize]
    public class NotificacionesController : Controller
    {
        private IConfiguration _configurationDB;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _rolManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;

        //Variables para verificar La sesión Y el Usuario
        public NotificacionesController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender, RoleManager<IdentityRole> rolManager, IConfiguration configurationDB)
        {
            _userManager = userManager;
            _rolManager = rolManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _configurationDB = configurationDB;
        }

        private string pMessage = "";
        private string avisoConfidencialidad = "AVISO DE CONFIDENCIALIDAD: Este mensaje es confidencial y/o puede contener información privada.<br/>"
                                            + "Si usted no es el destinatario o no es alguna persona autorizada por éste para recibir el mensaje,<br/>"
                                            + "usted no deberá utilizar, copiar, revelar o tomar ninguna acción basada en este mensaje.<br/>"
                                            + "Si recibe este mensaje por error, por favor notifíquelo de inmediato al remitente y bórrelo de su computadora.<br/>";
        public async Task<string> SendWelcome(string name, string email, string password, bool vip)
        {
            string linkExpo = vip ? "https://expomr.online/Presencial.aspx" : "https://expomr.online";

            string body = "<br/>Hola " + name + "<br/><br/>"
                    + "Medicinas Rosario te da la bienvenida a su Expo Virtual 2022<br/><br/>"
                    + "Ingresa en <a href='" + linkExpo + "'>" + linkExpo + "</a><br/><br/>"
                    + "Esta es la información de tu cuenta y solo podrás utilizarla los días que dure el evento:<br/><br/>"
                    + "Usuario: " + email + "<br/>"
                    + "Contraseña:  " + password + "<br/>"
                    + "(Esta es una contraseña generada automáticamente, te sugerimos cambiarla la primera vez que ingreses)<br/><br/><br/><br/>"
                    + "Si tienes alguna duda o comentario, te solicitamos nos envíes un correo electrónico a la siguiente dirección "
                    + "<a href='mailto:'>Soporte ExpoMR</a><br/><br/><br/>"
                +avisoConfidencialidad;
            try
            {
#if (TESTMODE)
                await _emailSender.SendEmailAsync("pruebas@medicinasrosario.com.mx", "Asunto", body);
#else
                    
                await _emailSender.SendEmailAsync(email, "Asunto", body);
#endif


                pMessage = "Mensaje Enviado Con exíto";
            }
            catch (Exception e)
            {
                pMessage = "Hubo un error al envir el correo: "+ e.Message;
            }

            return pMessage;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> OlvidoPassword(OlvidoPasswordViewModel opViewModel)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _userManager.FindByEmailAsync(opViewModel.Email);
                if (usuario == null)
                {
                    return RedirectToAction("ConfirmacionOlvidoPassword");
                }
                var codigo = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                var urlRetorno = Url.Action("ResetPassword", "Cuentas", new { userId = usuario.Id, code = codigo }, protocol: HttpContext.Request.Scheme);

                string body = "Restablece tu Contraseña<br/><br/><br/>"
                        + "Por favor restablece tu contraseña haciendo click"
                        + "<a href=\"" + urlRetorno + "\"> Aquí </a><br/><br/><br/>"
                        + "Si tienes alguna duda o comentario, te solicitamos nos envíes un correo electrónico a la siguiente dirección "
                        + "<a href='mailto:ventas@medicinasrosario.com.mx'>Soporte ExpoMR</a><br/><br/><br/>"
                        + avisoConfidencialidad;
               
#if (TESTMODE)
                await _emailSender.SendEmailAsync("pruebas@medicinasrosario.com.mx", "Recuperar Contraseña ExpoMR", body);
#else
                    
                  await _emailSender.SendEmailAsync(opViewModel.Email, "Recuperar Contraseña ExpoMR", body);
#endif

                return RedirectToAction("ConfirmacionOlvidoPassword");
            }

            return View(opViewModel);
        }
        public async Task<bool> SendPromos(int IdCliente)
        {
            string email = "";
            string body = string.Empty;
            //Log.SaveBusqueda(5535, 5108, "Paracetamol");
            //SqlConnection cnn = new SqlConnection(_configurationDB["ConnectionStrings:ExpoMRCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "SELECT dPromoCliente.IdPromocion, vPersonas.Nombre AS Cliente, " +
            //                    "CAST(dPromoCliente.Cantidad AS NVARCHAR(12)) + ' ' + mArticulos.Nombre AS Nombre, " +
            //                    "mArticulos.Descripcion, mUsuarios.UserName " +
            //                    "FROM dpromocliente INNER JOIN " +
            //                    "dpromociones ON dpromociones.idpromocion = dpromocliente.idpromocion INNER JOIN " +
            //                    "marticulos ON marticulos.Idarticulo = dPromociones.IdArticulo INNER JOIN " +
            //                    "vPersonas ON vPersonas.IdPersona = dPromoCliente.Cliente INNER JOIN " +
            //                    "mUsuarios ON vPersonas.IdPersona = mUsuarios.IdPersona " +
            //                    "WHERE dPromoCliente.Cliente =" + IdCliente;
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();
            string query = "SELECT dPromoCliente.IdPromocion, vPersonas.Nombre AS Cliente, " +
                                "CAST(dPromoCliente.Cantidad AS NVARCHAR(12)) + ' ' + mArticulos.Nombre AS Nombre, " +
                                "mArticulos.Descripcion, mUsuarios.UserName " +
                                "FROM dpromocliente INNER JOIN " +
                                "dpromociones ON dpromociones.idpromocion = dpromocliente.idpromocion INNER JOIN " +
                                "marticulos ON marticulos.Idarticulo = dPromociones.IdArticulo INNER JOIN " +
                                "vPersonas ON vPersonas.IdPersona = dPromoCliente.Cliente INNER JOIN " +
                                "mUsuarios ON vPersonas.IdPersona = mUsuarios.IdPersona " +
                                "WHERE dPromoCliente.Cliente =" + IdCliente;

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);
            foreach (DataRow row in dataTable.Rows)
            {
                if (string.IsNullOrEmpty(email))
                    email = row.Field<string>(4);

                if (string.IsNullOrEmpty(body))
                {
                    body = "<table width='100%'>" +
                                "<caption style='margin:10px; font-size:1.2em'><b>PROMOCIONES OBTENIDAS</b></caption>" +
                                "<tr><table border=2 style='width:100%'>" +
                                    "<tr><td colspan='2'><b>Cliente: </b>" + row.Field<string>(1) + "</td>" +
                                "</tr>" +
                                "<tr align='center'>" +
                                    "<td width='30%'><b>Nombre del Artículo</b></td>" +
                                    "<td><b>Descripción</b></td>" +
                                "</tr>";
                }

                body += "<tr><td>"
                    + row.Field<string>(2) + "</td><td>"
                    + row.Field<string>(3) + "</td></tr>";

            }

            body += "</table></tr><br/><br/><br/>"
                 + avisoConfidencialidad;

            try
            {
                
#if (TESTMODE)
                await _emailSender.SendEmailAsync("pruebas@medicinasrosario.com.mx", "Promociones Obtenidas", body);
#else
                    
                  await _emailSender.SendEmailAsync(email, "Recuperar Contraseña ExpoMR", body);
#endif
                return true;
            }catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<bool> SendOrder(int IdCliente, int IdPedido)
        {
            //var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            //string username = await _userManager.GetUserNameAsync(currentUser);
            //bool result = Log.SaveSesion(HttpContext.Request, username, true);
            
            decimal Total = 0;
            string email = "";
            string obsv = "";
            string body = string.Empty;
            //SqlConnection cnn = new SqlConnection(_configurationDB["ConnectionStrings:ExpoMRCS"]);
            //SqlCommand cmd = new SqlCommand();
            //DataTable dataTable = new DataTable();
            //SqlDataAdapter sqlDA;
            //cnn.Open();
            //cmd.CommandText = "SELECT vPedidoDatos.IdPedido, vPedidoDatos.Cliente, vPedidoDatos.RFC, vPedidoDatos.Documento, " +
            //                    "vPedidoDatos.TipoVenta, vPedidoDatos.DirEntrega, vPedidoDatos.Porte, vPedidoDatos.Transportista, " +
            //                    "vProductos.IdProducto, vProductos.Codigo, vProductos.Producto, vPedidos.Unitario, vPedidos.Descuento, vPedidos.Cantidad, " +
            //                    "(vPedidos.PrecioIva * vPedidos.Cantidad) AS Total, vPedidos.PrecioIva, CASE WHEN vProductos.Iva = 1 THEN  Indigo.dbo.GetIva(0) * 100 ELSE 0 END AS Iva, mUsuarios.UserName, " +
            //                    "vPedidoDatos.Obsv " +
            //                    "FROM vPedidoDatos INNER JOIN " +
            //                    "mUsuarios ON vPedidoDatos.IdPersona = mUsuarios.IdPersona INNER JOIN " +
            //                    "vPedidos ON vPedidoDatos.IdPedido = vPedidos.IdPedido INNER JOIN " +
            //                    "vProductos ON vProductos.IdProducto = vPedidos.IdProducto " +
            //                    "WHERE vPedidos.IdPedido = " + IdPedido + " AND vPedidoDatos.IdPersona =" + IdCliente;
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = cnn;
            //sqlDA = new SqlDataAdapter(cmd);
            //sqlDA.Fill(dataTable);
            //cnn.Close();
            string query = "SELECT vPedidoDatos.IdPedido, vPedidoDatos.Cliente, vPedidoDatos.RFC, vPedidoDatos.Documento, " +
                                "vPedidoDatos.TipoVenta, vPedidoDatos.DirEntrega, vPedidoDatos.Porte, vPedidoDatos.Transportista, " +
                                "vProductos.IdProducto, vProductos.Codigo, vProductos.Producto, vPedidos.Unitario, vPedidos.Descuento, vPedidos.Cantidad, " +
                                "(vPedidos.PrecioIva * vPedidos.Cantidad) AS Total, vPedidos.PrecioIva, CASE WHEN vProductos.Iva = 1 THEN  Indigo.dbo.GetIva(0) * 100 ELSE 0 END AS Iva, mUsuarios.UserName, " +
                                "vPedidoDatos.Obsv " +
                                "FROM vPedidoDatos INNER JOIN " +
                                "mUsuarios ON vPedidoDatos.IdPersona = mUsuarios.IdPersona INNER JOIN " +
                                "vPedidos ON vPedidoDatos.IdPedido = vPedidos.IdPedido INNER JOIN " +
                                "vProductos ON vProductos.IdProducto = vPedidos.IdProducto " +
                                "WHERE vPedidos.IdPedido = " + IdPedido + " AND vPedidoDatos.IdPersona =" + IdCliente;

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);
            foreach (DataRow row in dataTable.Rows)
            {
                if (string.IsNullOrEmpty(email))
                {
                    email = row.Field<string>(17);
                    obsv = row.Field<string>(18);
                }
                if (string.IsNullOrEmpty(body))
                {
                    body = "<table width='100%'>" +
                                "<caption style='margin:10px; font-size:1.2em'>" +
                                    "<b>ORDEN DE COMPRA</b>" +
                                "</caption>" +
                                "<tr>" +
                                    "<table border=2 width='100%'>" +
                                        "<tr><td width='70%'></td><td width='30%'><b>Orden: </b>" + row.Field<int>(0) + "</td></tr>" +
                                        "<tr><td colspan='2'><b>Cliente: </b>" + row.Field<string>(1) + "</td></tr>" +
                                        "<tr><td width='30%'><b>RFC: </b>" + row.Field<string>(2) + "</td>" +
                                            "<td width ='70%'><b>Tipo de Docto.: </b>" + row.Field<string>(3) + "</td></tr>" +
                                        "<tr><td colspan ='2'><b>Dirección de Entrega: </b>" + row.Field<string>(5) + "</td></tr>" +
                                    "</table>" +
                                "</tr></br>" +
                                "<tr>" +
                                    "<table border=2 style='width:100%'>" +
                                        "<tr align='center'>" +
                                            "<td width ='8%'><b>Código</b></td>" +
                                            "<td width ='40%'><b>Producto</b></td>" +
                                            "<td width ='8%'><b>Precio Unitario</b></td>" +
                                            "<td width='8%'><b>Descuento</b></td>" +
                                            "<td width='8%'><b>Cantidad</b></td>" +
                                            "<td width='8%'><b>IVA</b></td width='12%'>" +
                                            "<td width='8%'><b>Precio Final</b></td>" +
                                            "<td><b>Total</b></td>" +
                                        "</tr>";
                }
                try
                {
                    body += "<tr><td>" + row.Field<string>(9) + "</td><td>"
                                       + row.Field<string>(10) + "</td><td>"
                                       + row.Field<float>(11) + "</td><td>"
                                       + row.Field<decimal>(12) + "</td><td>"
                                       + row.Field<int>(13) + "</td><td>"
                                       + row.Field<float>(16) + "</td><td>"
                                       + row.Field<decimal>(15) + "</td><td>"
                                       + row.Field<decimal>(14) + "</td></tr>";
                   Total += row.Field<decimal>(14);
                }
                catch (Exception ex) {
                    string e = ex.Message;
                    return false;
                }
            }

            body += "<tr align='right'>" +
                        "<td colspan='6'></td><td><b>Total:</b></td>" +
                        "<td>" + Total + "</td>" +
                    "</tr>" +
                    "</table></tr></br>" +
                    "<tr>" +
                        "<table width='100%'>" +
                            "<tr><td colspan ='2'><b>Observaciones: </b>" + obsv + "</td></tr>" +
                        "</table>" +
                    "</tr>" +
                    "<tr>" +
                        "<table width='100%'>" +
                            "<tr align='right'><td><i><b>Orden de compra sujeta a existencias.Consulta tu agente de ventas.</b></i></td></tr></table>" +
                    "</tr>" +
                 "</table>"
                 + avisoConfidencialidad;           
            try
            {
#if (TESTMODE)
                await _emailSender.SendEmailAsync("pruebas@medicinasrosario.com.mx", "Pedido en firme", body);
#else
                    
                await _emailSender.SendEmailAsync(email, "Pedido en firme", body);
#endif
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
