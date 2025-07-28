using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProConnect.Application.DTOs;
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

        [HttpGet("by-client/{clientId}")]
        public async Task<ActionResult<List<ReviewDto>>> GetByClient(string clientId)
        {
            var reviews = await _reviewService.GetByClientIdAsync(clientId);
            return Ok(reviews);
        }

        [HttpPost]
        public async Task<ActionResult<ReviewDto>> Create([FromBody] CreateReviewDto dto)
        {
            var created = await _reviewService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
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
