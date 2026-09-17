using InovaGAB.API.Data;
using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;
using InovaGAB.API.Models;
using InovaGAB.API.Services.Interfaces;
using MongoDB.Driver;

namespace InovaGAB.API.Services.Implementations;

public class GuidelineService : IGuidelineService
{
    private readonly MongoDbContext _context;

    public GuidelineService(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<GuidelineResponse> CreateAsync(
        CreateGuidelineRequest request,
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

        var guideline = new StrategicGuideline
        {
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            Campaign = request.Campaign,
            Priority = ParsePriority(request.Priority),
            CreatedById = userId,
            CreatedBy = creator,
            IsActive = true,
            IsCurrent = true,
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };

        guideline.RootId = guideline.Id;

        await _context.StrategicGuidelines
            .InsertOneAsync(guideline);

        return MapToResponse(guideline);
    }

    public async Task<List<GuidelineResponse>> GetAllAsync()
    {
        // lista só a versão vigente de cada linha de histórico
        var guidelines = await _context.StrategicGuidelines
            .Find(guideline =>
                guideline.IsActive && guideline.IsCurrent)
            .SortByDescending(guideline => guideline.CreatedAt)
            .ToListAsync();

        await LoadCreatorsAsync(guidelines);

        return guidelines
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<GuidelineResponse?> GetByIdAsync(
        string id)
    {
        var guideline = await _context.StrategicGuidelines
            .Find(guideline => guideline.Id == id)
            .FirstOrDefaultAsync();

        if (guideline == null)
        {
            return null;
        }

        await LoadCreatorAsync(guideline);

        return MapToResponse(guideline);
    }

    public async Task<List<GuidelineResponse>?> GetHistoryAsync(
        string id)
    {
        var reference = await _context.StrategicGuidelines
            .Find(guideline => guideline.Id == id)
            .FirstOrDefaultAsync();

        if (reference == null)
        {
            return null;
        }

        var rootId = GetEffectiveRootId(reference);

        var history = await _context.StrategicGuidelines
            .Find(guideline =>
                guideline.RootId == rootId || guideline.Id == rootId)
            .SortByDescending(guideline => guideline.Version)
            .ToListAsync();

        await LoadCreatorsAsync(history);

        return history
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<GuidelineResponse?> UpdateAsync(
        string id,
        CreateGuidelineRequest request,
        string userId)
    {
        var current = await _context.StrategicGuidelines
            .Find(guideline => guideline.Id == id)
            .FirstOrDefaultAsync();

        if (current == null)
        {
            return null;
        }

        var editor = await _context.Users
            .Find(user => user.Id == userId)
            .FirstOrDefaultAsync();

        if (editor == null)
        {
            throw new InvalidOperationException(
                "Usuário responsável não encontrado.");
        }

        // cria uma nova versão vigente em vez de sobrescrever a atual,
        // preservando o histórico
        var newVersion = new StrategicGuideline
        {
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            Campaign = request.Campaign,
            Priority = ParsePriority(request.Priority),
            IsActive = true,
            IsCurrent = true,
            Version = current.Version + 1,
            RootId = GetEffectiveRootId(current),
            PreviousVersionId = current.Id,
            CreatedById = userId,
            CreatedBy = editor,
            CreatedAt = DateTime.UtcNow
        };

        // rebaixa a versão atual antes de inserir a nova: o índice único
        // (rootId + isCurrent) não permite duas vigentes ao mesmo tempo
        var supersede = Builders<StrategicGuideline>.Update
            .Set(guideline => guideline.IsCurrent, false)
            .Set(guideline => guideline.UpdatedAt, DateTime.UtcNow);

        await _context.StrategicGuidelines.UpdateOneAsync(
            guideline => guideline.Id == current.Id,
            supersede);

        await _context.StrategicGuidelines
            .InsertOneAsync(newVersion);

        return MapToResponse(newVersion);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var update = Builders<StrategicGuideline>.Update
            .Set(guideline => guideline.IsActive, false)
            .Set(
                guideline => guideline.UpdatedAt,
                DateTime.UtcNow);

        var result =
            await _context.StrategicGuidelines.UpdateOneAsync(
                guideline => guideline.Id == id,
                update);

        return result.MatchedCount > 0;
    }

    private static string GetEffectiveRootId(
        StrategicGuideline guideline)
    {
        return string.IsNullOrEmpty(guideline.RootId)
            ? guideline.Id
            : guideline.RootId;
    }

    private async Task LoadCreatorAsync(
        StrategicGuideline guideline)
    {
        var creator = await _context.Users
            .Find(user => user.Id == guideline.CreatedById)
            .FirstOrDefaultAsync();

        if (creator != null)
        {
            guideline.CreatedBy = creator;
        }
    }

    private async Task LoadCreatorsAsync(
        List<StrategicGuideline> guidelines)
    {
        var creatorIds = guidelines
            .Select(guideline => guideline.CreatedById)
            .Distinct()
            .ToList();

        if (creatorIds.Count == 0)
        {
            return;
        }

        var creators = await _context.Users
            .Find(user => creatorIds.Contains(user.Id))
            .ToListAsync();

        var creatorsById = creators
            .ToDictionary(user => user.Id);

        foreach (var guideline in guidelines)
        {
            if (creatorsById.TryGetValue(
                    guideline.CreatedById,
                    out var creator))
            {
                guideline.CreatedBy = creator;
            }
        }
    }

    private static GuidelinePriority ParsePriority(
        string priority)
    {
        if (Enum.TryParse<GuidelinePriority>(
                priority,
                ignoreCase: true,
                out var parsedPriority))
        {
            return parsedPriority;
        }

        throw new ArgumentException(
            "Prioridade da diretriz inválida.");
    }

    private static GuidelineResponse MapToResponse(
        StrategicGuideline guideline)
    {
        return new GuidelineResponse
        {
            Id = guideline.Id,
            Title = guideline.Title,
            Description = guideline.Description,
            Category = guideline.Category,
            Campaign = guideline.Campaign,
            Priority = guideline.Priority.ToString(),
            IsActive = guideline.IsActive,
            IsCurrent = guideline.IsCurrent,
            Version = guideline.Version,
            RootId = GetEffectiveRootId(guideline),
            PreviousVersionId = guideline.PreviousVersionId,
            CreatedAt = guideline.CreatedAt,
            CreatedByName =
                guideline.CreatedBy?.Name ?? string.Empty
        };
    }
}
