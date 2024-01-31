namespace OTPManager.Services.Interfaces
{
    public interface IOTPService
    {
        public string GenerateTotp(string secretKey);

        public bool ValidateTotp(string providedOtp, string secretKey);
    }
}
