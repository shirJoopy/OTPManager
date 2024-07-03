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

        private Credentials credentials;
        private VonageClient client;
        private readonly SmsSettings _smsSettings;
        private readonly ILogger<AuditTrailMiddleware> _logger;

        public SmsService(IOptions<SmsSettings> smsSettings,ILogger<AuditTrailMiddleware> logger)
        {
            _smsSettings = smsSettings.Value;
            _logger = logger;
            this.credentials = Credentials.FromApiKeyAndSecret(_smsSettings.ApiKey, _smsSettings.ApiSecret);
            this.client = new VonageClient(this.credentials);
        }


        public async void SendSmsAsync(string phoneNumber, string message)
        {

            try
            {
                var response = await client.SmsClient.SendAnSmsAsync(new Vonage.Messaging.SendSmsRequest()
                {
                    To = phoneNumber,
                    From = _smsSettings.From,
                    Text = message
                });
                _logger.Log(LogLevel.Information, $"Sms Was Sent from : {_smsSettings.From}, to {phoneNumber}, with the the text ::\n '{message}'");
            } catch (Exception ex)
            {
                _logger.LogError(ex,$"Failed seding sms to {phoneNumber}");
            }
           

            
        }
    }
}
