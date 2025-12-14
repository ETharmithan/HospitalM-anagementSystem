using HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto;
using HospitalManagementSystem.Application.DTOs.AppointmentDto;
using HospitalManagementSystem.Application.IServices.DoctorIServices;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Presentation.Controllers.DoctorControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorAppointmentController : ControllerBase
    {
        private readonly IDoctorAppointmentService _doctorAppointmentService;
        private readonly AppDbContext _dbContext;

        public DoctorAppointmentController(IDoctorAppointmentService doctorAppointmentService, AppDbContext dbContext)
        {
            _doctorAppointmentService = doctorAppointmentService;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _doctorAppointmentService.GetAllAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _doctorAppointmentService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("doctor/{doctorId:guid}")]
        public async Task<IActionResult> GetByDoctorId(Guid doctorId)
        {
            var result = await _doctorAppointmentService.GetByDoctorIdAsync(doctorId);
            return Ok(result);
        }

        [HttpGet("patient/{patientId:guid}")]
        public async Task<IActionResult> GetByPatientId(Guid patientId)
        {
            var result = await _doctorAppointmentService.GetByPatientIdAsync(patientId);
            return Ok(result);
        }

        [HttpGet("available-slots/{doctorId:guid}")]
        public async Task<IActionResult> GetAvailableSlots(Guid doctorId, [FromQuery] DateTime date, [FromQuery] Guid? hospitalId = null)
        {
            var result = await _doctorAppointmentService.GetAvailableSlotsAsync(doctorId, date, hospitalId);
            return Ok(result);
        }

        [HttpGet("fully-booked-dates/{doctorId:guid}")]
        public async Task<IActionResult> GetFullyBookedDates(
            Guid doctorId, 
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate, 
            [FromQuery] Guid? hospitalId = null)
        {
            var result = await _doctorAppointmentService.GetFullyBookedDatesAsync(doctorId, startDate, endDate, hospitalId);
            return Ok(result);
        }

        [HttpGet("check-availability/{doctorId:guid}")]
        public async Task<IActionResult> CheckSlotAvailability(
            Guid doctorId, 
            [FromQuery] DateTime date, 
            [FromQuery] string time)
        {
            var isAvailable = await _doctorAppointmentService.IsSlotAvailableAsync(doctorId, date, time);
            return Ok(new { isAvailable });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(DoctorAppointmentRequestDto doctorAppointmentRequestDto)
        {
            try
            {
                var result = await _doctorAppointmentService.CreateAsync(doctorAppointmentRequestDto);
                return CreatedAtAction(nameof(GetById), new { id = result.AppointmentId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, DoctorAppointmentRequestDto doctorAppointmentRequestDto)
        {
            try
            {
                var result = await _doctorAppointmentService.UpdateAsync(id, doctorAppointmentRequestDto);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("cancel/{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelAppointmentRequest? request = null)
        {
            var success = await _doctorAppointmentService.CancelAppointmentAsync(id, request?.Reason);
            if (!success) return NotFound();
            return Ok(new { message = "Appointment cancelled successfully" });
        }

        [HttpPost("request-cancellation/{id:guid}")]
        [Authorize]
        public async Task<IActionResult> RequestCancellation(Guid id, [FromBody] CancellationRequestDto request)
        {
            try
            {
                var success = await _doctorAppointmentService.RequestCancellationAsync(id, request.CancellationReason);
                if (!success) return NotFound();
                return Ok(new { message = "Cancellation request submitted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("approve-cancellation/{id:guid}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> ApproveCancellation(Guid id, [FromBody] ApproveCancellationDto request)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "User not authenticated" });

                var approvedBy = Guid.Parse(userIdClaim);
                var success = await _doctorAppointmentService.ApproveCancellationAsync(id, approvedBy, request.Note);
                if (!success) return NotFound();
                return Ok(new { message = "Cancellation approved successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("reject-cancellation/{id:guid}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> RejectCancellation(Guid id, [FromBody] RejectCancellationDto request)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "User not authenticated" });

                var rejectedBy = Guid.Parse(userIdClaim);
                var success = await _doctorAppointmentService.RejectCancellationAsync(id, rejectedBy, request.Reason);
                if (!success) return NotFound();
                return Ok(new { message = "Cancellation request rejected" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public class CancelAppointmentRequest
        {
            public string? Reason { get; set; }
        }

        public class CancellationRequestDto
        {
            public Guid AppointmentId { get; set; }
            public string CancellationReason { get; set; } = string.Empty;
        }

        public class ApproveCancellationDto
        {
            public string? Note { get; set; }
        }

        public class RejectCancellationDto
        {
            public string? Reason { get; set; }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _doctorAppointmentService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        // Cancellation Workflow Endpoints
        
        /// <summary>
        /// Patient requests cancellation (must be at least 3 hours before appointment)
        /// </summary>
        [HttpPost("request-cancellation")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> RequestCancellation([FromBody] CancellationRequestDto request)
        {
            try
            {
                var appointment = await _dbContext.DoctorAppointments
                    .FirstOrDefaultAsync(a => a.AppointmentId == request.AppointmentId);

                if (appointment == null)
                    return NotFound(new { message = "Appointment not found" });

                // Check if appointment is already cancelled
                if (appointment.AppointmentStatus == "Cancelled")
                    return BadRequest(new { message = "Appointment is already cancelled" });

                // Check if cancellation already requested
                if (appointment.CancellationRequested)
                    return BadRequest(new { message = "Cancellation request already submitted" });

                // Validate 3-hour window
                var appointmentDateTime = appointment.AppointmentDate.Date.Add(TimeSpan.Parse(appointment.AppointmentTime));
                var hoursUntilAppointment = (appointmentDateTime - DateTime.UtcNow).TotalHours;

                if (hoursUntilAppointment < 3)
                    return BadRequest(new { message = "Cancellation requests must be made at least 3 hours before the appointment time" });

                // Update appointment with cancellation request
                appointment.CancellationRequested = true;
                appointment.CancellationRequestedAt = DateTime.UtcNow;
                appointment.CancellationReason = request.CancellationReason;

                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Cancellation request submitted successfully. Awaiting approval from doctor or admin." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Doctor or Admin approves/rejects cancellation request
        /// </summary>
        [HttpPost("approve-cancellation")]
        [Authorize(Roles = "Doctor,Admin,SuperAdmin")]
        public async Task<IActionResult> ApproveCancellation([FromBody] CancellationApprovalDto approval)
        {
            try
            {
                var appointment = await _dbContext.DoctorAppointments
                    .FirstOrDefaultAsync(a => a.AppointmentId == approval.AppointmentId);

                if (appointment == null)
                    return NotFound(new { message = "Appointment not found" });

                if (!appointment.CancellationRequested)
                    return BadRequest(new { message = "No cancellation request found for this appointment" });

                if (appointment.CancellationApproved)
                    return BadRequest(new { message = "Cancellation request already processed" });

                // Get current user ID from claims
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized();

                appointment.CancellationApproved = approval.Approved;
                appointment.CancellationApprovedAt = DateTime.UtcNow;
                appointment.CancellationApprovedBy = Guid.Parse(userIdClaim);
                appointment.CancellationApprovalNote = approval.ApprovalNote;

                if (approval.Approved)
                {
                    appointment.AppointmentStatus = "Cancelled";
                }
                else
                {
                    // Reset cancellation request if rejected
                    appointment.CancellationRequested = false;
                    appointment.CancellationRequestedAt = null;
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new { 
                    message = approval.Approved 
                        ? "Cancellation approved. Appointment has been cancelled." 
                        : "Cancellation request rejected.",
                    approved = approval.Approved
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Get pending cancellation requests (for doctors and admins)
        /// </summary>
        [HttpGet("pending-cancellations")]
        [Authorize(Roles = "Doctor,Admin,SuperAdmin")]
        public async Task<IActionResult> GetPendingCancellations([FromQuery] Guid? doctorId = null)
        {
            try
            {
                var query = _dbContext.DoctorAppointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Where(a => a.CancellationRequested && !a.CancellationApproved);

                if (doctorId.HasValue)
                {
                    query = query.Where(a => a.DoctorId == doctorId.Value);
                }

                var pendingCancellations = await query
                    .OrderBy(a => a.CancellationRequestedAt)
                    .Select(a => new
                    {
                        a.AppointmentId,
                        a.AppointmentDate,
                        a.AppointmentTime,
                        a.CancellationReason,
                        a.CancellationRequestedAt,
                        PatientName = a.Patient != null ? a.Patient.FirstName + " " + a.Patient.LastName : "Unknown",
                        DoctorName = a.Doctor.Name,
                        a.DoctorId,
                        a.PatientId
                    })
                    .ToListAsync();

                return Ok(pendingCancellations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Search appointments by flexible criteria
        /// </summary>
        [HttpPost("search")]
        public async Task<IActionResult> SearchAppointments([FromBody] AppointmentSearchDto searchDto)
        {
            try
            {
                var query = _dbContext.DoctorAppointments
                    .Include(a => a.Doctor)
                        .ThenInclude(d => d.Department)
                    .Include(a => a.Hospital)
                    .Where(a => a.AppointmentStatus != "Cancelled")
                    .AsQueryable();

                // Filter by doctor
                if (searchDto.DoctorId.HasValue)
                {
                    query = query.Where(a => a.DoctorId == searchDto.DoctorId.Value);
                }

                // Filter by hospital
                if (searchDto.HospitalId.HasValue)
                {
                    query = query.Where(a => a.HospitalId == searchDto.HospitalId.Value);
                }

                // Filter by date
                if (searchDto.PreferredDate.HasValue)
                {
                    query = query.Where(a => a.AppointmentDate.Date == searchDto.PreferredDate.Value.Date);
                }

                // Filter by doctor name
                if (!string.IsNullOrEmpty(searchDto.DoctorName))
                {
                    query = query.Where(a => a.Doctor.Name.Contains(searchDto.DoctorName));
                }

                // Filter by specialization (using department name)
                if (!string.IsNullOrEmpty(searchDto.Specialization))
                {
                    query = query.Where(a => a.Doctor.Department != null && a.Doctor.Department.Name.Contains(searchDto.Specialization));
                }

                var results = await query
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentTime)
                    .Select(a => new
                    {
                        a.AppointmentId,
                        a.AppointmentDate,
                        a.AppointmentTime,
                        a.AppointmentStatus,
                        DoctorName = a.Doctor.Name,
                        DoctorQualification = a.Doctor.Qualification,
                        DepartmentName = a.Doctor.Department != null ? a.Doctor.Department.Name : null,
                        HospitalName = a.Hospital != null ? a.Hospital.Name : null,
                        a.DoctorId,
                        a.HospitalId
                    })
                    .ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Get available doctors by search criteria
        /// </summary>
        [HttpPost("search-available-doctors")]
        public async Task<IActionResult> SearchAvailableDoctors([FromBody] AppointmentSearchDto searchDto)
        {
            try
            {
                var query = _dbContext.Doctors
                    .Include(d => d.Department)
                    .Include(d => d.DoctorSchedules)
                    .Where(d => d.Status == "Active")
                    .AsQueryable();

                // Filter by hospital
                if (searchDto.HospitalId.HasValue)
                {
                    query = query.Where(d => d.DoctorSchedules.Any(s => s.HospitalId == searchDto.HospitalId.Value));
                }

                // Filter by specialization (using department name)
                if (!string.IsNullOrEmpty(searchDto.Specialization))
                {
                    query = query.Where(d => d.Department != null && d.Department.Name.Contains(searchDto.Specialization));
                }

                // Filter by doctor name
                if (!string.IsNullOrEmpty(searchDto.DoctorName))
                {
                    query = query.Where(d => d.Name.Contains(searchDto.DoctorName));
                }

                var doctors = await query
                    .Select(d => new
                    {
                        d.DoctorId,
                        d.Name,
                        d.Qualification,
                        DepartmentName = d.Department != null ? d.Department.Name : null,
                        d.Email,
                        d.Phone,
                        Hospitals = d.DoctorSchedules
                            .Where(s => s.Hospital != null)
                            .Select(s => new { s.HospitalId, HospitalName = s.Hospital!.Name })
                            .Distinct()
                            .ToList()
                    })
                    .ToListAsync();

                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }
    }
}
