using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;

namespace InovaGAB.API.Services.Interfaces
{
    public interface IChallengeService
    {
        Task<ChallengeResponse> CreateAsync(CreateChallengeRequest request, int userId);
        Task<List<ChallengeResponse>> GetAllActiveAsync();
        Task<ChallengeResponse?> GetByIdAsync(int id);
        Task<ChallengeResponse?> UpdateAsync(int id, CreateChallengeRequest request);
    }
}
