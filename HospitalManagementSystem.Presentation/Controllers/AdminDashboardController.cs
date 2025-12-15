using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Application.IServices.DoctorIServices;
using HospitalManagementSystem.Application.DTOs.AdminDto;
using HospitalManagementSystem.Application.DTOs.HospitalDto.Request_Dto;
using HospitalManagementSystem.Application.DTOs.HospitalDto.Response_Dto;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace HospitalManagementSystem.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly IDoctorAppointmentService _doctorAppointmentService;
        private readonly IHospitalService _hospitalService;
        private readonly AppDbContext _dbContext;

        public AdminDashboardController(IAdminDashboardService adminDashboardService, IDoctorAppointmentService doctorAppointmentService, IHospitalService hospitalService, AppDbContext dbContext)
        {
            _adminDashboardService = adminDashboardService;
            _doctorAppointmentService = doctorAppointmentService;
            _hospitalService = hospitalService;
            _dbContext = dbContext;
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return null;
            return Guid.Parse(userIdClaim);
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
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var hospitalAdmin = await _dbContext.HospitalAdmins
                .Include(ha => ha.Hospital)
                .FirstOrDefaultAsync(ha => ha.UserId == userId.Value && ha.IsActive);

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

        [HttpGet("my-hospital")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<HospitalResponseDto>> GetMyHospital()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var hospitalAdmin = await _dbContext.HospitalAdmins
                .AsNoTracking()
                .FirstOrDefaultAsync(ha => ha.UserId == userId.Value && ha.IsActive);

            if (hospitalAdmin == null)
                return NotFound(new { message = "Admin not assigned to any hospital" });

            var hospital = await _hospitalService.GetByIdAsync(hospitalAdmin.HospitalId);
            if (hospital == null)
                return NotFound(new { message = "Hospital not found" });

            return Ok(hospital);
        }

        [HttpPut("my-hospital")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<HospitalResponseDto>> UpdateMyHospital([FromBody] HospitalRequestDto hospitalDto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var hospitalAdmin = await _dbContext.HospitalAdmins
                .AsNoTracking()
                .FirstOrDefaultAsync(ha => ha.UserId == userId.Value && ha.IsActive);

            if (hospitalAdmin == null)
                return NotFound(new { message = "Admin not assigned to any hospital" });

            var result = await _hospitalService.UpdateHospitalAsync(hospitalAdmin.HospitalId, hospitalDto);
            if (result == null)
                return NotFound(new { message = "Hospital not found" });

            return Ok(result);
        }

        [HttpGet("my-profile")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null) return NotFound(new { message = "User not found" });

            return Ok(new
            {
                userId = user.UserId,
                username = user.Username,
                email = user.Email,
                imageUrl = user.ImageUrl,
                role = user.Role
            });
        }

        [HttpPut("my-profile")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateAdminDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var user = await _dbContext.Users.FindAsync(userId.Value);
            if (user == null) return NotFound(new { message = "User not found" });

            if (user.Role != "Admin" && user.Role != "SuperAdmin")
                return Forbid();

            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
            {
                var emailTaken = await _dbContext.Users.AnyAsync(u => u.Email == dto.Email && u.UserId != user.UserId);
                if (emailTaken) return BadRequest(new { message = "Email is already taken" });
                user.Email = dto.Email;
            }

            if (!string.IsNullOrEmpty(dto.Username))
            {
                user.Username = dto.Username;
            }

            if (!string.IsNullOrEmpty(dto.Password))
            {
                using var hmac = new HMACSHA512();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));
                user.PasswordSalt = hmac.Key;
            }

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully" });
        }
    }
}
