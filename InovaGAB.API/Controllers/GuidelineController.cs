using InovaGAB.API.DTOs.Request;
using InovaGAB.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InovaGAB.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GuidelineController : ControllerBase

    {
        private readonly IGuidelineService _guidelineService;

        public GuidelineController(IGuidelineService guidelineService)
        {
            _guidelineService = guidelineService;
        }

        [HttpPost]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> Create([FromBody] CreateGuidelineRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var response = await _guidelineService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpGet]
        [Authorize(Roles = "Operator,Manager,Leader")]
        public async Task<IActionResult> GetAll()
        {
            var guidelines = await _guidelineService.GetAllAsync();
            return Ok(guidelines);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Operator,Manager,Leader")]
        public async Task<IActionResult> GetById(int id)
        {
            var guideline = await _guidelineService.GetByIdAsync(id);
            if (guideline == null) return NotFound();
            return Ok(guideline);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateGuidelineRequest request)
        {
            var result = await _guidelineService.UpdateAsync(id, request);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _guidelineService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
