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
            Totp totp = new Totp(secretKey);
            return totp.ComputeTotp();
        }

        public bool ValidateTotp(string providedOtp, string key)
        {
            byte[] secretKey = Encoding.Unicode.GetBytes(key);
            var totp = new Totp(secretKey);
            return totp.VerifyTotp(providedOtp, out _, new VerificationWindow(1, 1));
        }

        // You may want methods for generating and storing secret keys here as well.
    }
}