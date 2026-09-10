using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;

namespace InovaGAB.API.Services.Interfaces
{
    public interface IIdeaService
    {
        Task<IdeaResponse> CreateAsync(
            CreateIdeaRequest request,
            string userId);

        Task<List<IdeaResponse>> GetMyIdeasAsync(string userId);

        Task<List<IdeaResponse>> GetAllAsync();

        Task<IdeaResponse?> ApproveAsync(
            string ideaId,
            int impactScore,
            int feasibilityScore,
            int alignmentScore);

        Task<IdeaResponse?> RejectAsync(string ideaId);
    }
}
