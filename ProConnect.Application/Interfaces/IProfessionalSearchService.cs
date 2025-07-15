using ProConnect.Application.DTOs;
using ProConnect.Application.DTOs.Shared;
using System.Threading.Tasks;

namespace ProConnect.Application.Interfaces
{
    public interface IProfessionalSearchService
    {
        Task<PagedResultDto<ProfessionalSearchResultDto>> SearchProfessionalsAsync(ProfessionalSearchFiltersDto filters);
    }
} 