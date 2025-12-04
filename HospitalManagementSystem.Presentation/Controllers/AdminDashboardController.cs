using HospitalManagementSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public AdminDashboardController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        [HttpGet("overview")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetOverview()
        {
            var overview = await _adminDashboardService.GetOverviewAsync();
            return Ok(overview);
        }
    }
}
