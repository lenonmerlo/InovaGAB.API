using InovaGAB.API.Data;
using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;
using InovaGAB.API.Models;
using InovaGAB.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InovaGAB.API.Services.Implementations;

public class IdeaService : IIdeaService
{
    private readonly AppDbContext _context;

    public IdeaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IdeaResponse> CreateAsync(CreateIdeaRequest request, int userId)
    {
        var idea = new Idea
        {
            Title = request.Title,
            Description = request.Description,
            Division = request.Division,
            EvidenceUrl = request.EvidenceUrl,
            ChallengeId = request.ChallengeId,
            UserId = userId,
            Status = IdeaStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Ideas.Add(idea);
        await _context.SaveChangesAsync();

        await _context.Entry(idea).Reference(i => i.User).LoadAsync();

        return MapToResponse(idea);
    }

    public async Task<List<IdeaResponse>> GetMyIdeasAsync(int userId)
    {
        var ideas = await _context.Ideas
            .Include(i => i.User)
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return ideas.Select(MapToResponse).ToList();
    }

    public async Task<List<IdeaResponse>> GetAllAsync()
    {
        var ideas = await _context.Ideas
            .Include(i => i.User)
            .OrderByDescending(i => i.TotalScore)
            .ToListAsync();

        return ideas.Select(MapToResponse).ToList();
    }

    public async Task<IdeaResponse?> ApproveAsync(int ideaId, int impactScore, int feasibilityScore, int alignmentScore)
    {
        var idea = await _context.Ideas
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Id == ideaId);

        if (idea == null) return null;

        idea.Status = IdeaStatus.Approved;
        idea.ImpactScore = impactScore;
        idea.FeasibilityScore = feasibilityScore;
        idea.AlignmentScore = alignmentScore;
        idea.UpdatedAt = DateTime.UtcNow;

        var user = await _context.Users.FindAsync(idea.UserId);
        if (user != null)
        {
            user.Points += 50;
        }

        await _context.SaveChangesAsync();
        return MapToResponse(idea);
    }

    public async Task<IdeaResponse?> RejectAsync(int ideaId)
    {
        var idea = await _context.Ideas
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Id == ideaId);

        if (idea == null) return null;

        idea.Status = IdeaStatus.Rejected;
        idea.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToResponse(idea);
    }

    private static IdeaResponse MapToResponse(Idea idea) => new()
    {
        Id = idea.Id,
        Title = idea.Title,
        Description = idea.Description,
        Division = idea.Division,
        Status = idea.Status.ToString(),
        ImpactScore = idea.ImpactScore,
        FeasibilityScore = idea.FeasibilityScore,
        AlignmentScore = idea.AlignmentScore,
        TotalScore = idea.TotalScore,
        EvidenceUrl = idea.EvidenceUrl,
        CreatedAt = idea.CreatedAt,
        UserName = idea.User?.Name ?? string.Empty
    };
}