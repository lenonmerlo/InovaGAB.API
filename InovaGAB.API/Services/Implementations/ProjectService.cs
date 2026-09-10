using InovaGAB.API.Data;
using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;
using InovaGAB.API.Models;
using InovaGAB.API.Services.Interfaces;
using MongoDB.Driver;

namespace InovaGAB.API.Services.Implementations;

public class ProjectService : IProjectService
{
    private readonly MongoDbContext _context;

    public ProjectService(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectResponse> CreateAsync(
        CreateProjectRequest request,
        string managerId)
    {
        var manager = await _context.Users
            .Find(user => user.Id == managerId)
            .FirstOrDefaultAsync();

        if (manager == null)
        {
            throw new InvalidOperationException(
                "Gestor responsável não encontrado.");
        }

        if (request.IdeaId != null)
        {
            var ideaExists = await _context.Ideas
                .Find(idea => idea.Id == request.IdeaId)
                .AnyAsync();

            if (!ideaExists)
            {
                throw new InvalidOperationException(
                    "Ideia vinculada não encontrada.");
            }
        }

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
            Manager = manager,
            Status = ProjectStatus.Planning,
            Stage = ProjectStage.Diagnosis,
            ProgressPercent = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Projects.InsertOneAsync(project);

        return MapToResponse(project);
    }

    public async Task<List<ProjectResponse>> GetAllAsync()
    {
        var projects = await _context.Projects
            .Find(_ => true)
            .SortByDescending(project => project.CreatedAt)
            .ToListAsync();

        foreach (var project in projects)
        {
            await LoadManagerAsync(project);
        }

        return projects
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<ProjectResponse?> GetByIdAsync(
        string id)
    {
        var project = await _context.Projects
            .Find(project => project.Id == id)
            .FirstOrDefaultAsync();

        if (project == null)
        {
            return null;
        }

        await LoadManagerAsync(project);

        return MapToResponse(project);
    }

    public async Task<ProjectResponse?> UpdateAsync(
        string id,
        UpdateProjectRequest request)
    {
        var project = await _context.Projects
            .Find(project => project.Id == id)
            .FirstOrDefaultAsync();

        if (project == null)
        {
            return null;
        }

        if (request.Title != null)
        {
            project.Title = request.Title;
        }

        if (request.Description != null)
        {
            project.Description = request.Description;
        }

        if (request.Status != null)
        {
            project.Status = request.Status.Value;
        }

        if (request.Stage != null)
        {
            project.Stage = request.Stage.Value;
        }

        if (request.Investment != null)
        {
            project.Investment = request.Investment.Value;
        }

        if (request.FinancialReturn != null)
        {
            project.FinancialReturn =
                request.FinancialReturn.Value;
        }

        if (request.ProductivityGain != null)
        {
            project.ProductivityGain =
                request.ProductivityGain.Value;
        }

        if (request.ProgressPercent != null)
        {
            project.ProgressPercent =
                request.ProgressPercent.Value;
        }

        if (request.Deadline != null)
        {
            project.Deadline = request.Deadline.Value;
        }

        project.UpdatedAt = DateTime.UtcNow;

        var updateResult =
            await _context.Projects.ReplaceOneAsync(
                existingProject => existingProject.Id == id,
                project);

        if (updateResult.MatchedCount == 0)
        {
            return null;
        }

        await LoadManagerAsync(project);

        return MapToResponse(project);
    }

    private async Task LoadManagerAsync(Project project)
    {
        var manager = await _context.Users
            .Find(user => user.Id == project.ManagerId)
            .FirstOrDefaultAsync();

        if (manager != null)
        {
            project.Manager = manager;
        }
    }

    private static ProjectResponse MapToResponse(
        Project project)
    {
        return new ProjectResponse
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
            ManagerName =
                project.Manager?.Name ?? string.Empty,
            IdeaId = project.IdeaId
        };
    }
}
