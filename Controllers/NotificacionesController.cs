#define TESTMODE

using ExpoMR.Clases;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Nancy.Json;
using System.Data;
using System.Runtime.Intrinsics.X86;
using ExpoMR.Datos;

namespace ExpoMR.Controllers
{
    [Authorize]
    public class NotificacionesController : Controller
    {
        private IConfiguration _configurationDB;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        //Variables para verificar La sesión Y el Usuario
        public NotificacionesController(UserManager<IdentityUser> userManager,IEmailSender emailSender, IConfiguration configurationDB)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _configurationDB = configurationDB;
        }

        private string pMessage = "";
        private string avisoConfidencialidad = "AVISO DE CONFIDENCIALIDAD: Este mensaje es confidencial y/o puede contener información privada.<br/>"
                                            + "Si usted no es el destinatario o no es alguna persona autorizada por éste para recibir el mensaje,<br/>"
                                            + "usted no deberá utilizar, copiar, revelar o tomar ninguna acción basada en este mensaje.<br/>"
                                            + "Si recibe este mensaje por error, por favor notifíquelo de inmediato al remitente y bórrelo de su computadora.<br/>";
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> OlvidoPassword(OlvidoPasswordViewModel opViewModel)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _userManager.FindByEmailAsync(opViewModel.Email);

                if (usuario == null)
                    return RedirectToAction(nameof(CuentasController.ConfirmacionOlvidoPassword), "Cuentas");

                var codigo = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                var urlRetorno = Url.Action("ResetPassword", "Cuentas", new { userId = usuario.Id, code = codigo }, protocol: HttpContext.Request.Scheme);

                string body = "Restablece tu Contraseña<br/><br/><br/>"
                        + "Por favor restablece tu contraseña haciendo click"
                        + "<a href=\"" + urlRetorno + "\"> Aquí </a><br/><br/><br/>"
                        + "Si tienes alguna duda o comentario, te solicitamos nos envíes un correo electrónico a la siguiente dirección "
                        + "<a href='mailto:ventas@medicinasrosario.com.mx'>Soporte ExpoMR</a><br/><br/><br/>"
                        + avisoConfidencialidad;
               
            #if (TESTMODE)
                await _emailSender.SendEmailAsync("carlos.rivera@medicinasrosario.com.mx", "Recuperar Contraseña ExpoMR", body);
            #else                    
                await _emailSender.SendEmailAsync(opViewModel.Email, "Recuperar Contraseña ExpoMR", body);
            #endif

                return RedirectToAction(nameof(CuentasController.ConfirmacionOlvidoPassword), "Cuentas");
            }

            return View(opViewModel);
        }

        [HttpPost]
        public async Task<JsonResult> SendWelcome(int IdEmail, string UserName, bool Vip)
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
                            await _emailSender.SendEmailAsync(UserName, "Bienvenido a Expo Virtual Medicinas Rosario", body);
                        #endif

                        WebSite.InsertFeNotificacion(IdEmail);
                        WebSite.InsertIinvitacion(IdEmail, Vip, "SUCCESS");

                        rm.Response = true;
                        rm.Message = "Se envio la invitacion correctamente";
                    }
                    else 
                    {
                        rm.Response = false;
                        rm.Message = "No se pudo Generar la contraseña para enviarla en la invitación";
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

        public async Task<JsonResult> SendPromos(int IdCliente)
        {
            var rm = new ResponseModel();

            string email = "";
            string body = string.Empty;

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdPersona", IdCliente);

            SqlDataReader dr = dB.ExecuteReader("GetDetailNotificacion", ref parameters);

            while (dr.Read())
            {
                if (string.IsNullOrEmpty(email))
                    email = (string)dr["Email"];

                if (string.IsNullOrEmpty(body))
                {
                    body = "<table width='100%'>" +
                                "<caption style='margin:10px; font-size:1.2em'><b>PROMOCIONES OBTENIDAS</b></caption>" +
                                "<tr><table border=2 style='width:100%'>" +
                                    "<tr><td colspan='2'><b>Cliente: </b>" + (string)dr["Cliente"] + "</td>" +
                                "</tr>" +
                                "<tr align='center'>" +
                                    "<td width='30%'><b>Nombre del Artículo</b></td>" +
                                    "<td><b>Descripción</b></td>" +
                                "</tr>";
                }

                body += "<tr><td>"
                    + (string)dr["Articulo"] + "</td><td>"
                    + (string)dr["Descripcion"] + "</td></tr>";
            }

            body += "</table></tr><br/><br/><br/>"
                    + avisoConfidencialidad;
            try
            {
                #if (TESTMODE)
                    await _emailSender.SendEmailAsync("carlos.rivera@medicinasrosario.com.mx", "Promociones Obtenidas", body);
                #else
                    await _emailSender.SendEmailAsync(email, "Promociones Obtenidas", body);
                #endif
            }
            catch (Exception ex)
            {
                Log.LogNotificacion(IdCliente, 1); //1 = Notificacion de promocion

                rm.Response = false;
                rm.Message = ex.Message;
            }

            Log.LogNotificacion(IdCliente, 1); //1 = Notificacion de promocion

            rm.Response = true;
            rm.Message = "Se envio la notificacion correctamente al email: " + email;

            return Json(rm);
        }
        
        public async Task<bool> SendOrder(int IdPedido)
        {
            decimal Total = 0;
            string email = "";
            string obsv = "";
            string body = string.Empty;

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@IdPersona", HttpContext.Session.GetString("ClienteSel"));
            parameters[1] = new SqlParameter("@IdPedido", IdPedido);

            SqlDataReader dr = dB.ExecuteReader("GetDetailOrder", ref parameters);

            while (dr.Read())
            {
                if (string.IsNullOrEmpty(email))
                {
                    email = (string)dr["Email"];
                    obsv = (string)dr["Obsv"];
                }

                if (string.IsNullOrEmpty(body))
                {
                    body = "<table width='100%'>" +
                                "<caption style='margin:10px; font-size:1.2em'>" +
                                    "<b>ORDEN DE COMPRA</b>" +
                                "</caption>" +
                                "<tr>" +
                                    "<table border=2 width='100%'>" +
                                        "<tr><td width='70%'></td><td width='30%'><b>Orden: </b>" + dr["IdPedido"].ToString() + "</td></tr>" +
                                        "<tr><td colspan='2'><b>Cliente: </b>" + dr["Cliente"].ToString() + "</td></tr>" +
                                        "<tr><td width='30%'><b>RFC: </b>" + dr["RFC"].ToString() + "</td>" +
                                            "<td width ='70%'><b>Tipo de Docto.: </b>" + dr["Documento"].ToString() + "</td></tr>" +
                                        "<tr><td colspan ='2'><b>Dirección de Entrega: </b>" + dr["DirEntrega"].ToString() + "</td></tr>" +
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
                    body += "<tr><td>" + dr["Codigo"].ToString() + "</td><td>"
                                       + dr["Producto"].ToString() + "</td><td>"
                                       + dr["Unitario"].ToString() + "</td><td>"
                                       + dr["Descuento"].ToString() + "</td><td>"
                                       + dr["Cantidad"].ToString() + "</td><td>"
                                       + dr["Iva"].ToString() + "</td><td>"
                                       + dr["PrecioIva"].ToString() + "</td><td>"
                                       + dr["Total"].ToString() + "</td></tr>";
                    Total += (decimal)dr["Total"];
                }
                catch (Exception ex)
                {
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
                    await _emailSender.SendEmailAsync("carlos.rivera@medicinasrosario.com.mx", "Pedido en firme", body);
                #else                   
                    await _emailSender.SendEmailAsync(email, "Pedido en firme", body);
                #endif
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return false;
            }
        }        
    }
}