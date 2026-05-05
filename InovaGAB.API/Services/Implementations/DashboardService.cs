using InovaGAB.API.Data;
using InovaGAB.API.DTOs.Response;
using InovaGAB.API.Models;
using InovaGAB.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InovaGAB.API.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResponse> GetDashboardAsync()
    {
        var projects = await _context.Projects
            .Include(p => p.Manager)
            .ToListAsync();

        var ideas = await _context.Ideas
            .Include(i => i.User)
            .ToListAsync();

        var users = await _context.Users.ToListAsync();

        var now = DateTime.UtcNow;

        // KPIs financeiros
        var totalRoi = projects.Sum(p => p.Roi);
        var totalSavings = projects.Sum(p => p.FinancialReturn);
        var productivityAvg = projects.Any()
            ? (int)projects.Average(p => p.ProductivityGain)
            : 0;

        // Projetos
        var activeProjects = projects.Count(p =>
            p.Status == ProjectStatus.InProgress ||
            p.Status == ProjectStatus.Planning);

        var delayedProjects = projects.Count(p =>
            p.Deadline < now &&
            p.Status != ProjectStatus.Completed &&
            p.Status != ProjectStatus.Cancelled);

        // Funil de ideias
        var funnel = new IdeaFunnelDto
        {
            TotalSubmitted = ideas.Count,
            UnderReview = ideas.Count(i => i.Status == IdeaStatus.UnderReview),
            Approved = ideas.Count(i => i.Status == IdeaStatus.Approved),
            Rejected = ideas.Count(i => i.Status == IdeaStatus.Rejected),
            ConvertedToProjects = projects.Count(p => p.IdeaId != null)
        };

        // Top 3 projetos por ROI
        var topProjects = projects
            .OrderByDescending(p => p.Roi)
            .Take(3)
            .Select(p => new ProjectResponse
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Division = p.Division,
                Status = p.Status.ToString(),
                Stage = p.Stage.ToString(),
                Investment = p.Investment,
                FinancialReturn = p.FinancialReturn,
                Roi = p.Roi,
                ProductivityGain = p.ProductivityGain,
                StartDate = p.StartDate,
                Deadline = p.Deadline,
                ProgressPercent = p.ProgressPercent,
                CreatedAt = p.CreatedAt,
                ManagerName = p.Manager?.Name ?? string.Empty,
                IdeaId = p.IdeaId
            }).ToList();

        // Top contribuidores por pontos
        var topContributors = users
            .OrderByDescending(u => u.Points)
            .Take(5)
            .Select(u => new RankingItemDto
            {
                UserName = u.Name,
                Division = u.Division,
                Points = u.Points,
                IdeasApproved = ideas.Count(i =>
                    i.UserId == u.Id &&
                    i.Status == IdeaStatus.Approved)
            }).ToList();

        return new DashboardResponse
        {
            TotalRoi = totalRoi,
            TotalSavings = totalSavings,
            ProductivityGainAverage = productivityAvg,
            ActiveProjects = activeProjects,
            DelayedProjects = delayedProjects,
            IdeaFunnel = funnel,
            TopProjects = topProjects,
            TopContributors = topContributors
        };
    }
}