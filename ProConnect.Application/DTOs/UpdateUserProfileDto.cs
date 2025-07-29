using System.ComponentModel.DataAnnotations;

namespace ProConnect.Application.DTOs
{
    public class UpdateUserProfileDto
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Bio { get; set; } = string.Empty;
    }
} 