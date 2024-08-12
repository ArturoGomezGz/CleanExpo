using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace ExpoMR.Clases
{
    public class Mail
    {
        #region "Enums"
        public enum SmtpProfiles
        {
            ExpoMR
        }

        public enum AddresseeType
        {
            To,
            CC,
            BCC,
            Reply,
        }
        #endregion

        #region "Variables"
        //private const string Company = "Medicinas Rosario - ";

        //private readonly SmtpClient cSmtp = new();
        //private SmtpProfiles cSmtpProfile;
        //private readonly MailAddress cFrom;

        //private readonly MailAddressCollection pTo = new();
        //private readonly MailAddressCollection pCC = new();
        //private readonly MailAddressCollection pBCC = new();
        //private readonly MailAddressCollection pReply = new();
        private string pSubject;
        private string pBody;
        private bool pIsBodyHtml = false;
        private MailPriority pPriority = MailPriority.Normal;
        #endregion

        #region "Properties"
        public string Subject
        {
            get { return pSubject; }

            set { pSubject = value; }
        }

        public string Body
        {
            get { return pBody; }

            set { pBody = value; }
        }

        public bool IsBodyHtml
        {
            get { return pIsBodyHtml; }

            set { pIsBodyHtml = value; }
        }

        public MailPriority Priority
        {
            get { return pPriority; }

            set { pPriority = value; }
        }

        public string ErrorMessage { get; }

        //public SmtpProfiles From
        //{
        //    get { return cSmtpProfile; }

        //    set
        //    {
        //        cSmtpProfile = value;

        //        //SmtpSection smtpSection;
        //        //smtpSection = (SmtpSection)ConfigurationManager.GetSection("mailSettings/" + cSmtpProfile.ToString());

        //        cSmtp.Host = smtpSection.Network.Host;
        //        cSmtp.Port = smtpSection.Network.Port;
        //        cSmtp.UseDefaultCredentials = smtpSection.Network.DefaultCredentials;
        //        cSmtp.Credentials = new NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);
        //        cSmtp.EnableSsl = smtpSection.Network.EnableSsl;
        //        cSmtp.DeliveryMethod = smtpSection.DeliveryMethod;
        //        cFrom = new MailAddress(smtpSection.From, Company + cSmtpProfile.ToString());
        //    }
        //}
        #endregion

        //        public Mail()
        //        { }

        //        public Mail(SmtpProfiles from, string to = "", string toName = "", string subject = "", string body = "")
        //        {
        //            this.From = from;
        //            if (to != "") pTo.Add(new MailAddress(to, toName));
        //            pSubject = subject;
        //            pBody = body;
        //        }

        //        public void AddMailAddress(AddresseeType addresseType, string mail, string displayName = "")
        //        {
        //            switch (addresseType)
        //            {
        //                case AddresseeType.To:
        //                    pTo.Add(new MailAddress(mail, displayName));
        //                    break;

        //                case AddresseeType.CC:
        //                    pCC.Add(new MailAddress(mail, displayName));
        //                    break;

        //                case AddresseeType.BCC:
        //                    pBCC.Add(new MailAddress(mail, displayName));
        //                    break;

        //                case AddresseeType.Reply:
        //                    pReply.Add(new MailAddress(mail, displayName));
        //                    break;

        //                default:
        //                    break;
        //            }
        //        }

        //        public void AddAttachment(string fileName)
        //        {
        //            pAttachments.Add(new Attachment(fileName));
        //        }

        //        public void AddAttachment(string name, byte[] data)
        //        {
        //            pAttachments.Add(new Attachment(new System.IO.MemoryStream(data), name));
        //        }

        //        public bool Send()
        //        {
        //            try
        //            {
        //                cSmtp.Send(SetMail());
        //            }
        //            catch (SmtpException smtpEx)
        //            {
        //                pError = "Hubo un problema al enviar el correo:" + smtpEx.Message.Replace("'", "\'");
        //                return false;
        //            }
        //            catch (Exception ex)
        //            {
        //                pError = "Hubo un problema general: " + ex.Message.Replace("'", "\'");
        //                return false;
        //            }

        //            pError = "";
        //            return true;
        //        }

        //        public void Clear()
        //        {
        //            cFrom = null;
        //            pTo.Clear();
        //            pCC.Clear();
        //            pBCC.Clear();
        //            pReply.Clear();
        //            pSubject = "";
        //            pBody = "";
        //            pIsBodyHtml = false;
        //            pPriority = MailPriority.Normal;
        //            pAttachments.Clear();
        //            pError = "";
        //        }

        //        private MailMessage SetMail()
        //        {
        //            MailMessage mailMessage = new MailMessage();

        //#if (TESTMODE)
        //            string trace = "";

        //            mailMessage.To.Add("pruebas@medicinasrosario.com.mx");
        //            trace = "To: ";
        //            foreach (MailAddress mailAddress in pTo)
        //            {
        //                trace = trace + mailAddress.Address + ", ";
        //            }
        //            trace = trace + "\r\n" + "CC: ";
        //            foreach (MailAddress mailAddress in pCC)
        //            {
        //                trace = trace + mailAddress.Address + ", ";
        //            }
        //            trace = trace + "\r\n" + "BCC: ";
        //            foreach (MailAddress mailAddress in pBCC)
        //            {
        //                trace = trace + mailAddress.Address + ", ";
        //            }
        //            pBody = trace + "\r\n" + pBody;
        //#else
        //        foreach (MailAddress mailAddress in pTo)
        //        {
        //            mailMessage.To.Add(mailAddress);
        //        }
        //        foreach (MailAddress mailAddress in pCC)
        //        {
        //            mailMessage.CC.Add(mailAddress);
        //        }
        //        foreach (MailAddress mailAddress in pBCC)
        //        {
        //            mailMessage.Bcc.Add(mailAddress);
        //        }
        //#endif
        //            foreach (MailAddress mailAddress in pReply)
        //            {
        //                mailMessage.ReplyToList.Add(mailAddress);
        //            }
        //            mailMessage.From = cFrom;
        //            mailMessage.Subject = pSubject;
        //            mailMessage.IsBodyHtml = pIsBodyHtml;
        //            mailMessage.Body = pBody;
        //            mailMessage.Priority = pPriority;
        //            foreach (Attachment attachment in pAttachments)
        //            {
        //                mailMessage.Attachments.Add(attachment);
        //            }

        //            return mailMessage;
        //        }
    }
}
