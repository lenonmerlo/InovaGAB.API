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

        // arquivamento lógico (ver decisão no README); null se não existir
        Task<bool?> ArchiveAsync(string id);
    }
}
