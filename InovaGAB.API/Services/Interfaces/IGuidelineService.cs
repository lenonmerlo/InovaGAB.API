using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;

namespace InovaGAB.API.Services.Interfaces
{
    public interface IGuidelineService
    {
        Task<GuidelineResponse> CreateAsync(
            CreateGuidelineRequest request,
            string userId);
        Task<List<GuidelineResponse>> GetAllAsync();
        Task<GuidelineResponse?> GetByIdAsync(string id);
        Task<GuidelineResponse?> UpdateAsync(string id, CreateGuidelineRequest request);
        Task<bool> DeleteAsync(string id);
    }
}
