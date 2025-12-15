using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Application.DTOs.Patient;
using HospitalManagementSystem.Application.DTOs.PatientDto;
using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Application.IServices.DoctorIServices;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models;
using HospitalManagementSystem.Domain.Models.Patient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class PatientController(
        IPatientRepository patientRepository, 
        IImageUploadService imageUploadService, 
        HospitalManagementSystem.Infrastructure.Data.AppDbContext dbContext,
        IDoctorPatientRecordsService prescriptionService,
        IDoctorAppointmentService appointmentService,
        IEmailService emailService) : ControllerBase
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
        public async Task<IActionResult> GetPatientByUserId(Guid userId)
        {
            try
            {
                var patient = await patientRepository.GetByUserIdAsync(userId);
                if (patient == null)
                    return NotFound(new { message = "Patient not found for this user" });

                return Ok(BuildPatientProfileResponse(patient));
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
                
                // Mark additional info as completed
                existingPatient.HasCompletedAdditionalInfo = true;
                existingPatient.AdditionalInfoCompletedAt = DateTime.UtcNow;
                
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

        /// <summary>
        /// Get patient's complete medical profile (for doctors to view)
        /// Includes: basic info, medical history, allergies, past prescriptions, past appointments
        /// </summary>
        [HttpGet("{patientId}/medical-profile")]
        public async Task<ActionResult<PatientMedicalProfileDto>> GetPatientMedicalProfile(Guid patientId)
        {
            try
            {
                var patient = await patientRepository.GetPatientWithDetailsAsync(patientId);
                if (patient == null)
                    return NotFound(new { message = "Patient not found" });

                // Get prescriptions
                var prescriptions = await prescriptionService.GetByPatientIdAsync(patientId);
                
                // Get past appointments
                var appointments = await appointmentService.GetByPatientIdAsync(patientId);

                var profile = new PatientMedicalProfileDto
                {
                    PatientId = patient.PatientId,
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    DateOfBirth = patient.DateOfBirth,
                    Gender = patient.Gender,
                    ImageUrl = patient.ImageUrl,

                    // Contact Info
                    Email = patient.ContactInfo?.EmailAddress,
                    Phone = patient.ContactInfo?.PhoneNumber,
                    Address = patient.ContactInfo != null 
                        ? $"{patient.ContactInfo.AddressLine1}, {patient.ContactInfo.City}, {patient.ContactInfo.State}" 
                        : null,

                    // Medical Info
                    BloodType = patient.MedicalRelatedInfo?.BloodType,
                    Allergies = patient.MedicalRelatedInfo?.Allergies,
                    ChronicConditions = patient.MedicalRelatedInfo?.ChronicConditions,

                    // Medical History
                    PastIllnesses = patient.MedicalHistory?.PastIllnesses,
                    Surgeries = patient.MedicalHistory?.Surgeries,
                    MedicalHistoryNotes = patient.MedicalHistory?.MedicalHistoryNotes,

                    // Emergency Contact
                    EmergencyContactName = patient.EmergencyContact?.ContactName,
                    EmergencyContactPhone = patient.EmergencyContact?.ContactPhone,
                    EmergencyContactRelationship = patient.EmergencyContact?.RelationshipToPatient,

                    // Prescriptions
                    Prescriptions = prescriptions.Select(p => new PrescriptionSummaryDto
                    {
                        RecordId = p.RecordId,
                        Diagnosis = p.Diagnosis,
                        Prescription = p.Prescription,
                        Notes = p.Notes,
                        VisitDate = p.VisitDate,
                        DoctorName = p.DoctorName,
                        DoctorId = p.DoctorId
                    }).ToList(),

                    // Past Appointments
                    PastAppointments = appointments.Select(a => new AppointmentSummaryDto
                    {
                        AppointmentId = a.AppointmentId,
                        AppointmentDate = a.AppointmentDate,
                        AppointmentTime = a.AppointmentTime,
                        Status = a.AppointmentStatus,
                        DoctorName = a.DoctorName,
                        HospitalName = a.HospitalName
                    }).ToList()
                };

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching patient medical profile", error = ex.Message });
            }
        }

        /// <summary>
        /// Get patient's prescriptions only
        /// </summary>
        [HttpGet("{patientId}/prescriptions")]
        public async Task<ActionResult> GetPatientPrescriptions(Guid patientId)
        {
            try
            {
                var prescriptions = await prescriptionService.GetByPatientIdAsync(patientId);
                return Ok(prescriptions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching prescriptions", error = ex.Message });
            }
        }

        /// <summary>
        /// Update patient profile (excluding NIC, email, and name)
        /// </summary>
        [HttpPut("{patientId}/profile")]
        public async Task<IActionResult> UpdatePatientProfile(Guid patientId, [FromBody] UpdatePatientProfileDto profileDto)
        {
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var patient = await dbContext.Patients
                        .Include(p => p.ContactInfo)
                        .Include(p => p.MedicalRelatedInfo)
                        .Include(p => p.EmergencyContact)
                        .FirstOrDefaultAsync(p => p.PatientId == patientId);

                    if (patient == null)
                        return NotFound(new { message = "Patient not found" });

                    // Update basic info
                    if (profileDto.DateOfBirth.HasValue)
                        patient.DateOfBirth = profileDto.DateOfBirth.Value;
                    if (!string.IsNullOrWhiteSpace(profileDto.Gender))
                        patient.Gender = profileDto.Gender;
                    if (!string.IsNullOrWhiteSpace(profileDto.ImageUrl))
                        patient.ImageUrl = profileDto.ImageUrl;

                    // Update or create contact info
                    if (patient.ContactInfo == null)
                    {
                        patient.ContactInfo = new Patient_Contact_Information
                        {
                            PatientId = patientId,
                            PhoneNumber = profileDto.PhoneNumber ?? string.Empty,
                            AddressLine1 = profileDto.AddressLine1 ?? string.Empty,
                            AddressLine2 = profileDto.AddressLine2 ?? string.Empty,
                            City = profileDto.City ?? string.Empty,
                            State = profileDto.State ?? string.Empty,
                            PostalCode = profileDto.PostalCode ?? string.Empty,
                            Country = profileDto.Country ?? string.Empty,
                            Nationality = profileDto.Nationality ?? string.Empty
                        };
                        dbContext.Set<Patient_Contact_Information>().Add(patient.ContactInfo);
                    }
                    else
                    {
                        if (profileDto.PhoneNumber != null)
                            patient.ContactInfo.PhoneNumber = profileDto.PhoneNumber;
                        if (profileDto.AddressLine1 != null)
                            patient.ContactInfo.AddressLine1 = profileDto.AddressLine1;
                        if (profileDto.AddressLine2 != null)
                            patient.ContactInfo.AddressLine2 = profileDto.AddressLine2;
                        if (profileDto.City != null)
                            patient.ContactInfo.City = profileDto.City;
                        if (profileDto.State != null)
                            patient.ContactInfo.State = profileDto.State;
                        if (profileDto.PostalCode != null)
                            patient.ContactInfo.PostalCode = profileDto.PostalCode;
                        if (profileDto.Country != null)
                            patient.ContactInfo.Country = profileDto.Country;
                        if (profileDto.Nationality != null)
                            patient.ContactInfo.Nationality = profileDto.Nationality;
                    }

                    // Update or create medical info
                    if (patient.MedicalRelatedInfo == null)
                    {
                        patient.MedicalRelatedInfo = new Patient_Medical_Related_Info
                        {
                            PatientId = patientId,
                            BloodType = profileDto.BloodType ?? string.Empty,
                            Allergies = profileDto.Allergies ?? string.Empty,
                            ChronicConditions = profileDto.ChronicConditions ?? string.Empty
                        };
                        dbContext.Set<Patient_Medical_Related_Info>().Add(patient.MedicalRelatedInfo);
                    }
                    else
                    {
                        if (profileDto.BloodType != null)
                            patient.MedicalRelatedInfo.BloodType = profileDto.BloodType;
                        if (profileDto.Allergies != null)
                            patient.MedicalRelatedInfo.Allergies = profileDto.Allergies;
                        if (profileDto.ChronicConditions != null)
                            patient.MedicalRelatedInfo.ChronicConditions = profileDto.ChronicConditions;
                    }

                    // Update or create emergency contact
                    if (patient.EmergencyContact == null && 
                        (!string.IsNullOrEmpty(profileDto.EmergencyContactName) || 
                         !string.IsNullOrEmpty(profileDto.EmergencyContactPhone)))
                    {
                        patient.EmergencyContact = new Patient_Emergency_Contact
                        {
                            Id = Guid.NewGuid(),
                            PatientId = patientId,
                            ContactName = profileDto.EmergencyContactName ?? string.Empty,
                            ContactEmail = profileDto.EmergencyContactEmail ?? string.Empty,
                            ContactPhone = profileDto.EmergencyContactPhone ?? string.Empty,
                            RelationshipToPatient = profileDto.EmergencyContactRelationship ?? string.Empty
                        };
                        dbContext.Set<Patient_Emergency_Contact>().Add(patient.EmergencyContact);
                    }
                    else if (patient.EmergencyContact != null)
                    {
                        if (profileDto.EmergencyContactName != null)
                            patient.EmergencyContact.ContactName = profileDto.EmergencyContactName;
                        if (profileDto.EmergencyContactEmail != null)
                            patient.EmergencyContact.ContactEmail = profileDto.EmergencyContactEmail;
                        if (profileDto.EmergencyContactPhone != null)
                            patient.EmergencyContact.ContactPhone = profileDto.EmergencyContactPhone;
                        if (profileDto.EmergencyContactRelationship != null)
                            patient.EmergencyContact.RelationshipToPatient = profileDto.EmergencyContactRelationship;
                    }

                    // Mark the patient as having completed additional info
                    patient.HasCompletedAdditionalInfo = true;
                    patient.AdditionalInfoCompletedAt = DateTime.UtcNow;

                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Fetch the updated patient with all related data
                    var updatedPatient = await dbContext.Patients
                        .Include(p => p.ContactInfo)
                        .Include(p => p.MedicalRelatedInfo)
                        .Include(p => p.EmergencyContact)
                        .FirstOrDefaultAsync(p => p.PatientId == patientId);

                    return Ok(new {
                        message = "Profile updated successfully",
                        patient = updatedPatient != null ? BuildPatientProfileResponse(updatedPatient) : BuildPatientProfileResponse(patient)
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new { message = "An error occurred while updating profile", error = ex.Message });
                }
            }
        }

        private static object BuildPatientProfileResponse(Patient patient)
        {
            return new
            {
                patientId = patient.PatientId,
                userId = patient.UserId,
                firstName = patient.FirstName,
                lastName = patient.LastName,
                dateOfBirth = patient.DateOfBirth,
                gender = patient.Gender,
                imageUrl = patient.ImageUrl,

                contactInfo = patient.ContactInfo == null ? null : new
                {
                    phoneNumber = patient.ContactInfo.PhoneNumber,
                    emailAddress = patient.ContactInfo.EmailAddress,
                    addressLine1 = patient.ContactInfo.AddressLine1,
                    addressLine2 = patient.ContactInfo.AddressLine2,
                    city = patient.ContactInfo.City,
                    state = patient.ContactInfo.State,
                    postalCode = patient.ContactInfo.PostalCode,
                    country = patient.ContactInfo.Country,
                    nationality = patient.ContactInfo.Nationality
                },

                medicalRelatedInfo = patient.MedicalRelatedInfo == null ? null : new
                {
                    bloodType = patient.MedicalRelatedInfo.BloodType,
                    allergies = patient.MedicalRelatedInfo.Allergies,
                    chronicConditions = patient.MedicalRelatedInfo.ChronicConditions
                },

                emergencyContact = patient.EmergencyContact == null ? null : new
                {
                    contactName = patient.EmergencyContact.ContactName,
                    contactEmail = patient.EmergencyContact.ContactEmail,
                    contactPhone = patient.EmergencyContact.ContactPhone,
                    relationshipToPatient = patient.EmergencyContact.RelationshipToPatient
                }
            };
        }

        /// <summary>
        /// Skip additional info and send reminder email
        /// </summary>
        [HttpPost("{patientId}/skip-additional-info")]
        public async Task<IActionResult> SkipAdditionalInfo(Guid patientId)
        {
            try
            {
                var patient = await patientRepository.GetPatientWithDetailsAsync(patientId);
                if (patient == null)
                    return NotFound(new { message = "Patient not found" });

                patient.AdditionalInfoReminderSent = true;
                patient.AdditionalInfoReminderSentAt = DateTime.UtcNow;

                await dbContext.SaveChangesAsync();

                // Send reminder email
                var user = await dbContext.Users.FindAsync(patient.UserId);
                if (user != null)
                {
                    await emailService.SendEmailAsync(
                        user.Email, 
                        "Complete Your Patient Profile", 
                        $"Hello {patient.FirstName},<br/><br/>We noticed you haven't completed your medical profile yet. Please log in and complete your profile to help us serve you better.<br/><br/>Best regards,<br/>MediBridge Team"
                    );
                }

                return Ok(new { message = "Reminder will be sent to your email" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if patient has completed additional info
        /// </summary>
        [HttpGet("{patientId}/additional-info-status")]
        public async Task<ActionResult> GetAdditionalInfoStatus(Guid patientId)
        {
            try
            {
                var patient = await patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                    return NotFound(new { message = "Patient not found" });

                return Ok(new 
                { 
                    hasCompletedAdditionalInfo = patient.HasCompletedAdditionalInfo,
                    completedAt = patient.AdditionalInfoCompletedAt,
                    reminderSent = patient.AdditionalInfoReminderSent,
                    reminderSentAt = patient.AdditionalInfoReminderSentAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }
    }
}
