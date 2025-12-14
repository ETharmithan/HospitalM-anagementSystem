using HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto;
using HospitalManagementSystem.Application.IServices.DoctorIServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Presentation.Controllers.DoctorControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorPatientRecordController : ControllerBase
    {
        private readonly IDoctorPatientRecordsService _doctorPatientRecordsService;

        public DoctorPatientRecordController(IDoctorPatientRecordsService doctorPatientRecordsService)
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

        // Get all prescriptions for a specific patient (doctor can view patient's history)
        [HttpGet("patient/{patientId:guid}")]
        public async Task<IActionResult> GetByPatientId(Guid patientId)
        {
            var result = await _doctorPatientRecordsService.GetByPatientIdAsync(patientId);
            return Ok(result);
        }

        // Get all prescriptions created by a specific doctor
        [HttpGet("doctor/{doctorId:guid}")]
        public async Task<IActionResult> GetByDoctorId(Guid doctorId)
        {
            var result = await _doctorPatientRecordsService.GetByDoctorIdAsync(doctorId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DoctorPatientRecordsRequestDto doctorPatientRecordsRequestDto)
        {
            try
            {
                var result = await _doctorPatientRecordsService.CreateAsync(doctorPatientRecordsRequestDto);
                return CreatedAtAction(nameof(GetById), new { id = result.RecordId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, details = ex.InnerException?.Message });
            }
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
