namespace ProConnect.Application.DTOs
{
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty; // Identificador único del usuario
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; // Solo lectura, no editable
        
        // Propiedades adicionales para las vistas
        public string Name => $"{FirstName} {LastName}".Trim();
        public string PhoneNumber => Phone;
    }
}