using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Application.IServices.DoctorIServices;
using HospitalManagementSystem.Application.DTOs.HospitalDto.Request_Dto;
using HospitalManagementSystem.Application.DTOs.HospitalDto.Response_Dto;
using DepartmentRequestDto = HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto.DepartmentRequestDto;
using DepartmentResponseDto = HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto.DepartmentResponseDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : ControllerBase
    {
        private readonly IHospitalService _hospitalService;
        private readonly IDepartmentService _departmentService;
        private readonly IUserRepository _userRepository;

        public SuperAdminController(
            IHospitalService hospitalService,
            IDepartmentService departmentService,
            IUserRepository userRepository)
        {
            _hospitalService = hospitalService;
            _departmentService = departmentService;
            _userRepository = userRepository;
        }

        // Hospital Management
        [HttpPost("hospitals")]
        public async Task<ActionResult<HospitalResponseDto>> CreateHospital([FromBody] HospitalRequestDto hospitalDto)
        {
            try
            {
                var result = await _hospitalService.CreateHospitalAsync(hospitalDto);
                return CreatedAtAction(nameof(GetHospital), new { hospitalId = result.HospitalId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("hospitals")]
        public async Task<ActionResult<List<HospitalResponseDto>>> GetAllHospitals()
        {
            var hospitals = await _hospitalService.GetAllHospitalsAsync();
            return Ok(hospitals);
        }

        [HttpGet("hospitals/{hospitalId}")]
        public async Task<ActionResult<HospitalResponseDto>> GetHospital(Guid hospitalId)
        {
            var hospital = await _hospitalService.GetHospitalByIdAsync(hospitalId);
            if (hospital == null)
                return NotFound(new { message = "Hospital not found" });

            return Ok(hospital);
        }

        [HttpPut("hospitals/{hospitalId}")]
        public async Task<ActionResult<HospitalResponseDto>> UpdateHospital(Guid hospitalId, [FromBody] HospitalRequestDto hospitalDto)
        {
            var result = await _hospitalService.UpdateHospitalAsync(hospitalId, hospitalDto);
            if (result == null)
                return NotFound(new { message = "Hospital not found" });

            return Ok(result);
        }

        [HttpDelete("hospitals/{hospitalId}")]
        public async Task<ActionResult> DeleteHospital(Guid hospitalId)
        {
            var result = await _hospitalService.DeleteHospitalAsync(hospitalId);
            if (!result)
                return NotFound(new { message = "Hospital not found" });

            return NoContent();
        }

        // Department Management for Hospitals
        [HttpGet("hospitals/{hospitalId}/departments")]
        public async Task<ActionResult<List<DepartmentResponseDto>>> GetHospitalDepartments(Guid hospitalId)
        {
            var departments = await _hospitalService.GetHospitalDepartmentsAsync(hospitalId);
            return Ok(departments);
        }

        [HttpPost("hospitals/{hospitalId}/departments")]
        public async Task<ActionResult<DepartmentResponseDto>> CreateDepartmentForHospital(
            Guid hospitalId, 
            [FromBody] DepartmentRequestDto departmentDto)
        {
            try
            {
                // Ensure the department is created for the specified hospital
                departmentDto.HospitalId = hospitalId;
                var result = await _departmentService.CreateAsync(departmentDto);
                return CreatedAtAction(nameof(GetHospitalDepartments), new { hospitalId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Hospital Admin Management
        [HttpPost("hospitals/{hospitalId}/admins")]
        public async Task<ActionResult> AssignHospitalAdmin(Guid hospitalId, [FromBody] HospitalAdminRequestDto adminDto)
        {
            try
            {
                // Verify hospital exists
                var hospital = await _hospitalService.GetHospitalByIdAsync(hospitalId);
                if (hospital == null)
                    return NotFound(new { message = "Hospital not found" });

                // Verify the user exists and has Admin role
                var user = await _userRepository.GetByIdAsync(adminDto.UserId);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                if (user.Role != "Admin")
                    return BadRequest(new { message = "User must have Admin role" });

                // Check if user is already admin for this hospital
                if (hospital.HospitalAdmins?.Any(ha => ha.UserId == adminDto.UserId) == true)
                    return BadRequest(new { message = "User is already an admin for this hospital" });

                // Assign hospital admin
                var result = await _hospitalService.AssignHospitalAdminAsync(hospitalId, adminDto.UserId);
                if (!result)
                    return BadRequest(new { message = "Failed to assign admin to hospital" });

                return Ok(new { message = "Hospital admin assigned successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("hospitals/{hospitalId}/admins/{userId}")]
        public async Task<ActionResult> RemoveHospitalAdmin(Guid hospitalId, Guid userId)
        {
            try
            {
                var result = await _hospitalService.RemoveHospitalAdminAsync(hospitalId, userId);
                if (!result)
                    return NotFound(new { message = "Hospital admin assignment not found" });

                return Ok(new { message = "Hospital admin removed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
