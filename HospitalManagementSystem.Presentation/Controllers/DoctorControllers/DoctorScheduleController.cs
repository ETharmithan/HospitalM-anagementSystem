using HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto;
using HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto;
using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Application.IServices.DoctorIServices;
using HospitalManagementSystem.Domain.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalManagementSystem.Presentation.Controllers.DoctorControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorScheduleController : ControllerBase
    {
        private readonly IDoctorScheduleService _doctorScheduleService;
        private readonly IDoctorService _doctorService;
        private readonly IHospitalService _hospitalService;
        private readonly IDoctorRepository _doctorRepository;

        public DoctorScheduleController(
            IDoctorScheduleService doctorScheduleService,
            IDoctorService doctorService,
            IHospitalService hospitalService,
            IDoctorRepository doctorRepository)
        {
            _doctorScheduleService = doctorScheduleService;
            _doctorService = doctorService;
            _hospitalService = hospitalService;
            _doctorRepository = doctorRepository;
        }

        // Get doctor ID by email (for current logged-in doctor)
        [HttpGet("my-doctor-id")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<object>> GetMyDoctorId()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                    return Unauthorized(new { message = "Email not found in token" });

                var doctor = await _doctorRepository.GetByEmailAsync(email);
                if (doctor == null)
                    return NotFound(new { message = "Doctor profile not found" });

                return Ok(new { doctorId = doctor.DoctorId, doctorName = doctor.Name });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Get schedules for current logged-in doctor
        [HttpGet("my-schedules")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<IEnumerable<DoctorScheduleResponseDto>>> GetMySchedules()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                    return Unauthorized(new { message = "Email not found in token" });

                var doctor = await _doctorRepository.GetByEmailAsync(email);
                if (doctor == null)
                    return NotFound(new { message = "Doctor profile not found" });

                var schedules = await _doctorScheduleService.GetByDoctorIdAsync(doctor.DoctorId);
                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Get all schedules for a specific doctor
        [HttpGet("doctor/{doctorId}")]
        public async Task<ActionResult<IEnumerable<DoctorScheduleResponseDto>>> GetByDoctorId(Guid doctorId)
        {
            try
            {
                var schedules = await _doctorScheduleService.GetByDoctorIdAsync(doctorId);
                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Debug endpoint: Get schedules for a specific date
        [HttpGet("doctor/{doctorId}/date/{date}")]
        public async Task<ActionResult<object>> GetByDoctorIdAndDate(Guid doctorId, DateTime date)
        {
            try
            {
                var schedules = await _doctorScheduleService.GetByDoctorIdAsync(doctorId);
                var dateOnly = date.Date;
                var dayOfWeek = date.DayOfWeek.ToString();
                
                var specificDateSchedule = schedules.FirstOrDefault(s => 
                    s.ScheduleDate.HasValue && s.ScheduleDate.Value.Date == dateOnly);
                var weeklySchedule = schedules.FirstOrDefault(s => 
                    !string.IsNullOrEmpty(s.DayOfWeek) && 
                    s.DayOfWeek.Equals(dayOfWeek, StringComparison.OrdinalIgnoreCase));
                
                return Ok(new
                {
                    requestedDate = dateOnly,
                    dayOfWeek = dayOfWeek,
                    allSchedules = schedules,
                    specificDateSchedule = specificDateSchedule,
                    weeklySchedule = weeklySchedule,
                    hasScheduleForDate = specificDateSchedule != null || weeklySchedule != null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Get a specific schedule by ID
        [HttpGet("{scheduleId}")]
        public async Task<ActionResult<DoctorScheduleResponseDto>> GetById(Guid scheduleId)
        {
            try
            {
                var schedule = await _doctorScheduleService.GetByIdAsync(scheduleId);
                if (schedule == null)
                    return NotFound(new { message = "Schedule not found" });

                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Create a new schedule for the current logged-in doctor
        [HttpPost]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult<DoctorScheduleResponseDto>> Create([FromBody] DoctorScheduleRequestDto scheduleDto)
        {
            try
            {
                // Get doctor from email in token (for Doctor role)
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(email))
                {
                    var doctorByEmail = await _doctorRepository.GetByEmailAsync(email);
                    if (doctorByEmail != null)
                    {
                        // Override the doctorId with the actual doctor ID
                        scheduleDto.DoctorId = doctorByEmail.DoctorId;
                    }
                }

                // Verify doctor exists
                var doctor = await _doctorService.GetByIdAsync(scheduleDto.DoctorId);
                if (doctor == null)
                    return NotFound(new { message = "Doctor not found" });

                // Verify hospital exists
                if (scheduleDto.HospitalId != Guid.Empty)
                {
                    var hospital = await _hospitalService.GetByIdAsync(scheduleDto.HospitalId);
                    if (hospital == null)
                        return NotFound(new { message = "Hospital not found" });
                }

                var result = await _doctorScheduleService.CreateAsync(scheduleDto);
                return CreatedAtAction(nameof(GetById), new { scheduleId = result.ScheduleId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Delete a schedule
        [HttpDelete("{scheduleId}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult> Delete(Guid scheduleId)
        {
            try
            {
                var result = await _doctorScheduleService.DeleteAsync(scheduleId);
                if (!result)
                    return NotFound(new { message = "Schedule not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Get all hospitals (for dropdown selection)
        [HttpGet("hospitals")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllHospitals()
        {
            try
            {
                var hospitals = await _hospitalService.GetAllAsync();
                var result = hospitals.Select(h => new
                {
                    hospitalId = h.HospitalId,
                    name = h.Name,
                    city = h.City,
                    address = h.Address
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
