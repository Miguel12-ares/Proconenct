using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ProConnect.Application.DTOs;

namespace ProConnect.Application.Interfaces
{
    public interface IPortfolioService
    {
        Task<PortfolioFileDto> UploadPortfolioFileAsync(string userId, IFormFile file, string description);
        Task<List<PortfolioFileDto>> GetPortfolioFilesAsync(string userId);
        Task<bool> DeletePortfolioFileAsync(string userId, string fileId);
        Task<bool> UpdatePortfolioFileDescriptionAsync(string userId, string fileId, string description);
    }
} 