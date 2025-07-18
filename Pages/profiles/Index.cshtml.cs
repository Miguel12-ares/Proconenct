using Microsoft.AspNetCore.Mvc.RazorPages;
using ProConnect.Application.Services;
using ProConnect.Application.DTOs;
using ProConnect.Application.DTOs.Shared;
using ProConnect.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Web;

namespace Proconenct.Pages.profiles
{
    public class IndexModel : PageModel
    {
        private readonly IProfessionalSearchService _searchService;

        public IndexModel(IProfessionalSearchService searchService)
        {
            _searchService = searchService;
        }

        [BindProperty(SupportsGet = true)]
        public string Query { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Specialty { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Location { get; set; }

        [BindProperty(SupportsGet = true)]
        public int MinRating { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public int MinExperience { get; set; } = 0;

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public PagedResultDto<ProfessionalSearchResultDto> SearchResults { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var filters = new ProfessionalSearchFiltersDto
                {
                    Query = Query,
                    Specialties = !string.IsNullOrEmpty(Specialty) ? new List<string> { Specialty } : null,
                    Location = Location,
                    MinHourlyRate = MinPrice,
                    MaxHourlyRate = MaxPrice,
                    MinExperienceYears = MinExperience,
                    MinRating = MinRating,
                    Page = CurrentPage,
                    PageSize = 12
                };

                SearchResults = await _searchService.SearchProfessionalsAsync(filters);
            }
            catch (Exception ex)
            {
                // Log error
                SearchResults = new PagedResultDto<ProfessionalSearchResultDto>
                {
                    Items = new List<ProfessionalSearchResultDto>(),
                    TotalCount = 0,
                    Page = CurrentPage,
                    PageSize = 12
                };
            }
        }

        /// <summary>
        /// Genera la cadena de consulta para la paginación manteniendo los filtros actuales
        /// </summary>
        public string GetQueryString()
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(Query))
                queryParams.Add($"query={Uri.EscapeDataString(Query)}");

            if (!string.IsNullOrEmpty(Specialty))
                queryParams.Add($"specialty={Uri.EscapeDataString(Specialty)}");

            if (!string.IsNullOrEmpty(Location))
                queryParams.Add($"location={Uri.EscapeDataString(Location)}");

            if (MinRating > 1)
                queryParams.Add($"minRating={MinRating}");

            if (MinPrice.HasValue)
                queryParams.Add($"minPrice={MinPrice}");

            if (MaxPrice.HasValue)
                queryParams.Add($"maxPrice={MaxPrice}");

            if (MinExperience > 0)
                queryParams.Add($"minExperience={MinExperience}");

            return string.Join("&", queryParams);
        }
    }
} 