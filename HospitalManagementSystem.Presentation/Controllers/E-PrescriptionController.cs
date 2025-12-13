using HospitalManagementSystem.Application.DTOs.EPrescriptionDto.Request_Dto;
using HospitalManagementSystem.Application.DTOs.EPrescriptionDto.Response_Dto;
using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models.Doctors;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EPrescriptionController(
        AppDbContext dbContext,
        IDoctorRepository doctorRepository,
        IPatientRepository patientRepository) : ControllerBase
    {
        private async Task<Guid?> GetCurrentDoctorIdAsync()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return null;
            var doctor = await doctorRepository.GetByEmailAsync(email);
            return doctor?.DoctorId;
        }

        private async Task<Guid?> GetCurrentPatientIdAsync()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdValue)) return null;
            if (!Guid.TryParse(userIdValue, out var userId)) return null;

            var patient = await patientRepository.GetByUserIdAsync(userId);
            return patient?.PatientId;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EPrescriptionResponseDto>> GetById(Guid id)
        {
            var entity = await dbContext.EPrescriptions.FirstOrDefaultAsync(x => x.EPrescriptionId == id);
            if (entity == null) return NotFound(new { message = "E-Prescription not found" });

            var isDoctor = User.IsInRole("Doctor");
            var isPatient = User.IsInRole("Patient");

            if (isDoctor)
            {
                var doctorId = await GetCurrentDoctorIdAsync();
                if (!doctorId.HasValue) return Unauthorized(new { message = "Doctor not found" });
                if (entity.DoctorId != doctorId.Value) return Forbid();
            }
            else if (isPatient)
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (!patientId.HasValue) return Unauthorized(new { message = "Patient not found" });
                if (entity.PatientId != patientId.Value) return Forbid();
            }

            return Ok(new EPrescriptionResponseDto
            {
                EPrescriptionId = entity.EPrescriptionId,
                Diagnosis = entity.Diagnosis,
                Prescription = entity.Prescription,
                Notes = entity.Notes,
                VisitDate = entity.VisitDate,
                CreatedAt = entity.CreatedAt,
                DoctorId = entity.DoctorId,
                PatientId = entity.PatientId
            });
        }

        [HttpGet("patient/{patientId:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<IEnumerable<EPrescriptionResponseDto>>> GetByPatientId(Guid patientId)
        {
            var list = await dbContext.EPrescriptions
                .Where(x => x.PatientId == patientId)
                .OrderByDescending(x => x.VisitDate)
                .ToListAsync();

            return Ok(list.Select(entity => new EPrescriptionResponseDto
            {
                EPrescriptionId = entity.EPrescriptionId,
                Diagnosis = entity.Diagnosis,
                Prescription = entity.Prescription,
                Notes = entity.Notes,
                VisitDate = entity.VisitDate,
                CreatedAt = entity.CreatedAt,
                DoctorId = entity.DoctorId,
                PatientId = entity.PatientId
            }));
        }

        [HttpGet("doctor/{doctorId:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<IEnumerable<EPrescriptionResponseDto>>> GetByDoctorId(Guid doctorId)
        {
            var list = await dbContext.EPrescriptions
                .Where(x => x.DoctorId == doctorId)
                .OrderByDescending(x => x.VisitDate)
                .ToListAsync();

            return Ok(list.Select(entity => new EPrescriptionResponseDto
            {
                EPrescriptionId = entity.EPrescriptionId,
                Diagnosis = entity.Diagnosis,
                Prescription = entity.Prescription,
                Notes = entity.Notes,
                VisitDate = entity.VisitDate,
                CreatedAt = entity.CreatedAt,
                DoctorId = entity.DoctorId,
                PatientId = entity.PatientId
            }));
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<EPrescriptionResponseDto>> Create([FromBody] CreateEPrescriptionRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var doctorId = await GetCurrentDoctorIdAsync();
            if (!doctorId.HasValue) return Unauthorized(new { message = "Doctor profile not found" });

            if (dto.DoctorId != doctorId.Value)
            {
                return BadRequest(new { message = "DoctorId mismatch" });
            }

            var entity = new EPrescription
            {
                EPrescriptionId = Guid.NewGuid(),
                Diagnosis = dto.Diagnosis,
                Prescription = dto.Prescription,
                Notes = dto.Notes ?? string.Empty,
                VisitDate = dto.VisitDate,
                CreatedAt = DateTime.UtcNow,
                DoctorId = dto.DoctorId,
                PatientId = dto.PatientId
            };

            await dbContext.EPrescriptions.AddAsync(entity);
            await dbContext.SaveChangesAsync();

            var response = new EPrescriptionResponseDto
            {
                EPrescriptionId = entity.EPrescriptionId,
                Diagnosis = entity.Diagnosis,
                Prescription = entity.Prescription,
                Notes = entity.Notes,
                VisitDate = entity.VisitDate,
                CreatedAt = entity.CreatedAt,
                DoctorId = entity.DoctorId,
                PatientId = entity.PatientId
            };

            return CreatedAtAction(nameof(GetById), new { id = response.EPrescriptionId }, response);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<EPrescriptionResponseDto>> Update(Guid id, [FromBody] UpdateEPrescriptionRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var doctorId = await GetCurrentDoctorIdAsync();
            if (!doctorId.HasValue) return Unauthorized(new { message = "Doctor profile not found" });

            var entity = await dbContext.EPrescriptions.FirstOrDefaultAsync(x => x.EPrescriptionId == id);
            if (entity == null) return NotFound(new { message = "E-Prescription not found" });
            if (entity.DoctorId != doctorId.Value) return Forbid();

            entity.Diagnosis = dto.Diagnosis;
            entity.Prescription = dto.Prescription;
            entity.Notes = dto.Notes ?? string.Empty;
            entity.VisitDate = dto.VisitDate;

            await dbContext.SaveChangesAsync();

            return Ok(new EPrescriptionResponseDto
            {
                EPrescriptionId = entity.EPrescriptionId,
                Diagnosis = entity.Diagnosis,
                Prescription = entity.Prescription,
                Notes = entity.Notes,
                VisitDate = entity.VisitDate,
                CreatedAt = entity.CreatedAt,
                DoctorId = entity.DoctorId,
                PatientId = entity.PatientId
            });
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (!doctorId.HasValue) return Unauthorized(new { message = "Doctor profile not found" });

            var entity = await dbContext.EPrescriptions.FirstOrDefaultAsync(x => x.EPrescriptionId == id);
            if (entity == null) return NotFound(new { message = "E-Prescription not found" });
            if (entity.DoctorId != doctorId.Value) return Forbid();

            dbContext.EPrescriptions.Remove(entity);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("my")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<IEnumerable<EPrescriptionResponseDto>>> GetMyPrescriptions()
        {
            var patientId = await GetCurrentPatientIdAsync();
            if (!patientId.HasValue) return Unauthorized(new { message = "Patient profile not found" });

            var list = await dbContext.EPrescriptions
                .Where(x => x.PatientId == patientId.Value)
                .OrderByDescending(x => x.VisitDate)
                .ToListAsync();

            return Ok(list.Select(entity => new EPrescriptionResponseDto
            {
                EPrescriptionId = entity.EPrescriptionId,
                Diagnosis = entity.Diagnosis,
                Prescription = entity.Prescription,
                Notes = entity.Notes,
                VisitDate = entity.VisitDate,
                CreatedAt = entity.CreatedAt,
                DoctorId = entity.DoctorId,
                PatientId = entity.PatientId
            }));
        }

        [HttpGet("{id:guid}/download")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Download(Guid id)
        {
            var patientId = await GetCurrentPatientIdAsync();
            if (!patientId.HasValue) return Unauthorized(new { message = "Patient profile not found" });

            var entity = await dbContext.EPrescriptions.FirstOrDefaultAsync(x => x.EPrescriptionId == id);
            if (entity == null) return NotFound(new { message = "E-Prescription not found" });
            if (entity.PatientId != patientId.Value) return Forbid();

            var sb = new StringBuilder();
            sb.AppendLine("E-PRESCRIPTION");
            sb.AppendLine($"Prescription ID: {entity.EPrescriptionId}");
            sb.AppendLine($"Visit Date: {entity.VisitDate:yyyy-MM-dd}");
            sb.AppendLine($"Doctor ID: {entity.DoctorId}");
            sb.AppendLine($"Patient ID: {entity.PatientId}");
            sb.AppendLine();
            sb.AppendLine($"Diagnosis: {entity.Diagnosis}");
            sb.AppendLine();
            sb.AppendLine("Prescription:");
            sb.AppendLine(entity.Prescription);
            sb.AppendLine();
            sb.AppendLine("Notes:");
            sb.AppendLine(entity.Notes);

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"e-prescription-{entity.EPrescriptionId}.txt";
            return File(bytes, "text/plain", fileName);
        }
    }
}
