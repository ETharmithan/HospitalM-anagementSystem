using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Domain.Models.Patient;
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
    public class PatientController(IPatientRepository patientRepository) : ControllerBase
    {
        /// <summary>
        /// Get all patients
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Patient>>> GetAllPatients()
        {
            try
            {
                var patients = await patientRepository.GetAllPatientsAsync();
                return Ok(patients);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching patients", error = ex.Message });
            }
        }

        /// <summary>
        /// Get patient by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Patient>> GetPatientById(Guid id)
        {
            try
            {
                var patient = await patientRepository.GetPatientWithDetailsAsync(id);
                if (patient == null)
                    return NotFound(new { message = "Patient not found" });

                return Ok(patient);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the patient", error = ex.Message });
            }
        }

        /// <summary>
        /// Get patient by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<Patient>> GetPatientByUserId(Guid userId)
        {
            try
            {
                var patient = await patientRepository.GetByUserIdAsync(userId);
                if (patient == null)
                    return NotFound(new { message = "Patient not found for this user" });

                return Ok(patient);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the patient", error = ex.Message });
            }
        }

        /// <summary>
        /// Get patients by gender
        /// </summary>
        [HttpGet("gender/{gender}")]
        public async Task<ActionResult<IEnumerable<Patient>>> GetPatientsByGender(string gender)
        {
            try
            {
                var patients = await patientRepository.GetPatientsByGenderAsync(gender);
                return Ok(patients);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching patients", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new patient
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Patient>> CreatePatient([FromBody] Patient patient)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (string.IsNullOrWhiteSpace(patient.FirstName) || string.IsNullOrWhiteSpace(patient.LastName))
                    return BadRequest(new { message = "First name and last name are required" });

                patient.PatientId = Guid.NewGuid();
                await patientRepository.AddAsync(patient);
                await patientRepository.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPatientById), new { id = patient.PatientId }, patient);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the patient", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing patient
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(Guid id, [FromBody] Patient patient)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingPatient = await patientRepository.GetByIdAsync(id);
                if (existingPatient == null)
                    return NotFound(new { message = "Patient not found" });

                existingPatient.FirstName = patient.FirstName;
                existingPatient.LastName = patient.LastName;
                existingPatient.DateOfBirth = patient.DateOfBirth;
                existingPatient.Gender = patient.Gender;

                await patientRepository.UpdateAsync(existingPatient);
                await patientRepository.SaveChangesAsync();

                return Ok(new { message = "Patient updated successfully", patient = existingPatient });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the patient", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a patient
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeletePatient(Guid id)
        {
            try
            {
                var patient = await patientRepository.GetByIdAsync(id);
                if (patient == null)
                    return NotFound(new { message = "Patient not found" });

                await patientRepository.DeleteAsync(id);
                await patientRepository.SaveChangesAsync();

                return Ok(new { message = "Patient deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the patient", error = ex.Message });
            }
        }
    }
}
