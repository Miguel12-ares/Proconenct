using ProConnect.Application.DTOs;
using ProConnect.Core.ValueObjects;

namespace ProConnect.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginUserDto loginDto);
        Task<AuthResponseDto> RefreshTokenAsync(string token, string refreshToken);
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> LogoutAsync(string userId);
    }
}
