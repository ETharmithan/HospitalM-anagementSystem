using HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto;
using HospitalManagementSystem.Application.IServices.DoctorIServices;
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

        [HttpPost]
        public async Task<IActionResult> Create(DoctorAppointmentRequestDto doctorAppointmentRequestDto)
        {
            var result = await _doctorAppointmentService.CreateAsync(doctorAppointmentRequestDto);
            return CreatedAtAction(nameof(GetById), new { id = result.AppointmentId }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, DoctorAppointmentRequestDto doctorAppointmentRequestDto)
        {
            var result = await _doctorAppointmentService.UpdateAsync(id, doctorAppointmentRequestDto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _doctorAppointmentService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
