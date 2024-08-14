namespace OTPManager.Models
{
    public class VonageSettings
    {
        public string? From { get; set; }
        public string? ApiKey { get; set; }
        public string? ApiSecret { get; set; }

        

        // Add other settings as needed, such as SSL/TLS options
    }

    public class CellactSettings
    {
        public string? User { get; set; }
        public string? Password { get; set; }
        public string? From { get; set; }
        public string? ServiceName { get; set; }
        public string? ApiUrl { get; set; }
        
    }


    public class SmsProviders
    {
        public string Use { get; set; }
        public VonageSettings Vonage { get; set; }
        public CellactSettings Cellact { get; set; }
    }

    public class SmsSettings
    {
        public string SmsProvider { get; set; }
        public SmsProviders SmsProviders { get; set; }
    }

}
