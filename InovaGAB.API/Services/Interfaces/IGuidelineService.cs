using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;

namespace InovaGAB.API.Services.Interfaces
{
    public interface IGuidelineService
    {
        Task<GuidelineResponse> CreateAsync(CreateGuidelineRequest request, int userId);
        Task<List<GuidelineResponse>> GetAllAsync();
        Task<GuidelineResponse?> GetByIdAsync(int id);
        Task<GuidelineResponse?> UpdateAsync(int id, CreateGuidelineRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
