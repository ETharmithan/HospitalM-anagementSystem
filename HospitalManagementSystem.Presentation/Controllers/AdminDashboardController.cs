using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly AppDbContext _dbContext;

        public AdminDashboardController(IAdminDashboardService adminDashboardService, AppDbContext dbContext)
        {
            _adminDashboardService = adminDashboardService;
            _dbContext = dbContext;
        }

        [HttpGet("overview")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetOverview()
        {
            // Check if user is Admin (not SuperAdmin)
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            if (userRole == "Admin")
            {
                // Get admin's hospital ID
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized();

                var userId = Guid.Parse(userIdClaim);
                var hospitalAdmin = await _dbContext.HospitalAdmins
                    .FirstOrDefaultAsync(ha => ha.UserId == userId && ha.IsActive);

                if (hospitalAdmin == null)
                    return NotFound(new { message = "Admin not assigned to any hospital" });

                // Get hospital-specific counts
                var departmentIds = await _dbContext.Departments
                    .Where(d => d.HospitalId == hospitalAdmin.HospitalId)
                    .Select(d => d.DepartmentId)
                    .ToListAsync();

                var doctorIds = await _dbContext.Doctors
                    .Where(d => d.DepartmentId.HasValue && departmentIds.Contains(d.DepartmentId.Value))
                    .Select(d => d.DoctorId)
                    .ToListAsync();

                var doctorCount = doctorIds.Count;
                var departmentCount = departmentIds.Count;

                var appointmentCount = await _dbContext.DoctorAppointments
                    .Where(a => doctorIds.Contains(a.DoctorId))
                    .CountAsync();

                var patientCount = await _dbContext.DoctorAppointments
                    .Where(a => doctorIds.Contains(a.DoctorId))
                    .Select(a => a.PatientId)
                    .Distinct()
                    .CountAsync();

                var userCount = await _dbContext.Users.CountAsync();

                return Ok(new
                {
                    totalUsers = userCount,
                    totalDoctors = doctorCount,
                    totalPatients = patientCount,
                    totalAppointments = appointmentCount,
                    totalDepartments = departmentCount
                });
            }
            else
            {
                // SuperAdmin gets all data
                var overview = await _adminDashboardService.GetOverviewAsync(null);
                return Ok(overview);
            }
        }

        [HttpGet("hospital-info")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetHospitalInfo()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);
            var hospitalAdmin = await _dbContext.HospitalAdmins
                .Include(ha => ha.Hospital)
                .FirstOrDefaultAsync(ha => ha.UserId == userId && ha.IsActive);

            if (hospitalAdmin == null)
                return NotFound(new { message = "Admin not assigned to any hospital" });

            return Ok(new
            {
                hospitalId = hospitalAdmin.HospitalId,
                hospitalName = hospitalAdmin.Hospital.Name,
                hospitalAddress = hospitalAdmin.Hospital.Address,
                hospitalCity = hospitalAdmin.Hospital.City,
                hospitalEmail = hospitalAdmin.Hospital.Email,
                hospitalPhone = hospitalAdmin.Hospital.PhoneNumber
            });
        }
    }
}
