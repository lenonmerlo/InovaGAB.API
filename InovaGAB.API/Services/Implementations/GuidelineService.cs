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
            Priority = ParsePriority(request.Priority),
            CreatedById = userId,
            CreatedBy = creator,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.StrategicGuidelines
            .InsertOneAsync(guideline);

        return MapToResponse(guideline);
    }

    public async Task<List<GuidelineResponse>> GetAllAsync()
    {
        var guidelines = await _context.StrategicGuidelines
            .Find(guideline => guideline.IsActive)
            .SortByDescending(guideline => guideline.CreatedAt)
            .ToListAsync();

        foreach (var guideline in guidelines)
        {
            await LoadCreatorAsync(guideline);
        }

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

    public async Task<GuidelineResponse?> UpdateAsync(
        string id,
        CreateGuidelineRequest request)
    {
        var guideline = await _context.StrategicGuidelines
            .Find(guideline => guideline.Id == id)
            .FirstOrDefaultAsync();

        if (guideline == null)
        {
            return null;
        }

        guideline.Title = request.Title;
        guideline.Description = request.Description;
        guideline.Category = request.Category;
        guideline.Priority =
            ParsePriority(request.Priority);
        guideline.UpdatedAt = DateTime.UtcNow;

        var updateResult =
            await _context.StrategicGuidelines.ReplaceOneAsync(
                existingGuideline =>
                    existingGuideline.Id == id,
                guideline);

        if (updateResult.MatchedCount == 0)
        {
            return null;
        }

        await LoadCreatorAsync(guideline);

        return MapToResponse(guideline);
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
            Priority = guideline.Priority.ToString(),
            IsActive = guideline.IsActive,
            CreatedAt = guideline.CreatedAt,
            CreatedByName =
                guideline.CreatedBy?.Name ?? string.Empty
        };
    }
}
