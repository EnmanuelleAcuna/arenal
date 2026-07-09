using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace plani.Identity;

public class EmailSender : IEmailSender {
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger) {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage) {
        // Configuración SMTP: valores no secretos en appsettings.json (Smtp:Host/Port/From);
        // credenciales (Smtp:User/Smtp:Password) en user-secrets (dev) o variables de entorno (prod).
        string host = _configuration["Smtp:Host"];
        int port = int.TryParse(_configuration["Smtp:Port"], out int parsedPort) ? parsedPort : 587;
        string user = _configuration["Smtp:User"];
        string password = _configuration["Smtp:Password"];
        string from = _configuration["Smtp:From"] ?? user;

        try {
            SmtpClient smtpClient = new(host, port: port) {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(userName: user, password: password)
            };

            await smtpClient.SendMailAsync(
                new MailMessage(from: from, to: email, subject: subject, body: htmlMessage) {
                    IsBodyHtml = true
                }
            );
        }
        catch (Exception e) {
            _logger.LogError(exception: e, "Error al enviar correo a {Email}", email);
            throw;
        }
    }
}
