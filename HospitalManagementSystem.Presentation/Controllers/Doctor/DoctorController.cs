using HospitalManagementSystem.Application.DTOs.Doctor.Request_Dto;
using HospitalManagementSystem.Application.DTOs.Doctor.Response_Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Presentation.Controllers.Doctor
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DoctorResponseDto>>> GetAll()
        {
            var doctors = await _doctorService.GetAllAsync();
            return Ok(doctors);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<DoctorResponseDto>> GetById(Guid id)
        {
            var doctor = await _doctorService.GetByIdAsync(id);
            if (doctor == null) return NotFound();
            return Ok(doctor);
        }

        [HttpPost]
        public async Task<ActionResult<DoctorResponseDto>> Create([FromBody] DoctorRequestDto doctorRequestDto)
        {
            var created = await _doctorService.CreateAsync(doctorRequestDto);
            return CreatedAtAction(nameof(GetById), new { id = created.DoctorId }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<DoctorResponseDto>> Update(Guid id, [FromBody] DoctorRequestDto doctorRequestDto)
        {
            var updated = await _doctorService.UpdateAsync(id, doctorRequestDto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _doctorService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
