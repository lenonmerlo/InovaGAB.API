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
            .Find(_ => true)
            .ToListAsync();

        var usersTask = _context.Users
            .Find(_ => true)
            .ToListAsync();

        await Task.WhenAll(
            projectsTask,
            ideasTask,
            usersTask);

        var projects = await projectsTask;
        var ideas = await ideasTask;
        var users = await usersTask;

        var usersById = users.ToDictionary(
            user => user.Id,
            user => user);

        foreach (var project in projects)
        {
            if (usersById.TryGetValue(
                    project.ManagerId,
                    out var manager))
            {
                project.Manager = manager;
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

        var totalRoi = totalInvestment > 0
            ? (totalFinancialReturn - totalInvestment)
              / totalInvestment * 100
            : 0;

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
            .Select(project => new ProjectResponse
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
            })
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
            TopContributors = topContributors
        };
    }
}
