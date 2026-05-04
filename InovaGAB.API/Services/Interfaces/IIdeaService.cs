using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;

namespace InovaGAB.API.Services.Interfaces
{
    public interface IIdeaService
    {
        Task<IdeaResponse> CreateAsync(CreateIdeaRequest request, int userId);
        Task<List<IdeaResponse>> GetMyIdeasAsync(int userId);
        Task<List<IdeaResponse>> GetAllAsync();
        Task<IdeaResponse?> ApproveAsync(int ideaId, int impactScore, int feasibilityScore, int alignmentScore);
        Task<IdeaResponse?> RejectAsync(int ideaId);
    }
}
