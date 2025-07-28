using System.Collections.Generic;
using System.Threading.Tasks;
using ProConnect.Application.DTOs;

namespace ProConnect.Application.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewDto> GetByIdAsync(string id);
        Task<List<ReviewDto>> GetByProfessionalIdAsync(string professionalId);
        Task<List<ReviewDto>> GetByClientIdAsync(string clientId);
        Task<ReviewDto> CreateAsync(CreateReviewDto dto);
        Task<bool> UpdateAsync(string id, UpdateReviewDto dto);
        Task<bool> DeleteAsync(string id);
    }
}
