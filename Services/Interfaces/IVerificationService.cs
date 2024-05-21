namespace OTPManager.Services.Interfaces
{
    
    public interface IVerificationService
    {

        bool CheckDbConnection();

        Dictionary<string, string> GetUser(int userId);

        Dictionary<string,string> GetUser(string userName,string identifer);

        string GetUserSecret(int tenantId, string userName, string identifer,string type="TOTP");

        void CleanSecrets(string userNamer,string type);

        int GetUserId(string userName, string? phoneNumber);

        string GetRegistrationToken(int tenantId, string userName, string smsOrEmail);

        void ValidateUser(int userId, string smsOrEmail);

        public string GenerateAndSaveSecret(int tenantId,string userName,string type);

        public string SetAndGetUserJoopyToken(int userId, string tokenType = "");
    }
}
