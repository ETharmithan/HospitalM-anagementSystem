using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Domain.Models.Doctor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController(IDoctorRepository doctorRepository) : ControllerBase
    {
        /// <summary>
        /// Get all doctors
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Doctors>>> GetAllDoctors()
        {
            try
            {
                var doctors = await doctorRepository.GetAllDoctorsAsync();
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching doctors", error = ex.Message });
            }
        }

        /// <summary>
        /// Get doctor by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Doctors>> GetDoctorById(Guid id)
        {
            try
            {
                var doctor = await doctorRepository.GetByIdAsync(id);
                if (doctor == null)
                    return NotFound(new { message = "Doctor not found" });

                return Ok(doctor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the doctor", error = ex.Message });
            }
        }

        /// <summary>
        /// Get doctor by name
        /// </summary>
        [HttpGet("name/{doctorName}")]
        public async Task<ActionResult<Doctors>> GetDoctorByName(string doctorName)
        {
            try
            {
                var doctor = await doctorRepository.GetByNameAsync(doctorName);
                if (doctor == null)
                    return NotFound(new { message = "Doctor not found" });

                return Ok(doctor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the doctor", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new doctor
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<Doctors>> CreateDoctor([FromBody] Doctors doctor)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (string.IsNullOrWhiteSpace(doctor.DoctorName))
                    return BadRequest(new { message = "Doctor name is required" });

                doctor.Id = Guid.NewGuid();
                await doctorRepository.AddAsync(doctor);
                await doctorRepository.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDoctorById), new { id = doctor.Id }, doctor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the doctor", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing doctor
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateDoctor(Guid id, [FromBody] Doctors doctor)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingDoctor = await doctorRepository.GetByIdAsync(id);
                if (existingDoctor == null)
                    return NotFound(new { message = "Doctor not found" });

                existingDoctor.DoctorName = doctor.DoctorName;

                await doctorRepository.UpdateAsync(existingDoctor);
                await doctorRepository.SaveChangesAsync();

                return Ok(new { message = "Doctor updated successfully", doctor = existingDoctor });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the doctor", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a doctor
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteDoctor(Guid id)
        {
            try
            {
                var doctor = await doctorRepository.GetByIdAsync(id);
                if (doctor == null)
                    return NotFound(new { message = "Doctor not found" });

                await doctorRepository.DeleteAsync(id);
                await doctorRepository.SaveChangesAsync();

                return Ok(new { message = "Doctor deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the doctor", error = ex.Message });
            }
        }
    }
}
