
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AJESeForm.Models
{
    public class EmailManager
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        
        public string ReceiverAddress { get; set; }
        public string ReceiverDisplayName { get; set; }
       
     

        public Task<string> SendEmail(string SenderEmailAddress= "eform.admin@ajes.ae", string SenderName= "AJES Service Center")
        {
            System.Net.Mail.MailMessage newemail = new System.Net.Mail.MailMessage();
            System.Net.Mail.MailAddress MailReceiver = new System.Net.Mail.MailAddress(ReceiverAddress, ReceiverDisplayName);
            System.Net.Mail.MailAddress MailSender = new System.Net.Mail.MailAddress(SenderEmailAddress, SenderName);
            newemail.From = MailSender;
            newemail.To.Add(MailReceiver);
            newemail.IsBodyHtml = true;
            newemail.Subject = Subject;
            newemail.Body = Body;
            return SendMail(newemail);
        }

        private async Task<string> SendMail(System.Net.Mail.MailMessage MailMsg)
        {
            System.Net.Mail.SmtpClient newe = new System.Net.Mail.SmtpClient();
            newe.Host = System.Configuration.ConfigurationManager.AppSettings.Get("SMTPHost");
            newe.Port = 25;
            //newe.Credentials = new System.Net.NetworkCredential("eform", "Al");
            newe.UseDefaultCredentials = false;
            newe.EnableSsl = false;
            try
            {
                await newe.SendMailAsync(MailMsg);
            }
            catch (Exception e)
            {
                return e.Message;
                newe.Dispose();
            }
            newe.Dispose();
            return "";

        }


        public string GetBody(string TemplatePath)
        {
            StreamReader sr;
            sr = new StreamReader(TemplatePath);
            string _EmailBody = sr.ReadToEnd();
            sr.Dispose();
            return _EmailBody;
        }
    }
}