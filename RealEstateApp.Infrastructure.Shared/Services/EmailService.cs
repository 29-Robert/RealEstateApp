using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using RealEstateApp.Application.DTOs.Email;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Domain.Settings;

namespace RealEstateApp.Infrastructure.Shared.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings MailSettings;
        private readonly ILogger<MailSettings> Logger;

        public EmailService(IOptions<MailSettings> mailSettings, ILogger<MailSettings> logger)
        {
            MailSettings = mailSettings.Value;
            Logger = logger;
        }

        public async Task SendAsync(EmailRequestDto emailRequestDto)
        {
            try
            {
                if (emailRequestDto != null && !string.IsNullOrWhiteSpace(emailRequestDto.To))
                {
                    MimeMessage email = new()
                    {
                        Sender = MailboxAddress.Parse(MailSettings.EmailFrom),
                        Subject = emailRequestDto.Subject
                    };

                    email.To.Add(MailboxAddress.Parse(emailRequestDto.To));

                    BodyBuilder builder = new()
                    {
                        HtmlBody = emailRequestDto.HtmlBody
                    };

                    email.Body = builder.ToMessageBody();

                    using SmtpClient smtpClient = new();

                    // Mitigación de errores de revocación y certificados autofirmados locales
                    smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    smtpClient.CheckCertificateRevocation = false;

                    await smtpClient.ConnectAsync(MailSettings.SmtpHost, MailSettings.SmtpPort, SecureSocketOptions.StartTls);
                    await smtpClient.AuthenticateAsync(MailSettings.SmtpUser, MailSettings.SmtpPassword);
                    await smtpClient.SendAsync(email);
                    await smtpClient.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
            }
        }
    }
}
