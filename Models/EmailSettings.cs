namespace OTPManager.Models
{
    public class EmailSettings
    {
        public string? From { get; set; }
        public string? SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }

        public bool EnableSsl { get; set; }


        // Add other settings as needed, such as SSL/TLS options
    }
}
