namespace OneTimeCodeApi.Services
{
    public interface IVerificationService
    {
        bool ValidateUser(string userName, string? phoneNumber);
        string GenerateAndSendCode(string? phoneNumber);
    }
}
