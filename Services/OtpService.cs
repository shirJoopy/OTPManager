using OTPManager.Services.Interfaces;
using OtpNet;
using System.Text;

namespace OTPManager.Services
{
    public class OtpService : IOTPService
    {
        public string GenerateTotp(string key)
        {
            byte[] secretKey = Encoding.Unicode.GetBytes(key);
            Totp totp = new Totp(secretKey, step: 120);
            return totp.ComputeTotp();
        }

        public bool ValidateTotp(string providedOtp, string key)
        {
            byte[] secretKey = Encoding.Unicode.GetBytes(key);
            var totp = new Totp(secretKey, step: 120);
            return totp.VerifyTotp(providedOtp, out _, new VerificationWindow(1, 1));
        }
    }

}