using InovaGAB.API.Data;
using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;
using InovaGAB.API.Models;
using InovaGAB.API.Services.Interfaces;
using MongoDB.Driver;

namespace InovaGAB.API.Services.Implementations;

public class ChallengeService : IChallengeService
{
    private readonly MongoDbContext _context;

    public ChallengeService(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<ChallengeResponse> CreateAsync(
        CreateChallengeRequest request,
        string userId)
    {
        var creator = await _context.Users
            .Find(user => user.Id == userId)
            .FirstOrDefaultAsync();

        if (creator == null)
        {
            throw new InvalidOperationException(
                "Usuário responsável não encontrado.");
        }

        var challenge = new Challenge
        {
            Title = request.Title,
            Description = request.Description,
            Prize = request.Prize,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = true,
            CreatedById = userId,
            CreatedBy = creator,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Challenges.InsertOneAsync(challenge);

        return MapToResponse(challenge);
    }

    public async Task<List<ChallengeResponse>> GetAllActiveAsync()
    {
        var challenges = await _context.Challenges
            .Find(challenge => challenge.IsActive)
            .SortBy(challenge => challenge.EndDate)
            .ToListAsync();

        foreach (var challenge in challenges)
        {
            await LoadRelationsAsync(challenge);
        }

        return challenges
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<ChallengeResponse?> GetByIdAsync(
        string id)
    {
        var challenge = await _context.Challenges
            .Find(challenge => challenge.Id == id)
            .FirstOrDefaultAsync();

        if (challenge == null)
        {
            return null;
        }

        await LoadRelationsAsync(challenge);

        return MapToResponse(challenge);
    }

    public async Task<ChallengeResponse?> UpdateAsync(
        string id,
        CreateChallengeRequest request)
    {
        var challenge = await _context.Challenges
            .Find(challenge => challenge.Id == id)
            .FirstOrDefaultAsync();

        if (challenge == null)
        {
            return null;
        }

        challenge.Title = request.Title;
        challenge.Description = request.Description;
        challenge.Prize = request.Prize;
        challenge.StartDate = request.StartDate;
        challenge.EndDate = request.EndDate;

        var updateResult = await _context.Challenges.ReplaceOneAsync(
            existingChallenge => existingChallenge.Id == id,
            challenge);

        if (updateResult.MatchedCount == 0)
        {
            return null;
        }

        await LoadRelationsAsync(challenge);

        return MapToResponse(challenge);
    }

    private async Task LoadRelationsAsync(
        Challenge challenge)
    {
        challenge.CreatedBy = await _context.Users
            .Find(user => user.Id == challenge.CreatedById)
            .FirstOrDefaultAsync();

        var ideas = await _context.Ideas
            .Find(idea => idea.ChallengeId == challenge.Id)
            .ToListAsync();

        foreach (var idea in ideas)
        {
            idea.User = await _context.Users
                .Find(user => user.Id == idea.UserId)
                .FirstOrDefaultAsync();
        }

        challenge.Ideas = ideas;
    }

    private static ChallengeResponse MapToResponse(
        Challenge challenge)
    {
        return new ChallengeResponse
        {
            Id = challenge.Id,
            Title = challenge.Title,
            Description = challenge.Description,
            Prize = challenge.Prize,
            StartDate = challenge.StartDate,
            EndDate = challenge.EndDate,
            IsActive = challenge.IsActive,
            DaysRemaining = Math.Max(
                0,
                (int)(challenge.EndDate - DateTime.UtcNow)
                    .TotalDays),
            CreatedByName =
                challenge.CreatedBy?.Name ?? string.Empty,
            TotalIdeas = challenge.Ideas?.Count ?? 0,
            Ideas = challenge.Ideas?
                .Select(idea => new IdeaResponse
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
                    UserName =
                        idea.User?.Name ?? string.Empty
                })
                .ToList() ?? new List<IdeaResponse>()
        };
    }
}
