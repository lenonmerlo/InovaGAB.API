using InovaGAB.API.Data;
using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;
using InovaGAB.API.Models;
using InovaGAB.API.Services.Interfaces;
using MongoDB.Driver;

namespace InovaGAB.API.Services.Implementations;

public class IdeaService : IIdeaService
{
    private readonly MongoDbContext _context;

    public IdeaService(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<IdeaResponse> CreateAsync(
        CreateIdeaRequest request,
        string userId)
    {
        var user = await _context.Users
            .Find(user => user.Id == userId)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            throw new InvalidOperationException(
                "Usuário responsável não encontrado.");
        }

        if (request.ChallengeId != null)
        {
            var challengeExists = await _context.Challenges
                .Find(challenge =>
                    challenge.Id == request.ChallengeId &&
                    challenge.IsActive)
                .AnyAsync();

            if (!challengeExists)
            {
                throw new InvalidOperationException(
                    "Desafio não encontrado ou inativo.");
            }
        }

        var idea = new Idea
        {
            Title = request.Title,
            Description = request.Description,
            Division = request.Division,
            EvidenceUrl = request.EvidenceUrl,
            ChallengeId = request.ChallengeId,
            UserId = userId,
            User = user,
            Status = IdeaStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Ideas.InsertOneAsync(idea);

        return MapToResponse(idea);
    }

    public async Task<List<IdeaResponse>> GetMyIdeasAsync(
        string userId)
    {
        var ideas = await _context.Ideas
            .Find(idea => idea.UserId == userId)
            .SortByDescending(idea => idea.CreatedAt)
            .ToListAsync();

        foreach (var idea in ideas)
        {
            await LoadUserAsync(idea);
        }

        return ideas
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<List<IdeaResponse>> GetAllAsync()
    {
        var ideas = await _context.Ideas
            .Find(_ => true)
            .ToListAsync();

        foreach (var idea in ideas)
        {
            await LoadUserAsync(idea);
        }

        return ideas
            .OrderByDescending(idea => idea.TotalScore)
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<IdeaResponse?> ApproveAsync(
        string ideaId,
        int impactScore,
        int feasibilityScore,
        int alignmentScore)
    {
        ValidateScore(impactScore, nameof(impactScore));
        ValidateScore(
            feasibilityScore,
            nameof(feasibilityScore));
        ValidateScore(
            alignmentScore,
            nameof(alignmentScore));

        var idea = await _context.Ideas
            .Find(idea => idea.Id == ideaId)
            .FirstOrDefaultAsync();

        if (idea == null)
        {
            return null;
        }

        var wasAlreadyApproved =
            idea.Status == IdeaStatus.Approved;

        idea.Status = IdeaStatus.Approved;
        idea.ImpactScore = impactScore;
        idea.FeasibilityScore = feasibilityScore;
        idea.AlignmentScore = alignmentScore;
        idea.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _context.Ideas.ReplaceOneAsync(
            existingIdea => existingIdea.Id == ideaId,
            idea);

        if (updateResult.MatchedCount == 0)
        {
            return null;
        }

        if (!wasAlreadyApproved)
        {
            var pointsUpdate = Builders<User>.Update
                .Inc(user => user.Points, 50);

            await _context.Users.UpdateOneAsync(
                user => user.Id == idea.UserId,
                pointsUpdate);
        }

        await LoadUserAsync(idea);

        return MapToResponse(idea);
    }

    public async Task<IdeaResponse?> RejectAsync(
        string ideaId)
    {
        var idea = await _context.Ideas
            .Find(idea => idea.Id == ideaId)
            .FirstOrDefaultAsync();

        if (idea == null)
        {
            return null;
        }

        idea.Status = IdeaStatus.Rejected;
        idea.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _context.Ideas.ReplaceOneAsync(
            existingIdea => existingIdea.Id == ideaId,
            idea);

        if (updateResult.MatchedCount == 0)
        {
            return null;
        }

        await LoadUserAsync(idea);

        return MapToResponse(idea);
    }

    private async Task LoadUserAsync(Idea idea)
    {
        var user = await _context.Users
            .Find(user => user.Id == idea.UserId)
            .FirstOrDefaultAsync();

        if (user != null)
        {
            idea.User = user;
        }
    }

    private static void ValidateScore(
        int score,
        string parameterName)
    {
        if (score is < 0 or > 10)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                "A pontuação deve estar entre 0 e 10.");
        }
    }

    private static IdeaResponse MapToResponse(Idea idea)
    {
        return new IdeaResponse
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
}
