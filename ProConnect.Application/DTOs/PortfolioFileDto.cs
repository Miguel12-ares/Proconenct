using System;

namespace ProConnect.Application.DTOs
{
    /// <summary>
    /// DTO para archivos del portafolio profesional.
    /// </summary>
    public class PortfolioFileDto
    {
        public string Id { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
} 