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
        public static string Secret = "VGhpcyBpcyBhIHN1cGVyIHNlY3JldCBrZXkgZm9yIEpXVCAhISEhISE=";

        public static async Task<string> GenerateAccessTokenAsync(int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Convert.FromBase64String(Secret);
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            });

            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claimsIdentity),
                Issuer = Issuer,
                Audience = Audience,
                Expires = DateTime.Now.AddMinutes(15),
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
