using HospitalManagementSystem.Application.IServices.DoctorIServices;
using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Domain.Models.Doctors;
using HospitalManagementSystem.Domain.IRepository;
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
        // /// <summary>
        // /// Get all doctors
        // /// </summary>
        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<Doctor>>> GetAllDoctors()
        // {
        //     try
        //     {
        //         var doctors = await doctorRepository.GetAllDoctorsAsync();
        //         return Ok(doctors);
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new { message = "An error occurred while fetching doctors", error = ex.Message });
        //     }
        // }

        /// <summary>
        /// Get doctor by ID
        /// </summary>
    //     [HttpGet("{id}")]
    //     public async Task<ActionResult<Doctor>> GetDoctorById(Guid id)
    //     {
    //         try
    //         {
    //             var doctor = await doctorRepository.GetByIdAsync(id);
    //             if (doctor == null)
    //                 return NotFound(new { message = "Doctor not found" });

    //             return Ok(doctor);
    //         }
    //         catch (Exception ex)
    //         {
    //             return StatusCode(500, new { message = "An error occurred while fetching the doctor", error = ex.Message });
    //         }
    //     }

    //     /// <summary>
    //     /// Get doctor by name
    //     /// </summary>
    //     [HttpGet("name/{doctorName}")]
    //     public async Task<ActionResult<Doctor>> GetDoctorByName(string doctorName)
    //     {
    //         try
    //         {
    //             var doctor = await doctorRepository.GetByNameAsync(doctorName);
    //             if (doctor == null)
    //                 return NotFound(new { message = "Doctor not found" });

    //             return Ok(doctor);
    //         }
    //         catch (Exception ex)
    //         {
    //             return StatusCode(500, new { message = "An error occurred while fetching the doctor", error = ex.Message });
    //         }
    //     }

    //     /// <summary>
    //     /// Create a new doctor
    //     /// </summary>
    //     [HttpPost]
    //     [Authorize(Roles = "Admin,SuperAdmin")]
    //     public async Task<ActionResult<Doctor>> CreateDoctor([FromBody] Doctor doctor)
    //     {
    //         try
    //         {
    //             if (!ModelState.IsValid)
    //                 return BadRequest(ModelState);

    //             if (string.IsNullOrWhiteSpace(doctor.Name))
    //                 return BadRequest(new { message = "Doctor name is required" });

    //             doctor.DoctorId = Guid.NewGuid();
    //             await doctorRepository.AddAsync(doctor);
    //             await doctorRepository.SaveChangesAsync();

    //             return CreatedAtAction(nameof(GetDoctorById), new { id = doctor.DoctorId }, doctor);
    //         }
    //         catch (Exception ex)
    //         {
    //             return StatusCode(500, new { message = "An error occurred while creating the doctor", error = ex.Message });
    //         }
    //     }

    //     /// <summary>
    //     /// Update an existing doctor
    //     /// </summary>
    //     [HttpPut("{id}")]
    //     [Authorize(Roles = "Admin,SuperAdmin")]
    //     public async Task<IActionResult> UpdateDoctor(Guid id, [FromBody] Doctor doctor)
    //     {
    //         try
    //         {
    //             if (!ModelState.IsValid)
    //                 return BadRequest(ModelState);

    //             var existingDoctor = await doctorRepository.GetByIdAsync(id);
    //             if (existingDoctor == null)
    //                 return NotFound(new { message = "Doctor not found" });

    //             existingDoctor.Name = doctor.Name;

    //             await doctorRepository.UpdateAsync(existingDoctor);
    //             await doctorRepository.SaveChangesAsync();

    //             return Ok(new { message = "Doctor updated successfully", doctor = existingDoctor });
    //         }
    //         catch (Exception ex)
    //         {
    //             return StatusCode(500, new { message = "An error occurred while updating the doctor", error = ex.Message });
    //         }
    //     }

    //     /// <summary>
    //     /// Delete a doctor
    //     /// </summary>
    //     [HttpDelete("{id}")]
    //     [Authorize(Roles = "Admin,SuperAdmin")]
    //     public async Task<IActionResult> DeleteDoctor(Guid id)
    //     {
    //         try
    //         {
    //             var doctor = await doctorRepository.GetByIdAsync(id);
    //             if (doctor == null)
    //                 return NotFound(new { message = "Doctor not found" });

    //             await doctorRepository.DeleteAsync(id);
    //             await doctorRepository.SaveChangesAsync();

    //             return Ok(new { message = "Doctor deleted successfully" });
    //         }
    //         catch (Exception ex)
    //         {
    //             return StatusCode(500, new { message = "An error occurred while deleting the doctor", error = ex.Message });
    //         }
    //     }
    }
}
