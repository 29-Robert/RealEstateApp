using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Domain.Settings;
using System;
using System.Threading.Tasks;

namespace RealEstateApp.Infrastructure.Shared.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            try
            {
                // 1. Crear el mensaje
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_mailSettings.DisplayName ?? "", _mailSettings.EmailFrom));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;

                // 2. Construir el cuerpo en HTML
                var builder = new BodyBuilder { HtmlBody = body };
                email.Body = builder.ToMessageBody();

                // 3. Conectar y enviar usando MailKit
                using var smtp = new MailKit.Net.Smtp.SmtpClient();

                // Mitigación de errores de revocación y certificados locales
                smtp.CheckCertificateRevocation = false;
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                // Conectar usando Auto para soportar dinámicamente 465 y 587
                await smtp.ConnectAsync(_mailSettings.SmtpHost, _mailSettings.SmtpPort, SecureSocketOptions.Auto);

                // Autenticar con tu correo real y la contraseña de aplicación
                await smtp.AuthenticateAsync(_mailSettings.SmtpUser, _mailSettings.SmtpPass);

                // Enviar el correo
                await smtp.SendAsync(email);

                // Desconectar limpiamente
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Registrar el error en el log de la aplicación
                _logger.LogError(ex, "Error al enviar correo electrónico a {To}. Asunto: {Subject}", to, subject);

                // Lanzar para ver en Visual Studio
                throw new Exception($"Error SMTP con MailKit: {ex.Message}", ex);
            }
        }
    }
}
