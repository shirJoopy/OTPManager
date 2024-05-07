using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OTPManager.Utilities
{
    public static class JWTGenerator
    {
        public static Dictionary<string, string> JWTParams { get; private set; }

        public static string GenerateJwtToken(string userName, string identifier, int tenantId, int userId, string phoneNumber, string email, IConfiguration configuration)
        {
            // Define token claims, including custom claims for tenantId and userId
            var claims = new List<Claim>
            {
                new Claim("userName", userName),
                new Claim("identifier", identifier),
                new Claim("phoneNumber", phoneNumber??""),
                new Claim("email", email??""),
                new Claim("tenantId", tenantId.ToString()), // Custom claim for tenantId
                new Claim("userId", userId.ToString()), // Custom claim for userId
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString())
                // Add additional claims as needed
            };

            // Create signing key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtEncryptionKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Generate token
            var token = new JwtSecurityToken(
                issuer: "www.joopy.co.il/TOTP",
                claims: claims,
                expires: DateTime.Now.AddMinutes(60), // Token expiration time
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string GenerateAccessJwtToken(Dictionary<string, string> userData, IConfiguration configuration)
        {
            // Define token claims, including custom claims for tenantId and userId
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString())
                // Add additional claims as needed
            };
            foreach (var pair in userData)
            {
                if (pair.Value != null)
                {
                    claims.Add(new Claim(pair.Key, pair.Value.ToString()));
                }

            }

            // Create signing key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtEncryptionKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Generate token
            var token = new JwtSecurityToken(
                        issuer: "www.joopy.co.il",
                        claims: claims,
                        expires: DateTime.Now.AddMinutes(60), // Token expiration time
                        signingCredentials: credentials
                        );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static Dictionary<string, string> ValidateAndReadToken(string token, string secretKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            Dictionary<string, string> res = new Dictionary<string, string>();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidateAudience = false,
                ValidIssuer = "www.joopy.co.il",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };

            try
            {
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

                // Token is valid, and you can read the claims
                foreach (var claim in principal.Claims)
                {
                    res.Add(claim.Type, claim.Value);
                }

                JWTParams = res;
                return res;
            }
            catch (SecurityTokenException)
            {
                // Token is invalid

                Console.WriteLine("Token validation failed");
                return null;
            }
        }
    }

}
