using HospitalManagementSystem.Application.DTOs.Doctor.Request_Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Presentation.Controllers.Doctor
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorPatientRecordController : ControllerBase
    {
        private readonly IDoctorPatientRecordsService _doctorPatientRecordsService;

        public DoctorPatientRecordsController(IDoctorPatientRecordsService doctorPatientRecordsService)
        {
            _doctorPatientRecordsService = doctorPatientRecordsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _doctorPatientRecordsService.GetAllAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _doctorPatientRecordsService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DoctorPatientRecordsRequestDto doctorPatientRecordsRequestDto)
        {
            var result = await _doctorPatientRecordsService.CreateAsync(doctorPatientRecordsRequestDto);
            return CreatedAtAction(nameof(GetById), new { id = result.RecordId }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, DoctorPatientRecordsRequestDto doctorPatientRecordsRequestDto)
        {
            var result = await _doctorPatientRecordsService.UpdateAsync(id, doctorPatientRecordsRequestDto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _doctorPatientRecordsService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
