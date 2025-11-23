using HospitalManagementSystem.Application.DTOs.Doctor.Request_Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Presentation.Controllers.Doctor
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorSalaryController : ControllerBase
    {
        private readonly IDoctorSalaryService _doctorSalaryService;

        public DoctorSalaryController(IDoctorSalaryService doctorSalaryService)
        {
            _doctorSalaryService = doctorSalaryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _doctorSalaryService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _doctorSalaryService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { Message = "Salary record not found" });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DoctorSalaryRequestDto doctorSalaryRequestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _doctorSalaryService.CreateAsync(doctorSalaryRequestDto);
                return CreatedAtAction(nameof(GetById), new { id = created.SalaryId }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _doctorSalaryService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { Message = "Salary record not found" });
            return Ok(new { Message = "Deleted successfully" });
        }
    }
}
