using InovaGAB.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InovaGAB.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IProjectService _projectService;

    public DashboardController(
        IDashboardService dashboardService,
        IProjectService projectService)
    {
        _dashboardService = dashboardService;
        _projectService = projectService;
    }

    [HttpGet]
    [Authorize(Roles = "Leader")]
    public async Task<IActionResult> GetDashboard()
    {
        var response = await _dashboardService.GetDashboardAsync();
        return Ok(response);
    }

    [HttpGet("guideline/{id}")]
    [Authorize(Roles = "Leader")]
    public async Task<IActionResult> GetByGuideline(string id)
    {
        var response = await _dashboardService.GetByGuidelineAsync(id);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpGet("project/{id}")]
    [Authorize(Roles = "Manager,Leader")]
    public async Task<IActionResult> GetByProject(string id)
    {
        var response = await _projectService.GetByIdAsync(id);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }
}
