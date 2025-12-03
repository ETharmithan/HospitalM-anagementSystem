using HospitalManagementSystem.Application.IServices.DoctorIServices;
using Microsoft.AspNetCore.Mvc;
using System;

namespace HospitalManagementSystem.Presentation.Controllers.DoctorControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;

        public AvailabilityController(IAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        /// <summary>
        /// Get available time slots for a doctor on a specific date
        /// </summary>
        [HttpGet("doctor/{doctorId}/date/{date}")]
        public async Task<IActionResult> GetAvailability(Guid doctorId, DateTime date)
        {
            try
            {
                var availability = await _availabilityService.GetAvailabilityAsync(doctorId, date);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                // TEMP: return full exception details for debugging
                return BadRequest(new { message = ex.ToString() });
            }
        }

        /// <summary>
        /// Get available dates for a doctor within a date range (for calendar)
        /// </summary>
        [HttpGet("doctor/{doctorId}/dates")]
        public async Task<IActionResult> GetAvailableDates(
            Guid doctorId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.Today;
                var end = endDate ?? DateTime.Today.AddMonths(3); // Default 3 months ahead
                
                var availableDates = await _availabilityService.GetAvailableDatesAsync(doctorId, start, end);
                return Ok(availableDates);
            }
            catch (Exception ex)
            {
                // TEMP: return full exception details for debugging
                return BadRequest(new { message = ex.ToString() });
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
                    request.Time);
                
                return Ok(new { available = isAvailable });
            }
            catch (Exception ex)
            {
                // TEMP: return full exception details for debugging
                return BadRequest(new { message = ex.ToString() });
            }
        }
    }

    public class CheckAvailabilityRequest
    {
        public DateTime Date { get; set; }
        public string Time { get; set; } = null!; // Format: "HH:mm"
    }
}

