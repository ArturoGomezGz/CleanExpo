
#define TESTMODE
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;

namespace ExpoMR.Clases
{
    public class Notifications
    {
        private readonly IEmailSender _emailSender;
        public Notifications(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        private string pMessage = "";
        private string avisoConfidencialidad = "AVISO DE CONFIDENCIALIDAD: Este mensaje es confidencial y/o puede contener información privada.<br/>"
                                            + "Si usted no es el destinatario o no es alguna persona autorizada por éste para recibir el mensaje,<br/>"
                                            + "usted no deberá utilizar, copiar, revelar o tomar ninguna acción basada en este mensaje.<br/>"
                                            + "Si recibe este mensaje por error, por favor notifíquelo de inmediato al remitente y bórrelo de su computadora.<br/>";
        public string Message
        {
            get { return pMessage; }
        }


        public async Task<string> SendWelcome(string name, string email, string password, bool vip)
        {
            
            string linkExpo = vip ? "https://expomr.online/Presencial.aspx" : "https://expomr.online";
            string body = "<br/>Hola<br/><br/>"
                    + "Medicinas Rosario te da la bienvenida a su Expo Virtual 2022<br/><br/>"
                    + "Ingresa en <a href='" + linkExpo + "'>" + linkExpo + "</a><br/><br/>"
                    + "Esta es la información de tu cuenta y solo podrás utilizarla los días que dure el evento:<br/><br/>"
                    + "Usuario: " + email + "<br/>"
                    + "Contraseña:  " + password + "<br/>"
                    + "(Esta es una contraseña generada automáticamente, te sugerimos cambiarla la primera vez que ingreses)<br/><br/><br/><br/>"
                    + "Si tienes alguna duda o comentario, te solicitamos nos envíes un correo electrónico a la siguiente dirección "
                    + "<a href='mailto:'>Soporte ExpoMR</a><br/><br/><br/>" + avisoConfidencialidad;
            try {
#if (TESTMODE)
                await _emailSender.SendEmailAsync("pruebas@medicinasrosario.com.mx", "Asunto", body);
#else                    
                await _emailSender.SendEmailAsync(email, "Asunto", body);
#endif


                pMessage = "mensaje enviado";
            }
            catch (Exception e)
            {
                pMessage = e.Message;
            }       
            return pMessage;
        }        
    }
}
