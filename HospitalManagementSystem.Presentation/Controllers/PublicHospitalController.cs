using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Application.DTOs.HospitalDto.Response_Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublicHospitalController : ControllerBase
    {
        private readonly IHospitalService _hospitalService;

        public PublicHospitalController(IHospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        /// <summary>
        /// Get all active hospitals (public endpoint for patients)
        /// </summary>
        [HttpGet("hospitals")]
        public async Task<ActionResult<List<HospitalResponseDto>>> GetAllActiveHospitals()
        {
            try
            {
                var hospitals = await _hospitalService.GetAllHospitalsAsync();
                // Only return active hospitals for public access
                var activeHospitals = hospitals.Where(h => h.IsActive).ToList();
                return Ok(activeHospitals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve hospitals", error = ex.Message });
            }
        }

        /// <summary>
        /// Get hospital by ID (public endpoint)
        /// </summary>
        [HttpGet("hospitals/{hospitalId}")]
        public async Task<ActionResult<HospitalResponseDto>> GetHospital(Guid hospitalId)
        {
            try
            {
                var hospital = await _hospitalService.GetHospitalByIdAsync(hospitalId);
                if (hospital == null || !hospital.IsActive)
                    return NotFound(new { message = "Hospital not found" });

                return Ok(hospital);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve hospital", error = ex.Message });
            }
        }
    }
}
