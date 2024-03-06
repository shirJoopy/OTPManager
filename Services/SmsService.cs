using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection;
using OTPManager.Services.Interfaces;
using Vonage.Common;
using Vonage.Messaging;
using Vonage.Request;
using Vonage;
using Microsoft.Extensions.Options;
using OTPManager.Models;

namespace OTPManager.Services
{
    public class SmsService : ISmsService
    {


        private readonly SmsSettings _smsSettings;
        private readonly ILogger<AuditTrailMiddleware> _logger;

        public SmsService(IOptions<SmsSettings> smsSettings,ILogger<AuditTrailMiddleware> logger)
        {
            _smsSettings = smsSettings.Value;
            _logger = logger;
        }


        public async void SendSmsAsync(string phoneNumber, string message)
        {
            var credentials = Credentials.FromApiKeyAndSecret(_smsSettings.ApiKey, _smsSettings.ApiSecret);
            var client = new VonageClient(credentials);

            try
            {
                await client.SmsClient.SendAnSmsAsync(new Vonage.Messaging.SendSmsRequest()
                {
                    To = phoneNumber,
                    From = _smsSettings.From,
                    Text = message
                });
            } catch (Exception ex)
            {
                _logger.LogError(ex,$"Failed seding sms to {phoneNumber}");
            }
           

            
        }
    }
}
