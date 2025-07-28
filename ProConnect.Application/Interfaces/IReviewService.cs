using System.Collections.Generic;
using System.Threading.Tasks;
using ProConnect.Application.DTOs;
using ProConnect.Application.DTOs.Shared;

namespace ProConnect.Application.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewDto> GetByIdAsync(string id);
        Task<List<ReviewDto>> GetByProfessionalIdAsync(string professionalId);
        Task<PagedResultDto<ReviewDto>> GetByProfessionalIdPagedAsync(string professionalId, int page, int pageSize);
        Task<List<ReviewDto>> GetByClientIdAsync(string clientId);
        Task<PagedResultDto<ReviewDto>> GetByClientIdPagedAsync(string clientId, int page, int pageSize);
        Task<ReviewDto> CreateAsync(CreateReviewDto dto);
        Task<bool> UpdateAsync(string id, UpdateReviewDto dto);
        Task<bool> DeleteAsync(string id);
    }
}
