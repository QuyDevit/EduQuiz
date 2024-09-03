using EduQuiz.Models;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace EduQuiz.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }
        public void SendEmail(string recipientEmail, string subject, string message)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName);
                mail.To.Add(recipientEmail);
                mail.Subject = subject;
                mail.Body = message;
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient(_emailSettings.SMTPHost, _emailSettings.SMTPPort))
                {
                    smtp.Credentials = new NetworkCredential(_emailSettings.SMTPUser, _emailSettings.SMTPPassword);
                    smtp.EnableSsl = _emailSettings.EnableSSL;
                    smtp.Send(mail);
                }
            }
        }
    }
}
