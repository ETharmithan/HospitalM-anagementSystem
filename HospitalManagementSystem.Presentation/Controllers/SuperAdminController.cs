using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Application.IServices.DoctorIServices;
using HospitalManagementSystem.Application.DTOs.HospitalDto.Request_Dto;
using HospitalManagementSystem.Application.DTOs.HospitalDto.Response_Dto;
using HospitalManagementSystem.Application.DTOs.AdminDto;
using HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
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
        private readonly AppDbContext _dbContext;

        public SuperAdminController(
            IHospitalService hospitalService,
            IDepartmentService departmentService,
            IUserRepository userRepository,
            AppDbContext dbContext)
        {
            _hospitalService = hospitalService;
            _departmentService = departmentService;
            _userRepository = userRepository;
            _dbContext = dbContext;
        }

        // Hospital Management
        
        /// <summary>
        /// Create hospital with admin in a single transaction
        /// </summary>
        [HttpPost("hospitals/with-admin")]
        public async Task<ActionResult> CreateHospitalWithAdmin([FromBody] CreateHospitalWithAdminDto dto)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Check if email already exists
                var existingUser = await _userRepository.GetByEmailAsync(dto.AdminEmail);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Email is already taken" });
                }

                // Create hospital
                var hospital = new Hospital
                {
                    HospitalId = Guid.NewGuid(),
                    Name = dto.Name,
                    Address = dto.Address,
                    City = dto.City,
                    State = dto.State,
                    Country = dto.Country,
                    PostalCode = dto.PostalCode,
                    PhoneNumber = dto.PhoneNumber,
                    Email = dto.Email,
                    Website = dto.Website,
                    Description = dto.Description,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Hospitals.Add(hospital);
                await _dbContext.SaveChangesAsync();

                // Create admin user
                using var hmac = new HMACSHA512();
                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = dto.AdminDisplayName,
                    Email = dto.AdminEmail,
                    Role = "Admin",
                    PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.AdminPassword)),
                    PasswordSalt = hmac.Key,
                    IsEmailVerified = true // Auto-verify for admin created by superadmin
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                // Assign admin to hospital
                var hospitalAdmin = new HospitalAdmin
                {
                    HospitalAdminId = Guid.NewGuid(),
                    HospitalId = hospital.HospitalId,
                    UserId = user.UserId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.HospitalAdmins.Add(hospitalAdmin);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Hospital and admin created successfully",
                    hospitalId = hospital.HospitalId,
                    userId = user.UserId
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Failed to create hospital with admin", error = ex.Message });
            }
        }

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

        [HttpGet("hospitals/{hospitalId}/doctors")]
        public async Task<ActionResult<List<DoctorResponseDto>>> GetHospitalDoctors(Guid hospitalId)
        {
            try
            {
                var doctors = await _dbContext.Doctors
                    .Include(d => d.Department)
                    .Where(d => d.DepartmentId != null && d.Department != null && d.Department.HospitalId == hospitalId)
                    .ToListAsync();

                var result = doctors.Select(d => new DoctorResponseDto
                {
                    DoctorId = d.DoctorId,
                    Name = d.Name,
                    Email = d.Email,
                    Phone = d.Phone,
                    Qualification = d.Qualification,
                    LicenseNumber = d.LicenseNumber,
                    Status = d.Status,
                    ProfileImage = d.ProfileImage,
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.Department != null ? d.Department.Name : null
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve hospital doctors", error = ex.Message });
            }
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

        // Admin User Management (CRUD operations on admin users themselves)
        
        /// <summary>
        /// Get all admin users across all hospitals
        /// </summary>
        [HttpGet("admins")]
        public async Task<ActionResult> GetAllAdmins()
        {
            try
            {
                var admins = await _dbContext.Users
                    .Where(u => u.Role == "Admin")
                    .Select(u => new
                    {
                        userId = u.UserId,
                        username = u.Username,
                        email = u.Email,
                        isEmailVerified = u.IsEmailVerified,
                        hospitals = _dbContext.HospitalAdmins
                            .Where(ha => ha.UserId == u.UserId && ha.IsActive)
                            .Select(ha => new
                            {
                                hospitalId = ha.HospitalId,
                                hospitalName = ha.Hospital.Name,
                                assignedAt = ha.CreatedAt
                            })
                            .ToList()
                    })
                    .ToListAsync();

                return Ok(admins);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch admins", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new admin user
        /// </summary>
        [HttpPost("admins")]
        public async Task<ActionResult> CreateAdmin([FromBody] CreateAdminDto dto)
        {
            try
            {
                // Check if email already exists
                var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Email is already taken" });
                }

                // Create admin user
                using var hmac = new HMACSHA512();
                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = dto.Username,
                    Email = dto.Email,
                    Role = "Admin",
                    PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password)),
                    PasswordSalt = hmac.Key,
                    IsEmailVerified = true // Auto-verify for admin created by superadmin
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                // Optionally assign to hospital if provided
                if (dto.HospitalId.HasValue)
                {
                    var hospital = await _dbContext.Hospitals.FindAsync(dto.HospitalId.Value);
                    if (hospital != null)
                    {
                        var hospitalAdmin = new HospitalAdmin
                        {
                            HospitalAdminId = Guid.NewGuid(),
                            HospitalId = dto.HospitalId.Value,
                            UserId = user.UserId,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _dbContext.HospitalAdmins.Add(hospitalAdmin);
                        await _dbContext.SaveChangesAsync();
                    }
                }

                return Ok(new
                {
                    message = "Admin created successfully",
                    userId = user.UserId,
                    username = user.Username,
                    email = user.Email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create admin", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an admin user
        /// </summary>
        [HttpPut("admins/{userId}")]
        public async Task<ActionResult> UpdateAdmin(Guid userId, [FromBody] UpdateAdminDto dto)
        {
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null || user.Role != "Admin")
                {
                    return NotFound(new { message = "Admin not found" });
                }

                // Check if new email is already taken by another user
                if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
                {
                    var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
                    if (existingUser != null)
                    {
                        return BadRequest(new { message = "Email is already taken" });
                    }
                    user.Email = dto.Email;
                }

                // Update username if provided
                if (!string.IsNullOrEmpty(dto.Username))
                {
                    user.Username = dto.Username;
                }

                // Update password if provided
                if (!string.IsNullOrEmpty(dto.Password))
                {
                    using var hmac = new HMACSHA512();
                    user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));
                    user.PasswordSalt = hmac.Key;
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Admin updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update admin", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete an admin user (removes user and all hospital assignments)
        /// </summary>
        [HttpDelete("admins/{userId}")]
        public async Task<ActionResult> DeleteAdmin(Guid userId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null || user.Role != "Admin")
                {
                    return NotFound(new { message = "Admin not found" });
                }

                // Remove all hospital admin assignments
                var hospitalAdmins = await _dbContext.HospitalAdmins
                    .Where(ha => ha.UserId == userId)
                    .ToListAsync();

                _dbContext.HospitalAdmins.RemoveRange(hospitalAdmins);

                // Remove the user
                _dbContext.Users.Remove(user);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Admin deleted successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Failed to delete admin", error = ex.Message });
            }
        }

        // Patient Management (View all patients across all hospitals)
        
        /// <summary>
        /// Get all patients in the system
        /// </summary>
        [HttpGet("patients")]
        public async Task<ActionResult> GetAllPatients()
        {
            try
            {
                var patients = await _dbContext.Patients
                    .Include(p => p.ContactInfo)
                    .Include(p => p.IdentificationDetails)
                    .Select(p => new
                    {
                        patientId = p.PatientId,
                        firstName = p.FirstName,
                        lastName = p.LastName,
                        email = p.ContactInfo != null ? p.ContactInfo.EmailAddress : "",
                        phoneNumber = p.ContactInfo != null ? p.ContactInfo.PhoneNumber : "",
                        dateOfBirth = p.DateOfBirth,
                        gender = p.Gender,
                        nic = p.IdentificationDetails != null ? p.IdentificationDetails.NIC : "",
                        city = p.ContactInfo != null ? p.ContactInfo.City : "",
                        province = p.ContactInfo != null ? p.ContactInfo.State : "",
                        country = p.ContactInfo != null ? p.ContactInfo.Country : "",
                        imageUrl = p.ImageUrl,
                        appointmentCount = _dbContext.DoctorAppointments.Count(a => a.PatientId == p.PatientId)
                    })
                    .ToListAsync();

                return Ok(patients);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch patients", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all users in the system (all roles)
        /// </summary>
        [HttpGet("users")]
        public async Task<ActionResult> GetAllUsers()
        {
            try
            {
                var users = await _dbContext.Users
                    .Select(u => new
                    {
                        userId = u.UserId,
                        username = u.Username,
                        email = u.Email,
                        role = u.Role,
                        isEmailVerified = u.IsEmailVerified
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch users", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a patient
        /// </summary>
        [HttpDelete("patients/{patientId}")]
        public async Task<ActionResult> DeletePatient(Guid patientId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var patient = await _dbContext.Patients.FindAsync(patientId);
                if (patient == null)
                {
                    return NotFound(new { message = "Patient not found" });
                }

                // Remove all appointments for this patient
                var appointments = await _dbContext.DoctorAppointments
                    .Where(a => a.PatientId == patientId)
                    .ToListAsync();
                _dbContext.DoctorAppointments.RemoveRange(appointments);

                // Remove the patient
                _dbContext.Patients.Remove(patient);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Patient deleted successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Failed to delete patient", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a user (any role)
        /// </summary>
        [HttpDelete("users/{userId}")]
        public async Task<ActionResult> DeleteUser(Guid userId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Handle role-specific cleanup
                if (user.Role == "Admin")
                {
                    // Remove hospital admin assignments
                    var hospitalAdmins = await _dbContext.HospitalAdmins
                        .Where(ha => ha.UserId == userId)
                        .ToListAsync();
                    _dbContext.HospitalAdmins.RemoveRange(hospitalAdmins);
                }
                else if (user.Role == "Doctor")
                {
                    // Find and remove doctor record
                    var doctor = await _dbContext.Doctors
                        .FirstOrDefaultAsync(d => d.Email == user.Email);
                    if (doctor != null)
                    {
                        // Remove doctor schedules
                        var schedules = await _dbContext.DoctorSchedules
                            .Where(s => s.DoctorId == doctor.DoctorId)
                            .ToListAsync();
                        _dbContext.DoctorSchedules.RemoveRange(schedules);

                        // Remove doctor appointments
                        var appointments = await _dbContext.DoctorAppointments
                            .Where(a => a.DoctorId == doctor.DoctorId)
                            .ToListAsync();
                        _dbContext.DoctorAppointments.RemoveRange(appointments);

                        _dbContext.Doctors.Remove(doctor);
                    }
                }
                else if (user.Role == "Patient")
                {
                    // Find and remove patient record by UserId
                    var patient = await _dbContext.Patients
                        .Include(p => p.ContactInfo)
                        .Include(p => p.IdentificationDetails)
                        .Include(p => p.MedicalHistory)
                        .Include(p => p.MedicalRelatedInfo)
                        .Include(p => p.EmergencyContact)
                        .FirstOrDefaultAsync(p => p.UserId == userId);
                    if (patient != null)
                    {
                        // Remove patient appointments
                        var appointments = await _dbContext.DoctorAppointments
                            .Where(a => a.PatientId == patient.PatientId)
                            .ToListAsync();
                        _dbContext.DoctorAppointments.RemoveRange(appointments);

                        // Remove related patient records
                        if (patient.ContactInfo != null)
                            _dbContext.Remove(patient.ContactInfo);
                        if (patient.IdentificationDetails != null)
                            _dbContext.Remove(patient.IdentificationDetails);
                        if (patient.MedicalHistory != null)
                            _dbContext.Remove(patient.MedicalHistory);
                        if (patient.MedicalRelatedInfo != null)
                            _dbContext.Remove(patient.MedicalRelatedInfo);
                        if (patient.EmergencyContact != null)
                            _dbContext.Remove(patient.EmergencyContact);

                        _dbContext.Patients.Remove(patient);
                    }
                }

                // Remove the user
                _dbContext.Users.Remove(user);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Failed to delete user", error = ex.Message });
            }
        }

        // Get hospital details with statistics
        [HttpGet("hospitals/{hospitalId}/details")]
        public async Task<ActionResult<HospitalDetailsDto>> GetHospitalDetails(Guid hospitalId)
        {
            try
            {
                var hospital = await _dbContext.Hospitals
                    .Include(h => h.Departments)
                        .ThenInclude(d => d.Doctors)
                    .Include(h => h.HospitalAdmins)
                        .ThenInclude(ha => ha.User)
                    .FirstOrDefaultAsync(h => h.HospitalId == hospitalId);

                if (hospital == null)
                    return NotFound(new { message = "Hospital not found" });

                // Get appointment statistics
                var allAppointments = await _dbContext.DoctorAppointments
                    .Where(a => a.HospitalId == hospitalId)
                    .ToListAsync();

                var now = DateTime.UtcNow;
                var upcomingBookings = allAppointments.Count(a => 
                    a.AppointmentDate >= now && 
                    a.AppointmentStatus != "Cancelled");

                var completedBookings = allAppointments.Count(a => 
                    a.AppointmentStatus == "Completed");

                var cancelledBookings = allAppointments.Count(a => 
                    a.AppointmentStatus == "Cancelled");

                var details = new HospitalDetailsDto
                {
                    HospitalId = hospital.HospitalId,
                    Name = hospital.Name,
                    Address = hospital.Address,
                    City = hospital.City,
                    State = hospital.State,
                    Country = hospital.Country,
                    PostalCode = hospital.PostalCode,
                    Latitude = hospital.Latitude,
                    Longitude = hospital.Longitude,
                    PhoneNumber = hospital.PhoneNumber,
                    Email = hospital.Email,
                    Website = hospital.Website,
                    Description = hospital.Description,
                    IsActive = hospital.IsActive,
                    CreatedAt = hospital.CreatedAt,
                    
                    TotalDepartments = hospital.Departments?.Count ?? 0,
                    TotalDoctors = hospital.Departments?.Sum(d => d.Doctors?.Count ?? 0) ?? 0,
                    TotalAdmins = hospital.HospitalAdmins?.Count ?? 0,
                    TotalBookings = allAppointments.Count,
                    UpcomingBookings = upcomingBookings,
                    CompletedBookings = completedBookings,
                    CancelledBookings = cancelledBookings,
                    
                    Departments = hospital.Departments?.Select(d => new DepartmentSummaryDto
                    {
                        DepartmentId = d.DepartmentId,
                        Name = d.Name,
                        Description = d.Description,
                        DoctorCount = d.Doctors?.Count ?? 0
                    }).ToList() ?? new List<DepartmentSummaryDto>(),
                    
                    Admins = hospital.HospitalAdmins?.Select(ha => new AdminSummaryDto
                    {
                        UserId = ha.UserId,
                        Username = ha.User.Username,
                        Email = ha.User.Email,
                        IsActive = ha.IsActive
                    }).ToList() ?? new List<AdminSummaryDto>()
                };

                return Ok(details);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching hospital details", error = ex.Message });
            }
        }
    }
}
