using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OTPManager.Utilities
{
    public static class JWTGenerator
    {
        public static string GenerateJwtToken(string userName, string identifier, int tenantId, int userId,string phoneNumber, string email, IConfiguration configuration)
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
    }
}
