using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProConnect.Core.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [BsonElement("lastName")]
        public string LastName { get; set; } = string.Empty;

        [BsonElement("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [BsonElement("userType")]
        public UserType UserType { get; set; } = UserType.Client;

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("emailVerified")]
        public bool EmailVerified { get; set; } = false;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("lastLoginAt")]
        public DateTime? LastLoginAt { get; set; }

        // Método para validar el dominio de la entidad
        public bool IsValidForRegistration()
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   UserType != UserType.Unknown;
        }

        public void UpdateLastLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public enum UserType
    {
        Unknown = 0,
        Client = 1,
        Professional = 2,
        Administrator = 3
    }
}
