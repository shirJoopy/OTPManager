using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection;
using OTPManager.Services.Interfaces;
using Vonage.Common;
using Vonage.Messaging;
using Vonage.Request;
using Vonage;
using Microsoft.Extensions.Options;
using OTPManager.Models;
using System.Text;

namespace OTPManager.Services.Sms
{
    public class CellactSmsService : ISmsService
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly CellactSettings _smsSettings;
        private readonly ILogger<AuditTrailMiddleware> _logger;

        public CellactSmsService(IOptions<CellactSettings> smsSettings, ILogger<AuditTrailMiddleware> logger)
        {
            _smsSettings = smsSettings.Value;
            _logger = logger;


        }

        private string GetXmlPayload(string phoneNumber, string message)
        {
            string xmlPayload = $@"
                <PALO>
                    <HEAD>
                        <FROM>{_smsSettings.User}</FROM>
                        <APP USER=""{_smsSettings.User}"" PASSWORD=""{_smsSettings.Password}"">LA</APP>
                        <CMD>sendtextmt</CMD>
                    </HEAD>
                    <BODY>
                        <SENDER>{_smsSettings.From}</SENDER>
                        <CONTENT><![CDATA[Your text message here]]></CONTENT>
                        <DEST_LIST>
                            <TO>{phoneNumber}</TO>
                        </DEST_LIST>
                    </BODY>
                    <OPTIONAL>
                        <SERVICE_NAME>{message}</SERVICE_NAME>
                    </OPTIONAL>
                </PALO>";

            return xmlPayload;
        }

        public async void SendSmsAsync(string phoneNumber, string message)
        {

            try
            {
                string xmlPayload = GetXmlPayload(phoneNumber, message);

                var content = new StringContent(xmlPayload, Encoding.UTF8, "application/xml");
                var response = await client.PostAsync(_smsSettings.ApiUrl, content);
                string responseContent = await response.Content.ReadAsStringAsync();


                _logger.Log(LogLevel.Information, $"Sms Was Sent from : {_smsSettings.From}, to {phoneNumber}, with the the text ::\n '{message}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed sending sms to {phoneNumber}");
            }



        }
    }
}
