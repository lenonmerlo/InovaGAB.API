using System.Security.Claims;
using InovaGAB.API.DTOs.Request;
using InovaGAB.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InovaGAB.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IdeaController : ControllerBase
    {
        private readonly IIdeaService _ideaService;

        public IdeaController(IIdeaService ideaService)
        {
            _ideaService = ideaService;
        }

        [HttpPost]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> Create([FromBody] CreateIdeaRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var response = await _ideaService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(Create), new { id = response.Id }, response);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> GetMyIdeas()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ideas = await _ideaService.GetMyIdeasAsync(userId);
            return Ok(ideas);
        }

        [HttpGet]
        [Authorize(Roles ="Manager,Leader")]
        public async Task<IActionResult> GetAll()
        {
            var ideas = await _ideaService.GetAllAsync();
            return Ok(ideas);
        }

        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Approve(int id, [FromBody] ApproveIdeaRequest request)
        {
            var result = await _ideaService.ApproveAsync(id, request.ImpactScore, request.FeasibilityScore, request.AlignmentScore);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPatch("{id}/reject")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _ideaService.RejectAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
