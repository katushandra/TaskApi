using TaskApi.Data.Requests;
using TaskApi.Data.Responses;
using TaskApi.Data.Entities;

namespace TaskApi.Interfaces
{
    public interface ITokenService
    {
        Task<Tuple<string, string>> GenerateTokenAsync(int userId);
        Task<ValidateRefreshTokenResponse> ValidateRefreshTokenAsync(RefreshTokenRequest refreshTokenRequest);
        Task<bool> RemoveRefreshTokenAsync(User user);
    }
}
