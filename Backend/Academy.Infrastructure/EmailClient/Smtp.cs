using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Models;
using System.Net;
using System.Net.Mail;

namespace Academy.Infrastructure.EmailClient
{
    public class Smtp<T> : ISmtp<T> where T : ISmtpSettings
    {
        private SmtpClient smtpClient = new();
        private readonly ISmtpSettings _smtpSettings;
        public Smtp(ISmtpSettings smtpSettings)
        {
            _smtpSettings = smtpSettings;
        }

        public async Task SendEmailAsync(string commaSeperatedTo, string subject, string bodyText, string commaSeperatedCc = "", string commSeperatedBcc = "")
        {
            if (string.IsNullOrEmpty(commaSeperatedTo) && string.IsNullOrEmpty(commaSeperatedCc) && string.IsNullOrEmpty(commSeperatedBcc))
                throw new ArgumentNullException($"Reciepents are empty");
            using (MailMessage mailMessage = new MailMessage())
            {
                mailMessage.From = new MailAddress(_smtpSettings.SenderEmail, _smtpSettings.SenderName);
                mailMessage.Body = bodyText;
                mailMessage.IsBodyHtml = true;
                mailMessage.Subject = subject;

                if (!string.IsNullOrWhiteSpace(commaSeperatedTo))
                {
                    foreach (string to in commaSeperatedTo.Split(','))
                    {
                        mailMessage.To.Add(to);
                    }
                }
                if (!string.IsNullOrWhiteSpace(commaSeperatedCc))
                {
                    foreach (string cc in commaSeperatedCc.Split(','))
                    {
                        mailMessage.CC.Add(cc);
                    }
                }
                if (!string.IsNullOrWhiteSpace(commSeperatedBcc))
                {
                    foreach (string bcc in commSeperatedBcc.Split(','))
                    {
                        mailMessage.To.Add(bcc);
                    }
                }
                mailMessage.Sender = new MailAddress(_smtpSettings.SenderEmail, _smtpSettings.SenderName ?? _smtpSettings.SenderEmail);
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    smtpClient.EnableSsl = _smtpSettings.EnableSsl;
                    smtpClient.Host = _smtpSettings.Host;
                    smtpClient.Port = _smtpSettings.Port;
                    smtpClient.UseDefaultCredentials = _smtpSettings.UseDefaultCredentials;
                    if (!_smtpSettings.UseDefaultCredentials)
                    {
                        smtpClient.Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password);
                    }
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
        }
    }
}