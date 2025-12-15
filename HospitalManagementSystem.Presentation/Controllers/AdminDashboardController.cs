using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Application.IServices.DoctorIServices;
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
        private readonly IDoctorAppointmentService _doctorAppointmentService;
        private readonly AppDbContext _dbContext;

        public AdminDashboardController(IAdminDashboardService adminDashboardService, IDoctorAppointmentService doctorAppointmentService, AppDbContext dbContext)
        {
            _adminDashboardService = adminDashboardService;
            _doctorAppointmentService = doctorAppointmentService;
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

                var appointmentsQuery = _dbContext.DoctorAppointments
                    .Where(a => (a.HospitalId.HasValue && a.HospitalId == hospitalAdmin.HospitalId) || doctorIds.Contains(a.DoctorId));

                var appointmentCount = await appointmentsQuery.CountAsync();

                var patientCount = await appointmentsQuery
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

        [HttpGet("appointments")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAppointments()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);
            var hospitalAdmin = await _dbContext.HospitalAdmins
                .FirstOrDefaultAsync(ha => ha.UserId == userId && ha.IsActive);

            if (hospitalAdmin == null)
                return NotFound(new { message = "Admin not assigned to any hospital" });

            var departmentIds = await _dbContext.Departments
                .Where(d => d.HospitalId == hospitalAdmin.HospitalId)
                .Select(d => d.DepartmentId)
                .ToListAsync();

            var doctorIds = await _dbContext.Doctors
                .Where(d => d.DepartmentId.HasValue && departmentIds.Contains(d.DepartmentId.Value))
                .Select(d => d.DoctorId)
                .ToListAsync();

            var appointments = await _dbContext.DoctorAppointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Hospital)
                .Where(a => (a.HospitalId.HasValue && a.HospitalId == hospitalAdmin.HospitalId) || doctorIds.Contains(a.DoctorId))
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .Select(a => new
                {
                    appointmentId = a.AppointmentId,
                    appointmentDate = a.AppointmentDate,
                    appointmentTime = a.AppointmentTime,
                    appointmentEndTime = a.AppointmentEndTime,
                    appointmentStatus = a.AppointmentStatus,
                    createdDate = a.CreatedDate,
                    durationMinutes = a.DurationMinutes,
                    patientId = a.PatientId,
                    doctorId = a.DoctorId,
                    hospitalId = a.HospitalId,
                    doctorName = a.Doctor != null ? a.Doctor.Name : null,
                    patientName = a.Patient != null ? (a.Patient.FirstName + " " + a.Patient.LastName) : null,
                    hospitalName = a.Hospital != null ? a.Hospital.Name : null,

                    cancellationRequested = a.CancellationRequested,
                    cancellationRequestedAt = a.CancellationRequestedAt,
                    cancellationReason = a.CancellationReason,
                    cancellationApproved = a.CancellationApproved,
                    cancellationApprovedAt = a.CancellationApprovedAt,
                    cancellationApprovedBy = a.CancellationApprovedBy,
                    cancellationApprovalNote = a.CancellationApprovalNote
                })
                .ToListAsync();

            return Ok(appointments);
        }

        public class ApproveCancellationRequest
        {
            public string? Note { get; set; }
        }

        public class RejectCancellationRequest
        {
            public string? Reason { get; set; }
        }

        [HttpPost("appointments/{appointmentId:guid}/approve-cancellation")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveCancellation(Guid appointmentId, [FromBody] ApproveCancellationRequest request)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "User not authenticated" });

            var userId = Guid.Parse(userIdClaim);

            var hospitalAdmin = await _dbContext.HospitalAdmins
                .FirstOrDefaultAsync(ha => ha.UserId == userId && ha.IsActive);

            if (hospitalAdmin == null)
                return NotFound(new { message = "Admin not assigned to any hospital" });

            var departmentIds = await _dbContext.Departments
                .Where(d => d.HospitalId == hospitalAdmin.HospitalId)
                .Select(d => d.DepartmentId)
                .ToListAsync();

            var doctorIds = await _dbContext.Doctors
                .Where(d => d.DepartmentId.HasValue && departmentIds.Contains(d.DepartmentId.Value))
                .Select(d => d.DoctorId)
                .ToListAsync();

            var appointment = await _dbContext.DoctorAppointments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
                return NotFound(new { message = "Appointment not found" });

            var isInAdminsHospital = (appointment.HospitalId.HasValue && appointment.HospitalId == hospitalAdmin.HospitalId) || doctorIds.Contains(appointment.DoctorId);
            if (!isInAdminsHospital)
                return Forbid();

            try
            {
                var success = await _doctorAppointmentService.ApproveCancellationAsync(appointmentId, userId, request?.Note);
                if (!success) return NotFound(new { message = "Appointment not found" });
                return Ok(new { message = "Cancellation approved successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("appointments/{appointmentId:guid}/reject-cancellation")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectCancellation(Guid appointmentId, [FromBody] RejectCancellationRequest request)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "User not authenticated" });

            var userId = Guid.Parse(userIdClaim);

            var hospitalAdmin = await _dbContext.HospitalAdmins
                .FirstOrDefaultAsync(ha => ha.UserId == userId && ha.IsActive);

            if (hospitalAdmin == null)
                return NotFound(new { message = "Admin not assigned to any hospital" });

            var departmentIds = await _dbContext.Departments
                .Where(d => d.HospitalId == hospitalAdmin.HospitalId)
                .Select(d => d.DepartmentId)
                .ToListAsync();

            var doctorIds = await _dbContext.Doctors
                .Where(d => d.DepartmentId.HasValue && departmentIds.Contains(d.DepartmentId.Value))
                .Select(d => d.DoctorId)
                .ToListAsync();

            var appointment = await _dbContext.DoctorAppointments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
                return NotFound(new { message = "Appointment not found" });

            var isInAdminsHospital = (appointment.HospitalId.HasValue && appointment.HospitalId == hospitalAdmin.HospitalId) || doctorIds.Contains(appointment.DoctorId);
            if (!isInAdminsHospital)
                return Forbid();

            try
            {
                var success = await _doctorAppointmentService.RejectCancellationAsync(appointmentId, userId, request?.Reason);
                if (!success) return NotFound(new { message = "Appointment not found" });
                return Ok(new { message = "Cancellation request rejected" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
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
