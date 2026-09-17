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

        var guideline = await ResolveGuidelineAsync(
            request.GuidelineId);

        var idea = new Idea
        {
            Title = request.Title,
            Description = request.Description,
            Division = request.Division,
            EvidenceUrl = request.EvidenceUrl,
            ChallengeId = request.ChallengeId,
            GuidelineId = request.GuidelineId,
            Guideline = guideline,
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
            .Find(idea =>
                idea.UserId == userId && !idea.IsDeleted)
            .SortByDescending(idea => idea.CreatedAt)
            .ToListAsync();

        foreach (var idea in ideas)
        {
            await LoadUserAsync(idea);
        }

        await LoadGuidelinesAsync(ideas);

        return ideas
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<List<IdeaResponse>> GetAllAsync()
    {
        var ideas = await _context.Ideas
            .Find(idea => !idea.IsDeleted)
            .ToListAsync();

        foreach (var idea in ideas)
        {
            await LoadUserAsync(idea);
        }

        await LoadGuidelinesAsync(ideas);

        return ideas
            .OrderByDescending(idea => idea.TotalScore)
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<IdeaResponse?> GetByIdAsync(string id)
    {
        var idea = await FindActiveAsync(id);

        if (idea == null)
        {
            return null;
        }

        await LoadUserAsync(idea);
        await LoadGuidelineAsync(idea);

        return MapToResponse(idea);
    }

    public async Task<IdeaResponse?> UpdateAsync(
        string id,
        UpdateIdeaRequest request,
        string userId)
    {
        var idea = await FindActiveAsync(id);

        if (idea == null)
        {
            return null;
        }

        EnsureOwnership(idea, userId);

        if (idea.Status != IdeaStatus.Submitted)
        {
            throw new InvalidOperationException(
                "A ideia só pode ser editada enquanto estiver com status Submitted.");
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

            idea.ChallengeId = request.ChallengeId;
        }

        if (request.GuidelineId != null)
        {
            idea.Guideline = await ResolveGuidelineAsync(
                request.GuidelineId);
            idea.GuidelineId = request.GuidelineId;
        }

        if (request.Title != null)
        {
            idea.Title = request.Title;
        }

        if (request.Description != null)
        {
            idea.Description = request.Description;
        }

        if (request.Division != null)
        {
            idea.Division = request.Division;
        }

        if (request.EvidenceUrl != null)
        {
            idea.EvidenceUrl = request.EvidenceUrl;
        }

        idea.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _context.Ideas.ReplaceOneAsync(
            existingIdea => existingIdea.Id == id,
            idea);

        if (updateResult.MatchedCount == 0)
        {
            return null;
        }

        await LoadUserAsync(idea);

        return MapToResponse(idea);
    }

    public async Task<bool?> DeleteAsync(
        string id,
        string userId)
    {
        var idea = await FindActiveAsync(id);

        if (idea == null)
        {
            return null;
        }

        EnsureOwnership(idea, userId);

        if (idea.Status != IdeaStatus.Submitted)
        {
            throw new InvalidOperationException(
                "A ideia só pode ser excluída antes de ser avaliada.");
        }

        var update = Builders<Idea>.Update
            .Set(existingIdea => existingIdea.IsDeleted, true)
            .Set(existingIdea => existingIdea.DeletedAt, DateTime.UtcNow);

        var result = await _context.Ideas.UpdateOneAsync(
            existingIdea => existingIdea.Id == id,
            update);

        return result.MatchedCount > 0;
    }

    public async Task<IdeaResponse?> PrioritizeAsync(
        string id,
        string priority)
    {
        var parsedPriority = ParsePriority(priority);

        var idea = await FindActiveAsync(id);

        if (idea == null)
        {
            return null;
        }

        idea.Priority = parsedPriority;
        idea.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _context.Ideas.ReplaceOneAsync(
            existingIdea => existingIdea.Id == id,
            idea);

        if (updateResult.MatchedCount == 0)
        {
            return null;
        }

        await LoadUserAsync(idea);
        await LoadGuidelineAsync(idea);

        return MapToResponse(idea);
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

        var idea = await FindActiveAsync(ideaId);

        if (idea == null)
        {
            return null;
        }

        if (idea.Status == IdeaStatus.Rejected)
        {
            throw new InvalidOperationException(
                "Uma ideia rejeitada não pode ser aprovada diretamente.");
        }

        // reaprovar é permitido para ajustar scores, mas o bônus só é dado uma vez
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
        await LoadGuidelineAsync(idea);

        return MapToResponse(idea);
    }

    public async Task<IdeaResponse?> RejectAsync(
        string ideaId)
    {
        var idea = await FindActiveAsync(ideaId);

        if (idea == null)
        {
            return null;
        }

        if (idea.Status is IdeaStatus.Approved or IdeaStatus.Rejected)
        {
            throw new InvalidOperationException(
                "Somente ideias em Submitted ou UnderReview podem ser rejeitadas.");
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
        await LoadGuidelineAsync(idea);

        return MapToResponse(idea);
    }

    private async Task<Idea?> FindActiveAsync(string id)
    {
        return await _context.Ideas
            .Find(idea => idea.Id == id && !idea.IsDeleted)
            .FirstOrDefaultAsync();
    }

    private static void EnsureOwnership(Idea idea, string userId)
    {
        if (idea.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "Você só pode gerenciar as próprias ideias.");
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

    private async Task LoadGuidelineAsync(Idea idea)
    {
        if (idea.GuidelineId == null)
        {
            return;
        }

        idea.Guideline = await _context.StrategicGuidelines
            .Find(guideline => guideline.Id == idea.GuidelineId)
            .FirstOrDefaultAsync();
    }

    // carrega as diretrizes das ideias em uma única consulta ($in), evita N+1
    private async Task LoadGuidelinesAsync(List<Idea> ideas)
    {
        var guidelineIds = ideas
            .Where(idea => idea.GuidelineId != null)
            .Select(idea => idea.GuidelineId!)
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

        foreach (var idea in ideas)
        {
            if (idea.GuidelineId != null &&
                guidelinesById.TryGetValue(
                    idea.GuidelineId,
                    out var guideline))
            {
                idea.Guideline = guideline;
            }
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

    private static IdeaPriority ParsePriority(string priority)
    {
        if (Enum.TryParse<IdeaPriority>(
                priority,
                ignoreCase: true,
                out var parsedPriority))
        {
            return parsedPriority;
        }

        throw new ArgumentException(
            "Prioridade da ideia inválida.");
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
            Priority = idea.Priority.ToString(),
            ImpactScore = idea.ImpactScore,
            FeasibilityScore = idea.FeasibilityScore,
            AlignmentScore = idea.AlignmentScore,
            TotalScore = idea.TotalScore,
            EvidenceUrl = idea.EvidenceUrl,
            CreatedAt = idea.CreatedAt,
            UserId = idea.UserId,
            UserName = idea.User?.Name ?? string.Empty,
            GuidelineId = idea.GuidelineId,
            Guideline = idea.Guideline == null
                ? null
                : new GuidelineSummaryResponse
                {
                    Id = idea.Guideline.Id,
                    Title = idea.Guideline.Title,
                    Category = idea.Guideline.Category,
                    Campaign = idea.Guideline.Campaign,
                    IsActive = idea.Guideline.IsActive
                }
        };
    }
}
