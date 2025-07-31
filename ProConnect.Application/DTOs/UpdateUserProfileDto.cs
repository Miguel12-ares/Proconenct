using System.ComponentModel.DataAnnotations;
using ProConnect.Core.Entities;

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

        [Required]
        [MaxLength(20)]
        public string DocumentId { get; set; } = string.Empty;

        [Required]
        public DocumentType DocumentType { get; set; } = DocumentType.CC;

        [MaxLength(500)]
        public string Bio { get; set; } = string.Empty;
    }
} 