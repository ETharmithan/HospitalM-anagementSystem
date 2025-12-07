using HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto;
using HospitalManagementSystem.Application.IServices.DoctorIServices;
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

        public DoctorAppointmentController(IDoctorAppointmentService doctorAppointmentService)
        {
            _doctorAppointmentService = doctorAppointmentService;
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

        public class CancelAppointmentRequest
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
    }
}
