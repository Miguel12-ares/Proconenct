using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProConnect.Application.DTOs;
using ProConnect.Application.DTOs.Shared;
using ProConnect.Application.Interfaces;

namespace Proconenct.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDto>> GetById(string id)
        {
            var review = await _reviewService.GetByIdAsync(id);
            if (review == null) return NotFound();
            return Ok(review);
        }

        [HttpGet("by-professional/{professionalId}")]
        public async Task<ActionResult<List<ReviewDto>>> GetByProfessional(string professionalId)
        {
            var reviews = await _reviewService.GetByProfessionalIdAsync(professionalId);
            return Ok(reviews);
        }

        [HttpGet("by-professional/{professionalId}/paged")]
        public async Task<ActionResult<PagedResultDto<ReviewDto>>> GetByProfessionalPaged(string professionalId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            var paged = await _reviewService.GetByProfessionalIdPagedAsync(professionalId, page, pageSize);
            return Ok(paged);
        }

        [HttpGet("by-client/{clientId}")]
        public async Task<ActionResult<List<ReviewDto>>> GetByClient(string clientId)
        {
            var reviews = await _reviewService.GetByClientIdAsync(clientId);
            return Ok(reviews);
        }

        [HttpGet("by-client/{clientId}/paged")]
        public async Task<ActionResult<PagedResultDto<ReviewDto>>> GetByClientPaged(string clientId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            var paged = await _reviewService.GetByClientIdPagedAsync(clientId, page, pageSize);
            return Ok(paged);
        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<ActionResult<ReviewDto>> Create([FromBody] CreateReviewDto dto)
        {
            // Extraer clientId del JWT
            var clientId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(clientId))
                return Unauthorized("No se pudo identificar al usuario autenticado.");

            // Sobrescribir clientId para seguridad
            dto.ClientId = clientId;

            try
            {
                var created = await _reviewService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateReviewDto dto)
        {
            var updated = await _reviewService.UpdateAsync(id, dto);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _reviewService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
