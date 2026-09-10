using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;

namespace InovaGAB.API.Services.Interfaces
{
    public interface IChallengeService
    {
        Task<ChallengeResponse> CreateAsync(
            CreateChallengeRequest request,
            string userId);

        Task<List<ChallengeResponse>> GetAllActiveAsync();

        Task<ChallengeResponse?> GetByIdAsync(string id);

        Task<ChallengeResponse?> UpdateAsync(
            string id,
            CreateChallengeRequest request);
    }
}
