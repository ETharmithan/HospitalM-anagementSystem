using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Domain.Models;
using HospitalManagementSystem.Domain.Models.Patient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController(IPatientRepository patientRepository, IImageUploadService imageUploadService, HospitalManagementSystem.Infrastructure.Data.AppDbContext dbContext) : ControllerBase
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
        [Authorize]
        public async Task<ActionResult<Patient>> CreatePatient([FromBody] CreatePatientDTO patientDto)
        {
            try
            {
                if (patientDto == null)
                    return BadRequest(new { message = "Patient data is required" });

                if (string.IsNullOrWhiteSpace(patientDto.FirstName) || string.IsNullOrWhiteSpace(patientDto.LastName))
                    return BadRequest(new { message = "First name and last name are required" });

                // Parse date of birth
                if (!DateTime.TryParse(patientDto.DateOfBirth, out var dateOfBirth))
                    return BadRequest(new { message = "Invalid date of birth format" });

                // Create patient object
                var patientId = Guid.NewGuid();
                User user = null;
                Guid userId;
                
                // Only create a user if userId is not provided
                if (!patientDto.UserId.HasValue || patientDto.UserId == Guid.Empty)
                {
                    userId = Guid.NewGuid();
                    
                    // Create a user for this patient (only if no userId provided)
                    user = new User
                    {
                        UserId = userId,
                        Username = patientDto.EmailAddress ?? $"patient_{patientId}",
                        Email = patientDto.EmailAddress ?? "",
                        PasswordHash = new byte[0],
                        PasswordSalt = new byte[0],
                        Role = "Patient"
                    };
                }
                else
                {
                    // Use the provided userId
                    userId = patientDto.UserId.Value;
                }
                
                var patient = new Patient
                {
                    PatientId = patientId,
                    UserId = userId,
                    FirstName = patientDto.FirstName,
                    LastName = patientDto.LastName,
                    DateOfBirth = dateOfBirth,
                    Gender = patientDto.Gender,
                    ImageUrl = patientDto.ImageUrl
                };

                // Create nested objects
                var contactInfo = new Patient_Contact_Information
                {
                    PatientId = patientId,
                    PhoneNumber = patientDto.PhoneNumber ?? "",
                    EmailAddress = patientDto.EmailAddress ?? "",
                    AddressLine1 = patientDto.AddressLine1 ?? "",
                    AddressLine2 = patientDto.AddressLine2 ?? "",
                    City = patientDto.City ?? "",
                    State = patientDto.Province ?? "",
                    PostalCode = patientDto.PostalCode ?? "",
                    Country = patientDto.Country ?? "",
                    Nationality = patientDto.Nationality ?? ""
                };

                var identificationDetails = new Patient_Identification_Details
                {
                    PatientId = patientId,
                    NIC = patientDto.NIC ?? ""
                };

                patient.ContactInfo = contactInfo;
                patient.IdentificationDetails = identificationDetails;

                // Add user and patient with nested objects to context
                if (user != null)
                {
                    dbContext.Users.Add(user);
                }
                dbContext.Patients.Add(patient);
                dbContext.Set<Patient_Contact_Information>().Add(contactInfo);
                dbContext.Set<Patient_Identification_Details>().Add(identificationDetails);
                
                await dbContext.SaveChangesAsync();

                // Return a simplified response to avoid circular references
                var response = new
                {
                    patientId = patient.PatientId,
                    userId = patient.UserId,
                    firstName = patient.FirstName,
                    lastName = patient.LastName,
                    dateOfBirth = patient.DateOfBirth,
                    gender = patient.Gender,
                    imageUrl = patient.ImageUrl,
                    message = "Patient registered successfully"
                };
                return CreatedAtAction(nameof(GetPatientById), new { id = patient.PatientId }, response);
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    message = "An error occurred while creating the patient",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                };
                return StatusCode(500, errorDetails);
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
                existingPatient.ImageUrl = patient.ImageUrl;

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

        /// <summary>
        /// Upload patient profile image
        /// </summary>
        [HttpPost("upload-image")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "No file provided" });

                // Validate file
                if (!imageUploadService.IsValidImageFile(file.FileName, file.Length))
                    return BadRequest(new { message = "Invalid file. Only images (jpg, jpeg, png, gif, webp) up to 5MB are allowed." });

                // Upload image
                using (var stream = file.OpenReadStream())
                {
                    string imagePath = await imageUploadService.UploadImageAsync(stream, file.FileName);
                    return Ok(new { message = "Image uploaded successfully", imageUrl = imagePath });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while uploading the image", error = ex.Message });
            }
        }

        /// <summary>
        /// Save additional patient details (emergency contact, medical history, medical info)
        /// </summary>
        [HttpPost("additional-details")]
        [AllowAnonymous]
        public async Task<IActionResult> SaveAdditionalDetails([FromBody] AdditionalPatientDetailsDTO additionalDetailsDto)
        {
            try
            {
                if (additionalDetailsDto == null)
                    return BadRequest(new { message = "Additional details data is required" });

                // Check if patient exists
                var existingPatient = await patientRepository.GetByIdAsync(additionalDetailsDto.PatientId);
                if (existingPatient == null)
                    return NotFound(new { message = "Patient not found" });

                // Create emergency contact
                var emergencyContact = new Patient_Emergency_Contact
                {
                    Id = Guid.NewGuid(),
                    PatientId = additionalDetailsDto.PatientId,
                    ContactName = additionalDetailsDto.EmergencyContact.ContactName,
                    ContactEmail = additionalDetailsDto.EmergencyContact.ContactEmail,
                    ContactPhone = additionalDetailsDto.EmergencyContact.ContactPhone,
                    RelationshipToPatient = additionalDetailsDto.EmergencyContact.RelationshipToPatient
                };

                // Create medical history
                var medicalHistory = new Patient_Medical_History
                {
                    PatientId = additionalDetailsDto.PatientId,
                    PastIllnesses = additionalDetailsDto.MedicalHistory.PastIllnesses,
                    Surgeries = additionalDetailsDto.MedicalHistory.Surgeries,
                    MedicalHistoryNotes = additionalDetailsDto.MedicalHistory.MedicalHistoryNotes
                };

                // Create medical related info
                var medicalInfo = new Patient_Medical_Related_Info
                {
                    PatientId = additionalDetailsDto.PatientId,
                    BloodType = additionalDetailsDto.MedicalInfo.BloodType,
                    Allergies = additionalDetailsDto.MedicalInfo.Allergies,
                    ChronicConditions = additionalDetailsDto.MedicalInfo.ChronicConditions
                };

                // Add to context and save
                dbContext.Set<Patient_Emergency_Contact>().Add(emergencyContact);
                dbContext.Set<Patient_Medical_History>().Add(medicalHistory);
                dbContext.Set<Patient_Medical_Related_Info>().Add(medicalInfo);
                
                await dbContext.SaveChangesAsync();

                return Ok(new { message = "Additional patient details saved successfully" });
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    message = "An error occurred while saving additional patient details",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                };
                return StatusCode(500, errorDetails);
            }
        }
    }
}
