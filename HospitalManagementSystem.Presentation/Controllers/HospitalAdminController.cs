using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Application.IServices.DoctorIServices;
using HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto;
using HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models.Patient;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class HospitalAdminController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly IDoctorScheduleService _doctorScheduleService;
        private readonly IDepartmentService _departmentService;
        private readonly IHospitalService _hospitalService;
        private readonly IPatientRepository _patientRepository;
        private readonly AppDbContext _dbContext;

        public HospitalAdminController(
            IDoctorService doctorService,
            IDoctorScheduleService doctorScheduleService,
            IDepartmentService departmentService,
            IHospitalService hospitalService,
            IPatientRepository patientRepository,
            AppDbContext dbContext)
        {
            _doctorService = doctorService;
            _doctorScheduleService = doctorScheduleService;
            _departmentService = departmentService;
            _hospitalService = hospitalService;
            _patientRepository = patientRepository;
            _dbContext = dbContext;
        }

        // Helper method to get admin's hospital ID
        private async Task<Guid?> GetAdminHospitalIdAsync()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            var userId = Guid.Parse(userIdClaim);
            var hospitalAdmin = await _dbContext.HospitalAdmins
                .FirstOrDefaultAsync(ha => ha.UserId == userId && ha.IsActive);

            return hospitalAdmin?.HospitalId;
        }

        // Doctor Management
        [HttpPost("doctors")]
        public async Task<ActionResult<DoctorResponseDto>> CreateDoctor([FromBody] DoctorRequestDto doctorDto)
        {
            try
            {
                // Verify the department exists and belongs to admin's hospital
                var department = await _departmentService.GetByIdAsync(doctorDto.DepartmentId);
                if (department == null)
                    return NotFound(new { message = "Department not found" });

                // Note: You should add logic here to verify the admin has access to this hospital/department
                // This would require getting the admin's hospital assignment from current user context

                var result = await _doctorService.CreateAsync(doctorDto);
                return CreatedAtAction(nameof(GetDoctor), new { doctorId = result.DoctorId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("doctors")]
        public async Task<ActionResult<List<DoctorResponseDto>>> GetDoctors([FromQuery] Guid? departmentId = null)
        {
            try
            {
                var hospitalId = await GetAdminHospitalIdAsync();
                if (!hospitalId.HasValue)
                    return Unauthorized(new { message = "Admin not assigned to any hospital" });

                // Get departments for this hospital
                var hospitalDepartments = await _dbContext.Departments
                    .Where(d => d.HospitalId == hospitalId.Value)
                    .Select(d => d.DepartmentId)
                    .ToListAsync();

                // Get all doctors
                var doctors = await _doctorService.GetAllAsync();
                
                // Filter by hospital's departments
                doctors = doctors.Where(d => d.DepartmentId.HasValue && hospitalDepartments.Contains(d.DepartmentId.Value)).ToList();
                
                if (departmentId.HasValue)
                {
                    doctors = doctors.Where(d => d.DepartmentId == departmentId.Value).ToList();
                }

                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("doctors/{doctorId}")]
        public async Task<ActionResult<DoctorResponseDto>> GetDoctor(Guid doctorId)
        {
            try
            {
                var doctor = await _doctorService.GetByIdAsync(doctorId);
                if (doctor == null)
                    return NotFound(new { message = "Doctor not found" });

                // Note: You should verify the admin has access to this doctor's hospital
                return Ok(doctor);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("doctors/{doctorId}")]
        public async Task<ActionResult<DoctorResponseDto>> UpdateDoctor(Guid doctorId, [FromBody] DoctorRequestDto doctorDto)
        {
            try
            {
                var result = await _doctorService.UpdateAsync(doctorId, doctorDto);
                if (result == null)
                    return NotFound(new { message = "Doctor not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("doctors/{doctorId}")]
        public async Task<ActionResult> DeleteDoctor(Guid doctorId)
        {
            try
            {
                var result = await _doctorService.DeleteAsync(doctorId);
                if (!result)
                    return NotFound(new { message = "Doctor not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Doctor Schedule Management
        [HttpPost("doctors/{doctorId}/schedules")]
        public async Task<ActionResult<DoctorScheduleResponseDto>> CreateDoctorSchedule(
            Guid doctorId, 
            [FromBody] DoctorScheduleRequestDto scheduleDto)
        {
            try
            {
                // Verify doctor exists and admin has access
                var doctor = await _doctorService.GetByIdAsync(doctorId);
                if (doctor == null)
                    return NotFound(new { message = "Doctor not found" });

                // Set the doctor ID in the schedule DTO
                scheduleDto.DoctorId = doctorId;

                var result = await _doctorScheduleService.CreateAsync(scheduleDto);
                return CreatedAtAction(nameof(GetDoctorSchedules), new { doctorId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("doctors/{doctorId}/schedules")]
        public async Task<ActionResult<List<DoctorScheduleResponseDto>>> GetDoctorSchedules(Guid doctorId)
        {
            try
            {
                var schedules = await _doctorScheduleService.GetAllAsync();
                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Update method not implemented in IDoctorScheduleService
        // [HttpPut("doctors/{doctorId}/schedules/{scheduleId}")]
        // public async Task<ActionResult<DoctorScheduleResponseDto>> UpdateDoctorSchedule(
        //     Guid doctorId,
        //     Guid scheduleId,
        //     [FromBody] DoctorScheduleRequestDto scheduleDto)
        // {
        //     try
        //     {
        //         scheduleDto.DoctorId = doctorId;
        //         var result = await _doctorScheduleService.UpdateAsync(scheduleId, scheduleDto);
        //         if (result == null)
        //             return NotFound(new { message = "Schedule not found" });

        //         return Ok(result);
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        [HttpDelete("doctors/{doctorId}/schedules/{scheduleId}")]
        public async Task<ActionResult> DeleteDoctorSchedule(Guid doctorId, Guid scheduleId)
        {
            try
            {
                var result = await _doctorScheduleService.DeleteAsync(scheduleId);
                if (!result)
                    return NotFound(new { message = "Schedule not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Department Management (within admin's hospital)
        [HttpGet("departments")]
        public async Task<ActionResult<List<DepartmentResponseDto>>> GetDepartments()
        {
            try
            {
                var hospitalId = await GetAdminHospitalIdAsync();
                if (!hospitalId.HasValue)
                    return Unauthorized(new { message = "Admin not assigned to any hospital" });

                var departments = await _departmentService.GetAllAsync();
                
                // Filter by admin's hospital
                var filteredDepartments = departments.Where(d => d.HospitalId == hospitalId.Value).ToList();
                
                return Ok(filteredDepartments);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("departments")]
        public async Task<ActionResult<DepartmentResponseDto>> CreateDepartment([FromBody] DepartmentRequestDto departmentDto)
        {
            try
            {
                var hospitalId = await GetAdminHospitalIdAsync();
                if (!hospitalId.HasValue)
                    return Unauthorized(new { message = "Admin not assigned to any hospital" });

                // Verify the hospital exists
                var hospital = await _hospitalService.GetByIdAsync(hospitalId.Value);
                if (hospital == null)
                    return BadRequest(new { message = "The hospital assigned to this admin does not exist. Please contact the system administrator." });

                // Set the hospital ID to the admin's hospital
                departmentDto.HospitalId = hospitalId.Value;
                
                var result = await _departmentService.CreateAsync(departmentDto);
                return CreatedAtAction(nameof(GetDepartment), new { departmentId = result.DepartmentId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("departments/{departmentId}")]
        public async Task<ActionResult<DepartmentResponseDto>> GetDepartment(Guid departmentId)
        {
            try
            {
                var department = await _departmentService.GetByIdAsync(departmentId);
                if (department == null)
                    return NotFound(new { message = "Department not found" });

                return Ok(department);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("departments/{departmentId}")]
        public async Task<ActionResult<DepartmentResponseDto>> UpdateDepartment(
            Guid departmentId, 
            [FromBody] DepartmentRequestDto departmentDto)
        {
            try
            {
                var result = await _departmentService.UpdateAsync(departmentId, departmentDto);
                if (result == null)
                    return NotFound(new { message = "Department not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("departments/{departmentId}")]
        public async Task<ActionResult> DeleteDepartment(Guid departmentId)
        {
            try
            {
                var result = await _departmentService.DeleteAsync(departmentId);
                if (!result)
                    return NotFound(new { message = "Department not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Patient Management
        [HttpGet("patients")]
        public async Task<ActionResult<List<Patient>>> GetPatients()
        {
            try
            {
                var patients = await _patientRepository.GetAllPatientsAsync();
                return Ok(patients);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("patients/{patientId}")]
        public async Task<ActionResult<Patient>> GetPatient(Guid patientId)
        {
            try
            {
                var patient = await _patientRepository.GetPatientWithDetailsAsync(patientId);
                if (patient == null)
                    return NotFound(new { message = "Patient not found" });

                return Ok(patient);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("patients/{patientId}")]
        public async Task<ActionResult> UpdatePatient(Guid patientId, [FromBody] Patient patient)
        {
            try
            {
                var existingPatient = await _patientRepository.GetByIdAsync(patientId);
                if (existingPatient == null)
                    return NotFound(new { message = "Patient not found" });

                existingPatient.FirstName = patient.FirstName;
                existingPatient.LastName = patient.LastName;
                existingPatient.DateOfBirth = patient.DateOfBirth;
                existingPatient.Gender = patient.Gender;
                existingPatient.ImageUrl = patient.ImageUrl;

                await _patientRepository.UpdateAsync(existingPatient);
                await _patientRepository.SaveChangesAsync();

                return Ok(new { message = "Patient updated successfully", patient = existingPatient });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("patients/{patientId}")]
        public async Task<ActionResult> DeletePatient(Guid patientId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                    return NotFound(new { message = "Patient not found" });

                await _patientRepository.DeleteAsync(patientId);
                await _patientRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
