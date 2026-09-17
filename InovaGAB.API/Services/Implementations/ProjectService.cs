using InovaGAB.API.Data;
using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;
using InovaGAB.API.Models;
using InovaGAB.API.Services.Interfaces;
using MongoDB.Driver;

namespace InovaGAB.API.Services.Implementations;

public class ProjectService : IProjectService
{
    private static readonly Dictionary<ProjectStatus, ProjectStatus[]>
        AllowedStatusTransitions = new()
        {
            [ProjectStatus.Planning] = new[]
            {
                ProjectStatus.Planning,
                ProjectStatus.InProgress,
                ProjectStatus.Cancelled
            },
            [ProjectStatus.InProgress] = new[]
            {
                ProjectStatus.InProgress,
                ProjectStatus.OnHold,
                ProjectStatus.Completed,
                ProjectStatus.Cancelled
            },
            [ProjectStatus.OnHold] = new[]
            {
                ProjectStatus.OnHold,
                ProjectStatus.InProgress,
                ProjectStatus.Cancelled
            },
            [ProjectStatus.Completed] = new[]
            {
                ProjectStatus.Completed
            },
            [ProjectStatus.Cancelled] = new[]
            {
                ProjectStatus.Cancelled
            }
        };

    private static readonly Dictionary<ProjectStage, ProjectStage[]>
        AllowedStageTransitions = new()
        {
            [ProjectStage.Diagnosis] = new[]
            {
                ProjectStage.Diagnosis,
                ProjectStage.Implementation
            },
            [ProjectStage.Implementation] = new[]
            {
                ProjectStage.Implementation,
                ProjectStage.Validation
            },
            [ProjectStage.Validation] = new[]
            {
                ProjectStage.Validation,
                ProjectStage.Closure
            },
            [ProjectStage.Closure] = new[]
            {
                ProjectStage.Closure
            }
        };

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

        if (request.Investment < 0)
        {
            throw new ArgumentException(
                "O investimento não pode ser negativo.");
        }

        if (request.Deadline < request.StartDate)
        {
            throw new ArgumentException(
                "O prazo não pode ser anterior à data de início.");
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

        var guideline = await ResolveGuidelineAsync(
            request.GuidelineId);

        var project = new Project
        {
            Title = request.Title,
            Description = request.Description,
            Division = request.Division,
            Investment = request.Investment,
            StartDate = request.StartDate,
            Deadline = request.Deadline,
            IdeaId = request.IdeaId,
            GuidelineId = request.GuidelineId,
            Guideline = guideline,
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
            .Find(project => !project.IsArchived)
            .SortByDescending(project => project.CreatedAt)
            .ToListAsync();

        foreach (var project in projects)
        {
            await LoadManagerAsync(project);
        }

        await LoadGuidelinesAsync(projects);

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
        await LoadGuidelineAsync(project);

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
            EnsureValidStatusTransition(
                project.Status,
                request.Status.Value);

            project.Status = request.Status.Value;
        }

        if (request.Stage != null)
        {
            EnsureValidStageTransition(
                project.Stage,
                request.Stage.Value);

            project.Stage = request.Stage.Value;
        }

        if (request.Investment != null)
        {
            if (request.Investment.Value < 0)
            {
                throw new ArgumentException(
                    "O investimento não pode ser negativo.");
            }

            project.Investment = request.Investment.Value;
        }

        if (request.FinancialReturn != null)
        {
            if (request.FinancialReturn.Value < 0)
            {
                throw new ArgumentException(
                    "O retorno financeiro não pode ser negativo.");
            }

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
            if (request.ProgressPercent.Value is < 0 or > 100)
            {
                throw new ArgumentException(
                    "O progresso deve estar entre 0 e 100.");
            }

            project.ProgressPercent =
                request.ProgressPercent.Value;
        }

        if (request.Deadline != null)
        {
            if (request.Deadline.Value < project.StartDate)
            {
                throw new ArgumentException(
                    "O prazo não pode ser anterior à data de início.");
            }

            project.Deadline = request.Deadline.Value;
        }

        if (request.GuidelineId != null)
        {
            project.Guideline = await ResolveGuidelineAsync(
                request.GuidelineId);
            project.GuidelineId = request.GuidelineId;
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

        if (request.GuidelineId == null)
        {
            await LoadGuidelineAsync(project);
        }

        return MapToResponse(project);
    }

    public async Task<bool?> ArchiveAsync(string id)
    {
        var project = await _context.Projects
            .Find(project => project.Id == id)
            .FirstOrDefaultAsync();

        if (project == null)
        {
            return null;
        }

        if (project.IsArchived)
        {
            return true;
        }

        var update = Builders<Project>.Update
            .Set(existingProject => existingProject.IsArchived, true)
            .Set(existingProject => existingProject.ArchivedAt, DateTime.UtcNow);

        var result = await _context.Projects.UpdateOneAsync(
            existingProject => existingProject.Id == id,
            update);

        return result.MatchedCount > 0;
    }

    private static void EnsureValidStatusTransition(
        ProjectStatus current,
        ProjectStatus next)
    {
        if (!AllowedStatusTransitions[current].Contains(next))
        {
            throw new InvalidOperationException(
                $"Transição de status inválida: {current} -> {next}.");
        }
    }

    private static void EnsureValidStageTransition(
        ProjectStage current,
        ProjectStage next)
    {
        if (!AllowedStageTransitions[current].Contains(next))
        {
            throw new InvalidOperationException(
                $"Transição de etapa inválida: {current} -> {next}.");
        }
    }

    private async Task<StrategicGuideline?> ResolveGuidelineAsync(
        string? guidelineId)
    {
        if (guidelineId == null)
        {
            return null;
        }

        var guideline = await _context.StrategicGuidelines
            .Find(strategicGuideline =>
                strategicGuideline.Id == guidelineId &&
                strategicGuideline.IsActive)
            .FirstOrDefaultAsync();

        if (guideline == null)
        {
            throw new InvalidOperationException(
                "Diretriz estratégica não encontrada ou inativa.");
        }

        return guideline;
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

    private async Task LoadGuidelineAsync(Project project)
    {
        if (project.GuidelineId == null)
        {
            return;
        }

        project.Guideline = await _context.StrategicGuidelines
            .Find(guideline => guideline.Id == project.GuidelineId)
            .FirstOrDefaultAsync();
    }

    // carrega as diretrizes dos projetos em uma única consulta ($in), evita N+1
    private async Task LoadGuidelinesAsync(List<Project> projects)
    {
        var guidelineIds = projects
            .Where(project => project.GuidelineId != null)
            .Select(project => project.GuidelineId!)
            .Distinct()
            .ToList();

        if (guidelineIds.Count == 0)
        {
            return;
        }

        var guidelines = await _context.StrategicGuidelines
            .Find(guideline => guidelineIds.Contains(guideline.Id))
            .ToListAsync();

        var guidelinesById = guidelines
            .ToDictionary(guideline => guideline.Id);

        foreach (var project in projects)
        {
            if (project.GuidelineId != null &&
                guidelinesById.TryGetValue(
                    project.GuidelineId,
                    out var guideline))
            {
                project.Guideline = guideline;
            }
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
            IdeaId = project.IdeaId,
            GuidelineId = project.GuidelineId,
            Guideline = project.Guideline == null
                ? null
                : new GuidelineSummaryResponse
                {
                    Id = project.Guideline.Id,
                    Title = project.Guideline.Title,
                    Category = project.Guideline.Category,
                    Campaign = project.Guideline.Campaign,
                    IsActive = project.Guideline.IsActive
                },
            IsArchived = project.IsArchived
        };
    }
}
