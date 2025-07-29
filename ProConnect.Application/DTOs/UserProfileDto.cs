using ProConnect.Core.Entities;

namespace ProConnect.Application.DTOs
{
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty; // Identificador único del usuario
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; // Solo lectura, no editable
        public UserType UserType { get; set; } = UserType.Client;
        public bool IsActive { get; set; } = true;
        public bool EmailVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        
        // Propiedades adicionales para las vistas
        public string Name { get { return ($"{FirstName} {LastName}").Trim(); } }
    }
}