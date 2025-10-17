using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace TaskApi.Helpers
{
    public class TokenHelper
    {
        public static readonly string Issuer = "https://sutkt.ru/";
        public static readonly string Audience = "https://sutkt.ru/";
        public static readonly string Secret = "pOGX06VuVZLRPef0ty09jCqK4uZufDa6LP4n8Gj+8hQPB30f94pFiECAnPeMi5N6VT3/uscoGH7+ZJv4AuuPg==";

        public static async Task<string> GenerateAccessTokenAsync(int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Convert.FromBase64String(Secret);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = Issuer,
                Audience = Audience,
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = signingCredentials,
            };

            var secutityToken = tokenHandler.CreateToken(tokenDescriptor);
            return await Task.Run(()=>tokenHandler.WriteToken(secutityToken));
        }

        public static async Task<string> GenerateRefreshToken()
        {
            var secureRandomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            await Task.Run(() => rng.GetBytes(secureRandomBytes));
            var refreshToken = Convert.ToBase64String(secureRandomBytes);
            return refreshToken;
        }
    }
}
