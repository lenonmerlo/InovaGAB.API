using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;

namespace InovaGAB.API.Services.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectResponse> CreateAsync(CreateProjectRequest request, string managerId);
        Task<List<ProjectResponse>> GetAllAsync();
        Task<ProjectResponse?> GetByIdAsync(string id);
        Task<ProjectResponse?> UpdateAsync(string id, UpdateProjectRequest request);
    }
}
