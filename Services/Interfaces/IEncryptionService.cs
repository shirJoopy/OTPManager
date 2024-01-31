namespace OTPManager.Services.Interfaces
{
    public interface IEncryptionService
    {
        public string Encrypt(string textToEncrypt);
        public string Decrypt(string encryptedText);

    }
}
