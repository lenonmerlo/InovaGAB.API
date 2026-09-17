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

        Task<IdeaResponse?> GetByIdAsync(string id);

        // lança UnauthorizedAccessException (403) se userId não for o
        // autor, InvalidOperationException (400) se status != Submitted
        Task<IdeaResponse?> UpdateAsync(
            string id,
            UpdateIdeaRequest request,
            string userId);

        // mesmas regras de erro do UpdateAsync
        Task<bool?> DeleteAsync(string id, string userId);

        Task<IdeaResponse?> PrioritizeAsync(
            string id,
            string priority);

        Task<IdeaResponse?> ApproveAsync(
            string ideaId,
            int impactScore,
            int feasibilityScore,
            int alignmentScore);

        Task<IdeaResponse?> RejectAsync(string ideaId);
    }
}
