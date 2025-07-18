using System.Collections.Generic;
using System.Threading.Tasks;
using ProConnect.Core.Entities;

namespace ProConnect.Core.Interfaces
{
    public interface IPortfolioRepository
    {
        Task AddFileAsync(PortfolioFile file);
        Task<List<PortfolioFile>> GetFilesByUserAsync(string userId);
        Task<PortfolioFile?> GetFileByIdAsync(string userId, string fileId);
        Task DeleteFileAsync(string userId, string fileId);
        Task<bool> UpdateFileDescriptionAsync(string userId, string fileId, string description);
    }
} 