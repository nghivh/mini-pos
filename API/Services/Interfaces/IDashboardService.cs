using API.Models.DTOs;

namespace API.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryResponse> GetDashboardSummaryAsync(DateTime startDate, DateTime endDate, CancellationToken ct);
    }
}
