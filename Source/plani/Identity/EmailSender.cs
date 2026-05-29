using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace plani.Identity;

public class EmailSender : IEmailSender {
    public async Task SendEmailAsync(string email, string subject, string htmlMessage) {
        try {
            SmtpClient smtpClient = new("smtp.gmail.com", port: 587) {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("sandiconsultores@gmail.com", "zann yqbc cvve nyhn")
            };

            await smtpClient.SendMailAsync(
                new MailMessage("sandiconsultores@gmail.com", to: email, subject: subject, body: htmlMessage) {
                    IsBodyHtml = true
                }
            );
        }
        catch (Exception e) {
            Console.WriteLine(value: e);
            throw;
        }
    }
}