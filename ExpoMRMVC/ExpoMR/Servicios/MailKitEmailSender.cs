using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;

namespace ExpoMR.Servicios
{
    public class MailKitEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        public OpcionesMailKit _opcionesMailKit;

        public MailKitEmailSender()
        {
        }

        public MailKitEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                _opcionesMailKit = _configuration.GetSection("MailKit").Get<OpcionesMailKit>();

                var emailSend = new MimeMessage();
                emailSend.From.Add(MailboxAddress.Parse(_opcionesMailKit.SenderEmail));
                emailSend.To.Add(MailboxAddress.Parse(email));
                emailSend.Subject = subject;
                emailSend.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                smtp.Connect(_opcionesMailKit.Server, _opcionesMailKit.Port, SecureSocketOptions.StartTls);
                smtp.Authenticate(_opcionesMailKit.UserName, _opcionesMailKit.Password);
                smtp.Send(emailSend);
                smtp.Disconnect(true);
            }
            catch (Exception e)
            {
                string message = e.Message;
            }
        }
    }
}
