using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;

namespace InovaGAB.API.Services.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectResponse> CreateAsync(CreateProjectRequest request, int managerId);
        Task<List<ProjectResponse>> GetAllAsync();
        Task<ProjectResponse?> GetByIdAsync(int id);
        Task<ProjectResponse?> UpdateAsync(int id, UpdateProjectRequest request);
    }
}
