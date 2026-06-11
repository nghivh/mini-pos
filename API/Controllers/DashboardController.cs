using API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken ct)
        {
            var result = await _dashboardService.GetDashboardSummaryAsync(startDate, endDate, ct);
            return Ok(result);
        }
    }
}
