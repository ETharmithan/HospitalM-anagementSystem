using HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto;
using HospitalManagementSystem.Application.IServices.DoctorIServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Presentation.Controllers.DoctorControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorLeaveController : ControllerBase
    {
        private readonly IDoctorLeaveService _doctorLeaveService;

        public DoctorLeaveController(IDoctorLeaveService doctorLeaveService)
        {
            _doctorLeaveService = doctorLeaveService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _doctorLeaveService.GetAllAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _doctorLeaveService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DoctorLeaveRequestDto doctorLeaveRequestDto)
        {
            var result = await _doctorLeaveService.CreateAsync(doctorLeaveRequestDto);
            return CreatedAtAction(nameof(GetById), new { id = result.LeaveId }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, DoctorLeaveRequestDto doctorLeaveRequestDto)
        {
            var result = await _doctorLeaveService.UpdateAsync(id, doctorLeaveRequestDto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _doctorLeaveService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
