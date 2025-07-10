using ProConnect.Core.Entities;

namespace ProConnect.Core.ValueObjects
{
    public class AuthResult
    {
        public bool IsSuccess { get; private set; }
        public string Token { get; private set; } = string.Empty;
        public string UserId { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public UserType UserType { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public List<string> Errors { get; private set; } = new();

        private AuthResult() { }

        public static AuthResult CreateSuccess(string token, string userId, string email, UserType userType, DateTime expiresAt)
        {
            return new AuthResult
            {
                IsSuccess = true,
                Token = token,
                UserId = userId,
                Email = email,
                UserType = userType,
                ExpiresAt = expiresAt
            };
        }

        public static AuthResult CreateFailure(params string[] errors)
        {
            return new AuthResult
            {
                IsSuccess = false,
                Errors = errors.ToList()
            };
        }

        public static AuthResult CreateFailure(List<string> errors)
        {
            return new AuthResult
            {
                IsSuccess = false,
                Errors = errors
            };
        }
    }
}
