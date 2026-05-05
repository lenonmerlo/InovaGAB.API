using InovaGAB.API.DTOs.Response;

namespace InovaGAB.API.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardResponse> GetDashboardAsync();
    }
}
