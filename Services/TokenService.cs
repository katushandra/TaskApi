using Microsoft.EntityFrameworkCore;
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
            var accessToken = await TokenHelper.GenerateAccessTokenAsync(userId);
            var refreshToken = await TokenHelper.GenerateRefreshToken();
            var userRecord = await tasksDbContext.Users.Include(O => O.RefreshTokens).FirstOrDefaultAsync(e => e.Id == userId);

            if (userRecord != null)
            {
                return null;
            }
            var salt = PasswordHelper.GetSecureSalt();
            var refreshTokenHashed = PasswordHelper.HashUsingPbkdf2(refreshToken, salt);

            if (userRecord.RefreshTokens != null && userRecord.RefreshTokens.Any())
            {
                await RemoveRefreshTokenAsync(userRecord);
            }

            userRecord.RefreshTokens?.Add(new RefreshToken
            {
                ExpiryDate = DateTime.Now.AddDays(30),
                Ts = DateTime.Now,
                UserId = userId,
                TokenHash = refreshTokenHashed,
                TokenSalt = Convert.ToBase64String(salt)
            });
            await tasksDbContext.SaveChangesAsync();
            var token = new Tuple<string, string>(accessToken, refreshTokenHashed);
            return token;
        }

        public async Task<bool> RemoveRefreshTokenAsync(User user)
        {
            var userRecord = await tasksDbContext.Users.Include(o => o.RefreshTokens).FirstOrDefaultAsync(e => e.Id == user.Id);

            if (userRecord == null)
            {
                return false;
            }

            if (userRecord.RefreshTokens != null && userRecord.RefreshTokens.Any())
            {
                var currentRefreshToken = userRecord.RefreshTokens.First();
                tasksDbContext.RefreshTokens.Remove(currentRefreshToken);
            }
            return false;
        }
        public async Task<ValidateRefreshTokenResponse> ValidateRefreshTokenAsync(RefreshTokenRequest refreshTokenRequest)
        {
            var refreshToken = await tasksDbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == refreshTokenRequest.UserId);
            var response = new ValidateRefreshTokenResponse();

            if (refreshToken == null)
            {
                response.Success = false;
                response.Error = "Invalid session or user already logged out";
                response.ErrorCode = "R02";

            }
            var refreshTokenToValidateHash = PasswordHelper.HashUsingPbkdf2(refreshTokenRequest.RefreshToken, Convert.FromBase64String(refreshToken.TokenSalt));

            if (refreshToken.TokenHash != refreshTokenToValidateHash)
            {
                response.Success = false;
                response.Error = "Invalid refresh token";
                response.ErrorCode = "R03";

            }

            if (refreshToken.ExpiryDate < DateTime.Now)
            {
                response.Success = false;
                response.Error = "Refresh token has expired";
                response.ErrorCode = "R04";
            }

            response.Success = true;
            response.UserId = refreshTokenRequest.UserId;
            return response;
        }
    }
}