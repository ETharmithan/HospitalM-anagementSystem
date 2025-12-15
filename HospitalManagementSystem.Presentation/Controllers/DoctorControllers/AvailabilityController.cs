using HospitalManagementSystem.Application.IServices.DoctorIServices;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models.Doctors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace HospitalManagementSystem.Presentation.Controllers.DoctorControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;
        private readonly IDoctorAvailabilityRepository _availabilityRepository;

        public AvailabilityController(
            IAvailabilityService availabilityService,
            IDoctorAvailabilityRepository availabilityRepository)
        {
            _availabilityService = availabilityService;
            _availabilityRepository = availabilityRepository;
        }

        /// <summary>
        /// Get available time slots for a doctor on a specific date
        /// </summary>
        [HttpGet("doctor/{doctorId}/date/{date}")]
        public async Task<IActionResult> GetAvailability(Guid doctorId, DateTime date, [FromQuery] Guid? hospitalId = null)
        {
            try
            {
                var availability = await _availabilityService.GetAvailabilityAsync(doctorId, date, hospitalId);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get available dates for a doctor within a date range (for calendar)
        /// </summary>
        [HttpGet("doctor/{doctorId}/dates")]
        public async Task<IActionResult> GetAvailableDates(
            Guid doctorId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] Guid? hospitalId = null)
        {
            try
            {
                var start = startDate ?? DateTime.Today;
                var end = endDate ?? DateTime.Today.AddMonths(3);
                
                var availableDates = await _availabilityService.GetAvailableDatesAsync(doctorId, start, end, hospitalId);
                return Ok(availableDates);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Check if a specific time slot is available
        /// </summary>
        [HttpPost("doctor/{doctorId}/check")]
        public async Task<IActionResult> CheckSlotAvailability(
            Guid doctorId,
            [FromBody] CheckAvailabilityRequest request)
        {
            try
            {
                var isAvailable = await _availabilityService.IsSlotAvailableAsync(
                    doctorId, 
                    request.Date, 
                    request.Time,
                    request.HospitalId);
                
                return Ok(new { available = isAvailable });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all availability records for a doctor
        /// </summary>
        [HttpGet("doctor/{doctorId}/all")]
        public async Task<IActionResult> GetDoctorAvailabilityRecords(Guid doctorId)
        {
            var records = await _availabilityRepository.GetByDoctorIdAsync(doctorId);
            return Ok(records);
        }

        /// <summary>
        /// Get availability records for a doctor at a specific hospital
        /// </summary>
        [HttpGet("doctor/{doctorId}/hospital/{hospitalId}")]
        public async Task<IActionResult> GetDoctorAvailabilityByHospital(Guid doctorId, Guid hospitalId)
        {
            var records = await _availabilityRepository.GetByDoctorAndHospitalAsync(doctorId, hospitalId);
            return Ok(records);
        }

        /// <summary>
        /// Doctor sets their own availability for a hospital
        /// </summary>
        [HttpPost("doctor/set")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> SetDoctorAvailability([FromBody] SetAvailabilityRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Check if availability already exists for this date and hospital
                var existing = await _availabilityRepository.GetByDoctorAndDateAsync(
                    request.DoctorId, request.Date, request.HospitalId);

                if (existing != null)
                {
                    // Update existing
                    existing.StartTime = request.StartTime;
                    existing.EndTime = request.EndTime;
                    existing.SlotDurationMinutes = request.SlotDurationMinutes;
                    existing.MaxAppointments = request.MaxAppointments;
                    existing.IsAvailable = request.IsAvailable;
                    existing.Reason = request.Reason;
                    existing.ModifiedDate = DateTime.UtcNow;
                    
                    await _availabilityRepository.UpdateAsync(existing);
                    return Ok(existing);
                }

                // Create new
                var availability = new DoctorAvailability
                {
                    AvailabilityId = Guid.NewGuid(),
                    DoctorId = request.DoctorId,
                    HospitalId = request.HospitalId,
                    Date = request.Date.Date,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    SlotDurationMinutes = request.SlotDurationMinutes,
                    MaxAppointments = request.MaxAppointments,
                    IsAvailable = request.IsAvailable,
                    Reason = request.Reason,
                    CreatedByUserId = Guid.Parse(userId),
                    CreatedDate = DateTime.UtcNow
                };

                await _availabilityRepository.CreateAsync(availability);
                return CreatedAtAction(nameof(GetDoctorAvailabilityRecords), 
                    new { doctorId = request.DoctorId }, availability);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Hospital Admin sets doctor availability for their hospital
        /// </summary>
        [HttpPost("admin/set")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> AdminSetDoctorAvailability([FromBody] SetAvailabilityRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // TODO: Verify admin has access to this hospital

                var existing = await _availabilityRepository.GetByDoctorAndDateAsync(
                    request.DoctorId, request.Date, request.HospitalId);

                if (existing != null)
                {
                    existing.StartTime = request.StartTime;
                    existing.EndTime = request.EndTime;
                    existing.SlotDurationMinutes = request.SlotDurationMinutes;
                    existing.MaxAppointments = request.MaxAppointments;
                    existing.IsAvailable = request.IsAvailable;
                    existing.Reason = request.Reason;
                    existing.ModifiedDate = DateTime.UtcNow;
                    
                    await _availabilityRepository.UpdateAsync(existing);
                    return Ok(existing);
                }

                var availability = new DoctorAvailability
                {
                    AvailabilityId = Guid.NewGuid(),
                    DoctorId = request.DoctorId,
                    HospitalId = request.HospitalId,
                    Date = request.Date.Date,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    SlotDurationMinutes = request.SlotDurationMinutes,
                    MaxAppointments = request.MaxAppointments,
                    IsAvailable = request.IsAvailable,
                    Reason = request.Reason,
                    CreatedByUserId = Guid.Parse(userId),
                    CreatedDate = DateTime.UtcNow
                };

                await _availabilityRepository.CreateAsync(availability);
                return CreatedAtAction(nameof(GetDoctorAvailabilityRecords), 
                    new { doctorId = request.DoctorId }, availability);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update availability record
        /// </summary>
        [HttpPut("{availabilityId}")]
        [Authorize(Roles = "Doctor,Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateAvailability(Guid availabilityId, [FromBody] UpdateAvailabilityRequest request)
        {
            try
            {
                var existing = await _availabilityRepository.GetByIdAsync(availabilityId);
                if (existing == null)
                    return NotFound();

                existing.StartTime = request.StartTime;
                existing.EndTime = request.EndTime;
                existing.SlotDurationMinutes = request.SlotDurationMinutes;
                existing.MaxAppointments = request.MaxAppointments;
                existing.IsAvailable = request.IsAvailable;
                existing.Reason = request.Reason;
                existing.ModifiedDate = DateTime.UtcNow;

                await _availabilityRepository.UpdateAsync(existing);
                return Ok(existing);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete availability record
        /// </summary>
        [HttpDelete("{availabilityId}")]
        [Authorize(Roles = "Doctor,Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteAvailability(Guid availabilityId)
        {
            var success = await _availabilityRepository.DeleteAsync(availabilityId);
            if (!success)
                return NotFound();
            return NoContent();
        }
    }

    public class CheckAvailabilityRequest
    {
        public DateTime Date { get; set; }
        public string Time { get; set; } = null!;
        public Guid? HospitalId { get; set; }
    }

    public class SetAvailabilityRequest
    {
        public Guid DoctorId { get; set; }
        public Guid? HospitalId { get; set; }
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = "09:00";
        public string EndTime { get; set; } = "17:00";
        public int SlotDurationMinutes { get; set; } = 30;
        public int MaxAppointments { get; set; } = 10;
        public bool IsAvailable { get; set; } = true;
        public string? Reason { get; set; }
    }

    public class UpdateAvailabilityRequest
    {
        public string StartTime { get; set; } = "09:00";
        public string EndTime { get; set; } = "17:00";
        public int SlotDurationMinutes { get; set; } = 30;
        public int MaxAppointments { get; set; } = 10;
        public bool IsAvailable { get; set; } = true;
        public string? Reason { get; set; }
    }
}

