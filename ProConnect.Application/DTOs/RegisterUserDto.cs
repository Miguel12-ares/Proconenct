using ProConnect.Core.Entities;

namespace ProConnect.Application.DTOs
{
    public class RegisterUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string DocumentId { get; set; } = string.Empty;
        public DocumentType DocumentType { get; set; } = DocumentType.CC;
        public UserType UserType { get; set; } = UserType.Client;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiresAt { get; set; }
    }
}
