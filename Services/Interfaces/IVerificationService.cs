namespace OTPManager.Services.Interfaces
{
    public interface IVerificationService
    {
        string? GetUserSecret(string userName, string phoneNumber);
        void CleanSecrets(string userNamer);
    }
}
