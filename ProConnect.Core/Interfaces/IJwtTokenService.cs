using ProConnect.Core.Entities;
using ProConnect.Core.ValueObjects;

namespace ProConnect.Core.Interfaces
{
    public interface IJwtTokenService
    {
        Task<string> GenerateTokenAsync(User user);
        Task<AuthResult> ValidateTokenAsync(string token);
        Task<bool> IsTokenValidAsync(string token);
        string GenerateRefreshToken();
        Task<AuthResult> RefreshTokenAsync(string token, string refreshToken);
    }
}
