namespace RealEstateApp.Domain.Settings
{
    public class MailSettings
    {
        public required string EmailFrom { get; set; }
        public required string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public required string SmtpUser { get; set; }
        public required string SmtpPassword { get; set; }
        public required string DisplayName { get; set; }
    }
}
