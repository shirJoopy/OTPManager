using Microsoft.Extensions.Options;
using OTPManager.Models;

namespace OTPManager.Services.Interfaces
{
    public interface IEMailService
    {
        public Task SendEmailAsync(string toAddress, string subject, string body);
    }
}
