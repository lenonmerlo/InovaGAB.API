// Services/Implementations/ProjectService.cs
using InovaGAB.API.Data;
using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;
using InovaGAB.API.Models;
using InovaGAB.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InovaGAB.API.Services.Implementations;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _context;

    public ProjectService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request, int managerId)
    {
        var project = new Project
        {
            Title = request.Title,
            Description = request.Description,
            Division = request.Division,
            Investment = request.Investment,
            StartDate = request.StartDate,
            Deadline = request.Deadline,
            IdeaId = request.IdeaId,
            ManagerId = managerId,
            Status = ProjectStatus.Planning,
            Stage = ProjectStage.Diagnosis,
            ProgressPercent = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        await _context.Entry(project).Reference(p => p.Manager).LoadAsync();

        return MapToResponse(project);
    }

    public async Task<List<ProjectResponse>> GetAllAsync()
    {
        var projects = await _context.Projects
            .Include(p => p.Manager)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return projects.Select(MapToResponse).ToList();
    }

    public async Task<ProjectResponse?> GetByIdAsync(int id)
    {
        var project = await _context.Projects
            .Include(p => p.Manager)
            .FirstOrDefaultAsync(p => p.Id == id);

        return project == null ? null : MapToResponse(project);
    }

    public async Task<ProjectResponse?> UpdateAsync(int id, UpdateProjectRequest request)
    {
        var project = await _context.Projects
            .Include(p => p.Manager)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null) return null;

        if (request.Title != null) project.Title = request.Title;
        if (request.Description != null) project.Description = request.Description;
        if (request.Status != null) project.Status = request.Status.Value;
        if (request.Stage != null) project.Stage = request.Stage.Value;
        if (request.Investment != null) project.Investment = request.Investment.Value;
        if (request.FinancialReturn != null) project.FinancialReturn = request.FinancialReturn.Value;
        if (request.ProductivityGain != null) project.ProductivityGain = request.ProductivityGain.Value;
        if (request.ProgressPercent != null) project.ProgressPercent = request.ProgressPercent.Value;
        if (request.Deadline != null) project.Deadline = request.Deadline.Value;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToResponse(project);
    }

    private static ProjectResponse MapToResponse(Project project) => new()
    {
        Id = project.Id,
        Title = project.Title,
        Description = project.Description,
        Division = project.Division,
        Status = project.Status.ToString(),
        Stage = project.Stage.ToString(),
        Investment = project.Investment,
        FinancialReturn = project.FinancialReturn,
        Roi = project.Roi,
        ProductivityGain = project.ProductivityGain,
        StartDate = project.StartDate,
        Deadline = project.Deadline,
        ProgressPercent = project.ProgressPercent,
        CreatedAt = project.CreatedAt,
        ManagerName = project.Manager?.Name ?? string.Empty,
        IdeaId = project.IdeaId
    };
}