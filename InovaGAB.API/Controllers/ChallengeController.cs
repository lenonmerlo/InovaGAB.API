using System.Security.Claims;
using InovaGAB.API.DTOs.Request;
using InovaGAB.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InovaGAB.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChallengeController : ControllerBase
{
    private readonly IChallengeService _challengeService;

    public ChallengeController(IChallengeService challengeService)
    {
        _challengeService = challengeService;
    }

    [HttpPost]
    [Authorize(Roles = "Leader")]
    public async Task<IActionResult> Create([FromBody] CreateChallengeRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _challengeService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    [Authorize(Roles = "Operator,Manager,Leader")]
    public async Task<IActionResult> GetAll()
    {
        var challenges = await _challengeService.GetAllActiveAsync();
        return Ok(challenges);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Operator,Manager,Leader")]
    public async Task<IActionResult> GetById(int id)
    {
        var challenge = await _challengeService.GetByIdAsync(id);
        if (challenge == null) return NotFound();
        return Ok(challenge);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Leader")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateChallengeRequest request)
    {
        var result = await _challengeService.UpdateAsync(id, request);
        if (result == null) return NotFound();
        return Ok(result);
    }
}