using Microsoft.AspNetCore.Mvc.RazorPages;
using ProConnect.Application.Services;
using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Proconenct.Pages.profiles
{
    public class DetailModel : PageModel
    {
        private readonly IProfessionalProfileService _profileService;
        private readonly IPortfolioService _portfolioService;

        public DetailModel(IProfessionalProfileService profileService, IPortfolioService portfolioService)
        {
            _profileService = profileService;
            _portfolioService = portfolioService;
        }

        [BindProperty(SupportsGet = true)]
        public string Id { get; set; }

        public ProfessionalProfileResponseDto? Professional { get; set; }
        public List<PortfolioFileDto>? PortfolioFiles { get; set; }
        public List<ProfessionalRecommendationDto>? Reviews { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(Id))
                {
                    return NotFound();
                }

                // Obtener perfil profesional
                Professional = await _profileService.GetPublicProfileAsync(Id);
                
                if (Professional == null)
                {
                    return NotFound();
                }

                // Obtener archivos del portafolio
                PortfolioFiles = await _portfolioService.GetPortfolioFilesAsync(Id);

                // Obtener reseñas (placeholder - implementar cuando esté disponible el servicio de reseñas)
                Reviews = new List<ProfessionalRecommendationDto>();

                return Page();
            }
            catch (Exception ex)
            {
                // Log error
                return NotFound();
            }
        }
    }
} 