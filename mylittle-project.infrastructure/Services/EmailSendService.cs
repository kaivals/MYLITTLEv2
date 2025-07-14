using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace mylittle_project.Infrastructure.Services
{
    public class EmailSendService : IEmailSender
    {
        private readonly string _fromEmail = "jaintisha880@gmail.com"; // your Gmail
        private readonly string _appPassword = "hocb cjyx hoam bwql"; // your app password form app passwords after 2fa enabling

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(_fromEmail, _appPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            return client.SendMailAsync(mailMessage);
        }
    }
}
