using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskApi.Data;
using TaskApi.Data.Entities;
using TaskApi.Data.Requests;
using TaskApi.Data.Responses;
using TaskApi.Helpers;
using TaskApi.Interfaces;

namespace TaskApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly TasksDbContext tasksDbContext;

        public TokenService(TasksDbContext tasksDbContext)
        {
            this.tasksDbContext = tasksDbContext;
        }

        public async Task<Tuple<string, string>> GenerateTokenAsync(int userId)
        {
            var accessToken = await TokenHelper.GenerateAccessToken(userId);
            var refreshToken = await TokenHelper.GenerateRefreshToken();
            var userRecord = await tasksDbContext.Users.Include(O => O.RefreshTokens),

 }

        public async Task<ValidateRefreshTokenResponse> ValidateRefreshTokenAsync(RefreshTokenRequest refreshTokenRequest)
        {
            var refreshToken = await _tasksDbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == refreshTokenRequest.UserId);

            if (refreshToken == null)
            {
                return new ValidateRefreshTokenResponse
                {
                    Success = false,
                    Error = "Invalid refresh token",
                    ErrorCode = "R01"
                };
            }

            // Хешируем полученный refresh token для сравнения с сохраненным в БД
            var hashedRefreshToken = HashRefreshToken(refreshTokenRequest.RefreshToken);

            if (refreshToken.TokenHash != hashedRefreshToken)
            {
                return new ValidateRefreshTokenResponse
                {
                    Success = false,
                    Error = "Invalid refresh token",
                    ErrorCode = "R02"
                };
            }

            if (refreshToken.ExpiryDate < DateTime.UtcNow)
            {
                return new ValidateRefreshTokenResponse
                {
                    Success = false,
                    Error = "Refresh token expired",
                    ErrorCode = "R03"
                };
            }

            return new ValidateRefreshTokenResponse
            {
                Success = true,
                UserId = refreshTokenRequest.UserId
            };
        }

        private string GenerateAccessToken(int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(TokenHelper.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Email, GetUserEmail(userId) ?? string.Empty)
                }),
                Expires = DateTime.UtcNow.AddMinutes(TokenHelper.AccessTokenExpirationMinutes),
                Issuer = TokenHelper.Issuer,
                Audience = TokenHelper.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task SaveRefreshTokenAsync(int userId, string refreshToken)
        {
            // Удаляем старый refresh token если существует
            var existingToken = await _tasksDbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == userId);

            if (existingToken != null)
            {
                _tasksDbContext.RefreshTokens.Remove(existingToken);
            }

            // Сохраняем новый refresh token
            var refreshTokenEntity = new RefreshToken
            {
                UserId = userId,
                TokenHash = HashRefreshToken(refreshToken),
                ExpiryDate = DateTime.UtcNow.AddDays(TokenHelper.RefreshTokenExpirationDays),
                CreatedDate = DateTime.UtcNow
            };

            await _tasksDbContext.RefreshTokens.AddAsync(refreshTokenEntity);
            await _tasksDbContext.SaveChangesAsync();
        }

        private string HashRefreshToken(string refreshToken)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(refreshToken);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private string? GetUserEmail(int userId)
        {
            var user = _tasksDbContext.Users.Find(userId);
            return user?.Email;
        }

        public async Task<bool> RevokeRefreshTokenAsync(int userId)
        {
            var refreshToken = await _tasksDbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == userId);

            if (refreshToken != null)
            {
                _tasksDbContext.RefreshTokens.Remove(refreshToken);
                var result = await _tasksDbContext.SaveChangesAsync();
                return result > 0;
            }

            return true; // Если токена нет, считаем что отзыв успешен
        }
    }
}