using System.Security.Cryptography;

namespace OTPManager.Utilities
{
    public class SecretGenerator
    {
        public static string GenerateRandomSecret(int lengthBytes = 32)
        {
            if (lengthBytes <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lengthBytes), "Length must be greater than zero.");
            }

            byte[] secretBytes = new byte[lengthBytes];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(secretBytes);
            }

            // You can convert the bytes to a hexadecimal string
            //string secretHex = BitConverter.ToString(secretBytes).Replace("-", "");

            // Or you can convert the bytes to Base64
            string secretBase64 = Convert.ToBase64String(secretBytes);

            return secretBase64;
        }
    }

}

