using InovaGAB.API.Data;
using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;
using InovaGAB.API.Models;
using InovaGAB.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InovaGAB.API.Services.Implementations;

public class ChallengeService : IChallengeService
{
    private readonly AppDbContext _context;

    public ChallengeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ChallengeResponse> CreateAsync(CreateChallengeRequest request, int userId)
    {
        var challenge = new Challenge
        {
            Title = request.Title,
            Description = request.Description,
            Prize = request.Prize,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = true,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Challenges.Add(challenge);
        await _context.SaveChangesAsync();

        await _context.Entry(challenge).Reference(c => c.CreatedBy).LoadAsync();

        return MapToResponse(challenge);
    }

    public async Task<List<ChallengeResponse>> GetAllActiveAsync()
    {
        var challenges = await _context.Challenges
            .Include(c => c.CreatedBy)
            .Include(c => c.Ideas)
                .ThenInclude(i => i.User)
            .Where(c => c.IsActive)
            .OrderBy(c => c.EndDate)
            .ToListAsync();

        return challenges.Select(MapToResponse).ToList();
    }

    public async Task<ChallengeResponse?> GetByIdAsync(int id)
    {
        var challenge = await _context.Challenges
            .Include(c => c.CreatedBy)
            .Include(c => c.Ideas)
                .ThenInclude(i => i.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        return challenge == null ? null : MapToResponse(challenge);
    }

    public async Task<ChallengeResponse?> UpdateAsync(int id, CreateChallengeRequest request)
    {
        var challenge = await _context.Challenges
            .Include(c => c.CreatedBy)
            .Include(c => c.Ideas)
                .ThenInclude(i => i.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (challenge == null) return null;

        challenge.Title = request.Title;
        challenge.Description = request.Description;
        challenge.Prize = request.Prize;
        challenge.StartDate = request.StartDate;
        challenge.EndDate = request.EndDate;

        await _context.SaveChangesAsync();
        return MapToResponse(challenge);
    }

    private static ChallengeResponse MapToResponse(Challenge challenge) => new()
    {
        Id = challenge.Id,
        Title = challenge.Title,
        Description = challenge.Description,
        Prize = challenge.Prize,
        StartDate = challenge.StartDate,
        EndDate = challenge.EndDate,
        IsActive = challenge.IsActive,
        DaysRemaining = Math.Max(0, (int)(challenge.EndDate - DateTime.UtcNow).TotalDays),
        CreatedByName = challenge.CreatedBy?.Name ?? string.Empty,
        TotalIdeas = challenge.Ideas?.Count ?? 0,
        Ideas = challenge.Ideas?.Select(i => new IdeaResponse
        {
            Id = i.Id,
            Title = i.Title,
            Description = i.Description,
            Division = i.Division,
            Status = i.Status.ToString(),
            ImpactScore = i.ImpactScore,
            FeasibilityScore = i.FeasibilityScore,
            AlignmentScore = i.AlignmentScore,
            TotalScore = i.TotalScore,
            EvidenceUrl = i.EvidenceUrl,
            CreatedAt = i.CreatedAt,
            UserName = i.User?.Name ?? string.Empty
        }).ToList() ?? new()
    };
}