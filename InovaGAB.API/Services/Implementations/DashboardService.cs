using InovaGAB.API.Data;
using InovaGAB.API.DTOs.Response;
using InovaGAB.API.Models;
using InovaGAB.API.Services.Interfaces;
using MongoDB.Driver;

namespace InovaGAB.API.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly MongoDbContext _context;

    public DashboardService(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResponse> GetDashboardAsync()
    {
        var projectsTask = _context.Projects
            .Find(_ => true)
            .ToListAsync();

        var ideasTask = _context.Ideas
            .Find(idea => !idea.IsDeleted)
            .ToListAsync();

        var usersTask = _context.Users
            .Find(_ => true)
            .ToListAsync();

        var guidelinesTask = _context.StrategicGuidelines
            .Find(_ => true)
            .ToListAsync();

        await Task.WhenAll(
            projectsTask,
            ideasTask,
            usersTask,
            guidelinesTask);

        var projects = await projectsTask;
        var ideas = await ideasTask;
        var users = await usersTask;
        var guidelines = await guidelinesTask;

        var usersById = users.ToDictionary(
            user => user.Id,
            user => user);

        var guidelinesById = guidelines.ToDictionary(
            guideline => guideline.Id,
            guideline => guideline);

        foreach (var project in projects)
        {
            if (usersById.TryGetValue(
                    project.ManagerId,
                    out var manager))
            {
                project.Manager = manager;
            }

            if (project.GuidelineId != null &&
                guidelinesById.TryGetValue(
                    project.GuidelineId,
                    out var guideline))
            {
                project.Guideline = guideline;
            }
        }

        foreach (var idea in ideas)
        {
            if (usersById.TryGetValue(
                    idea.UserId,
                    out var user))
            {
                idea.User = user;
            }
        }

        var now = DateTime.UtcNow;

        var totalInvestment = projects.Sum(
            project => project.Investment);

        var totalFinancialReturn = projects.Sum(
            project => project.FinancialReturn);

        var totalRoi = RoiCalculator.Calculate(
            totalInvestment,
            totalFinancialReturn);

        var productivityAverage = projects.Count > 0
            ? (int)projects.Average(
                project => project.ProductivityGain)
            : 0;

        var activeProjects = projects.Count(project =>
            project.Status == ProjectStatus.InProgress ||
            project.Status == ProjectStatus.Planning);

        var delayedProjects = projects.Count(project =>
            project.Deadline < now &&
            project.Status != ProjectStatus.Completed &&
            project.Status != ProjectStatus.Cancelled);

        var funnel = new IdeaFunnelDto
        {
            TotalSubmitted = ideas.Count,
            UnderReview = ideas.Count(idea =>
                idea.Status == IdeaStatus.UnderReview),
            Approved = ideas.Count(idea =>
                idea.Status == IdeaStatus.Approved),
            Rejected = ideas.Count(idea =>
                idea.Status == IdeaStatus.Rejected),
            ConvertedToProjects = projects.Count(project =>
                project.IdeaId != null)
        };

        var topProjects = projects
            .OrderByDescending(project => project.Roi)
            .Take(3)
            .Select(MapToProjectResponse)
            .ToList();

        var topContributors = users
            .OrderByDescending(user => user.Points)
            .Take(5)
            .Select(user => new RankingItemDto
            {
                UserName = user.Name,
                Division = user.Division,
                Points = user.Points,
                IdeasApproved = ideas.Count(idea =>
                    idea.UserId == user.Id &&
                    idea.Status == IdeaStatus.Approved)
            })
            .ToList();

        var guidelineBreakdown = BuildGuidelineBreakdown(
            projects,
            guidelinesById);

        return new DashboardResponse
        {
            TotalRoi = totalRoi,
            TotalSavings = totalFinancialReturn,
            ProductivityGainAverage =
                productivityAverage,
            ActiveProjects = activeProjects,
            DelayedProjects = delayedProjects,
            IdeaFunnel = funnel,
            TopProjects = topProjects,
            TopContributors = topContributors,
            GuidelineBreakdown = guidelineBreakdown
        };
    }

    public async Task<GuidelineDashboardDto?> GetByGuidelineAsync(
        string guidelineId)
    {
        var reference = await _context.StrategicGuidelines
            .Find(guideline => guideline.Id == guidelineId)
            .FirstOrDefaultAsync();

        if (reference == null)
        {
            return null;
        }

        var rootId = string.IsNullOrEmpty(reference.RootId)
            ? reference.Id
            : reference.RootId;

        var guidelines = await _context.StrategicGuidelines
            .Find(_ => true)
            .ToListAsync();

        var guidelinesById = guidelines.ToDictionary(
            guideline => guideline.Id,
            guideline => guideline);

        var projects = await _context.Projects
            .Find(_ => true)
            .ToListAsync();

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

        var groupProjects = projects
            .Where(project => GetGroupRootId(project) == rootId)
            .ToList();

        var current = guidelines.FirstOrDefault(guideline =>
            guideline.RootId == rootId && guideline.IsCurrent) ??
            reference;

        return BuildGroupDto(rootId, current, groupProjects);
    }

    private static List<GuidelineDashboardDto> BuildGuidelineBreakdown(
        List<Project> projects,
        Dictionary<string, StrategicGuideline> guidelinesById)
    {
        var groups = projects
            .GroupBy(GetGroupRootId);

        var breakdown = new List<GuidelineDashboardDto>();

        foreach (var group in groups)
        {
            var rootId = group.Key;

            StrategicGuideline? current = null;

            if (rootId != null)
            {
                current = guidelinesById.Values.FirstOrDefault(
                    guideline =>
                        guideline.RootId == rootId &&
                        guideline.IsCurrent);
            }

            breakdown.Add(BuildGroupDto(
                rootId,
                current,
                group.ToList()));
        }

        return breakdown
            .OrderByDescending(dto => dto.TotalInvestment)
            .ToList();
    }

    private static GuidelineDashboardDto BuildGroupDto(
        string? rootId,
        StrategicGuideline? current,
        List<Project> groupProjects)
    {
        var now = DateTime.UtcNow;

        var totalInvestment = groupProjects.Sum(
            project => project.Investment);

        var totalFinancialReturn = groupProjects.Sum(
            project => project.FinancialReturn);

        var roi = RoiCalculator.Calculate(
            totalInvestment,
            totalFinancialReturn);

        var productivityAverage = groupProjects.Count > 0
            ? (int)groupProjects.Average(
                project => project.ProductivityGain)
            : 0;

        var activeProjects = groupProjects.Count(project =>
            project.Status == ProjectStatus.InProgress ||
            project.Status == ProjectStatus.Planning);

        var delayedProjects = groupProjects.Count(project =>
            project.Deadline < now &&
            project.Status != ProjectStatus.Completed &&
            project.Status != ProjectStatus.Cancelled);

        return new GuidelineDashboardDto
        {
            GuidelineId = rootId,
            GuidelineTitle = current?.Title ?? "Sem diretriz",
            Category = current?.Category ?? string.Empty,
            Campaign = current?.Campaign ?? string.Empty,
            ProjectCount = groupProjects.Count,
            TotalInvestment = totalInvestment,
            TotalFinancialReturn = totalFinancialReturn,
            Roi = roi,
            ProductivityGainAverage = productivityAverage,
            ActiveProjects = activeProjects,
            DelayedProjects = delayedProjects
        };
    }

    private static string? GetGroupRootId(Project project)
    {
        if (project.Guideline == null)
        {
            return null;
        }

        return string.IsNullOrEmpty(project.Guideline.RootId)
            ? project.Guideline.Id
            : project.Guideline.RootId;
    }

    private static ProjectResponse MapToProjectResponse(
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
