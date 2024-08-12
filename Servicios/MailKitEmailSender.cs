
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System;

namespace ExpoMR.Servicios
{
    public class MailKitEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public OpcionesMailKit _opcionesMailKit;

        public MailKitEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /* *
         * Envio estadar de Correo
         * */
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                _opcionesMailKit = _configuration.GetSection("MailKit").Get<OpcionesMailKit>();

                var emailSend = new MimeMessage();
                emailSend.From.Add(MailboxAddress.Parse(_opcionesMailKit.SenderEmail));

                if(_opcionesMailKit.Pruebas == 1)
                    emailSend.To.Add(MailboxAddress.Parse(_opcionesMailKit.Tosend));
                else
                    emailSend.To.Add(MailboxAddress.Parse(email));

                emailSend.Subject = subject;
                emailSend.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

                using var smtp = new SmtpClient();
                smtp.Connect(_opcionesMailKit.Server, _opcionesMailKit.Port, SecureSocketOptions.StartTls);
                smtp.Authenticate(_opcionesMailKit.UserName, _opcionesMailKit.Password);
                smtp.Send(emailSend);
                smtp.Disconnect(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return Task.CompletedTask;
        }
    }
}
