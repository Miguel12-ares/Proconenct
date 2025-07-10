using ProConnect.Core.Entities;

namespace ProConnect.Application.DTOs
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public UserType UserType { get; set; }
        public DateTime ExpiresAt { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
