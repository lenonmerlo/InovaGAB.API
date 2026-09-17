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
        private readonly IAiScoringService _aiScoringService;

        public IdeaController(
            IIdeaService ideaService,
            IAiScoringService aiScoringService)
        {
            _ideaService = ideaService;
            _aiScoringService = aiScoringService;
        }

        [HttpPost]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> Create(
            [FromBody] CreateIdeaRequest request)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var response = await _ideaService.CreateAsync(
                request,
                userId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = response.Id },
                response);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> GetMyIdeas()
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var ideas = await _ideaService.GetMyIdeasAsync(userId);

            return Ok(ideas);
        }

        [HttpGet]
        [Authorize(Roles = "Manager,Leader")]
        public async Task<IActionResult> GetAll()
        {
            var ideas = await _ideaService.GetAllAsync();

            return Ok(ideas);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Operator,Manager,Leader")]
        public async Task<IActionResult> GetById(string id)
        {
            var idea = await _ideaService.GetByIdAsync(id);

            if (idea == null)
            {
                return NotFound();
            }

            if (IsOperatorNotOwner(idea.UserId))
            {
                throw new UnauthorizedAccessException(
                    "Você só pode consultar as próprias ideias.");
            }

            return Ok(idea);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> Update(
            string id,
            [FromBody] UpdateIdeaRequest request)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _ideaService.UpdateAsync(
                id,
                request,
                userId);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _ideaService.DeleteAsync(
                id,
                userId);

            if (result != true)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPatch("{id}/prioritize")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Prioritize(
            string id,
            [FromBody] PrioritizeIdeaRequest request)
        {
            var result = await _ideaService.PrioritizeAsync(
                id,
                request.Priority);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost("{id}/ai-score")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> SuggestAiScore(
            string id,
            CancellationToken cancellationToken)
        {
            var suggestion = await _aiScoringService.SuggestScoreAsync(
                id,
                cancellationToken);

            if (suggestion == null)
            {
                return NotFound();
            }

            return Ok(suggestion);
        }

        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Approve(
            string id,
            [FromBody] ApproveIdeaRequest request)
        {
            var result = await _ideaService.ApproveAsync(
                id,
                request.ImpactScore,
                request.FeasibilityScore,
                request.AlignmentScore);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPatch("{id}/reject")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Reject(string id)
        {
            var result = await _ideaService.RejectAsync(id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        private bool IsOperatorNotOwner(string ideaUserId)
        {
            if (!User.IsInRole("Operator"))
            {
                return false;
            }

            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return userId != ideaUserId;
        }
    }
}
