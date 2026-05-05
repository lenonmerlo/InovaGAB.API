using InovaGAB.API.Data;
using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;
using InovaGAB.API.Models;
using InovaGAB.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InovaGAB.API.Services.Implementations;

public class GuidelineService : IGuidelineService
{
    private readonly AppDbContext _context;

    public GuidelineService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GuidelineResponse> CreateAsync(CreateGuidelineRequest request, int userId)
    {
        var guideline = new StrategicGuideline
        {
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            Priority = Enum.Parse<GuidelinePriority>(request.Priority, ignoreCase: true),
            CreatedById = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.StrategicGuidelines.Add(guideline);
        await _context.SaveChangesAsync();

        await _context.Entry(guideline).Reference(g => g.CreatedBy).LoadAsync();

        return MapToResponse(guideline);
    }

    public async Task<List<GuidelineResponse>> GetAllAsync()
    {
        var guidelines = await _context.StrategicGuidelines
            .Include(g => g.CreatedBy)
            .Where(g => g.IsActive)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return guidelines.Select(MapToResponse).ToList();
    }

    public async Task<GuidelineResponse?> GetByIdAsync(int id)
    {
        var guideline = await _context.StrategicGuidelines
            .Include(g => g.CreatedBy)
            .FirstOrDefaultAsync(g => g.Id == id);

        return guideline == null ? null : MapToResponse(guideline);
    }

    public async Task<GuidelineResponse?> UpdateAsync(int id, CreateGuidelineRequest request)
    {
        var guideline = await _context.StrategicGuidelines
            .Include(g => g.CreatedBy)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (guideline == null) return null;

        guideline.Title = request.Title;
        guideline.Description = request.Description;
        guideline.Category = request.Category;
        guideline.Priority = Enum.Parse<GuidelinePriority>(request.Priority, ignoreCase: true);
        guideline.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToResponse(guideline);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var guideline = await _context.StrategicGuidelines.FindAsync(id);
        if (guideline == null) return false;

        guideline.IsActive = false;
        guideline.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    private static GuidelineResponse MapToResponse(StrategicGuideline g) => new()
    {
        Id = g.Id,
        Title = g.Title,
        Description = g.Description,
        Category = g.Category,
        Priority = g.Priority.ToString(),
        IsActive = g.IsActive,
        CreatedAt = g.CreatedAt,
        CreatedByName = g.CreatedBy?.Name ?? string.Empty
    };
}