using OTPManager.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace OTPManager.Services
{
    public class EncryptionService : IEncryptionService
    {

        private readonly string encryptionKey;

        public EncryptionService()
        {
            // Retrieve the encryption key from an environment variable
            encryptionKey = Environment.GetEnvironmentVariable("ENCRYPTION_KEY") ?? "this_is_not_safe";
           /* if (string.IsNullOrWhiteSpace(encryptionKey))
            {
                throw new ApplicationException("Encryption key not found in environment variables.");
            }*/
        }

        public string Encrypt(string textToEncrypt)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(encryptionKey);
                aesAlg.IV = new byte[16]; // Initialization vector (IV) should be unique and random for each encryption

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(textToEncrypt);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public string Decrypt(string encryptedText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(encryptionKey);
                aesAlg.IV = new byte[16]; // Same IV as used for encryption

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

}


