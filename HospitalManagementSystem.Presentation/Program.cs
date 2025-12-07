
using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Application.IServices.DoctorIServices;
using HospitalManagementSystem.Application.Services;
using HospitalManagementSystem.Application.Services.DoctorServices;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Infrastructure.Data;
using HospitalManagementSystem.Infrastructure.Repositories;
using HospitalManagementSystem.Infrastructure.Repository.Doctor;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HospitalManagementSystem.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // change
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("HMSConnection"), sql => sql.MigrationsAssembly("HospitalManagementSystem.Infrastructure")));

            builder.Services.AddCors();

            // Register Services
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();
            builder.Services.AddScoped<IDoctorService, DoctorService>();
            builder.Services.AddScoped<IDoctorAppointmentRepository, DoctorAppointmentRepository>();
            builder.Services.AddScoped<IDoctorAppointmentService, DoctorAppointmentService>();
            builder.Services.AddScoped<IDoctorScheduleRepository, DoctorScheduleRepository>();
            builder.Services.AddScoped<IDoctorLeaveRepository, DoctorLeaveRepository>();
            builder.Services.AddScoped<IDoctorAvailabilityRepository, DoctorAvailabilityRepository>();
            builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
            builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            
            // Hospital Management Services
            builder.Services.AddScoped<IHospitalRepository, HospitalRepository>();
            builder.Services.AddScoped<IHospitalService, HospitalService>();








            builder.Services.AddScoped<IImageUploadService>(provider => 
                new ImageUploadService(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/patients")));

            // Register Repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
            builder.Services.AddScoped<IPatientRepository, PatientRepository>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // Get token key - support both string format (Development) and object format (Production)
                    var tokenKey = builder.Configuration["TokenKey:Key"] ?? builder.Configuration["TokenKey"]
                        ?? throw new InvalidOperationException("TokenKey is not configured.");
                    
                    var issuer = builder.Configuration["TokenKey:Issuer"] ?? "HospitalManagementSystem";
                    var audience = builder.Configuration["TokenKey:Audience"] ?? "HospitalManagementSystemClient";
                    
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero // Remove clock skew tolerance for stricter validation
                    };
                });


            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            // builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

    //builder.Services.AddSwaggerGen(options =>
    //{
    //    options.DocInclusionPredicate((docName, apiDesc) =>
    //    {
    //        var cad = apiDesc.ActionDescriptor as ControllerActionDescriptor;
    //        if (cad == null) return false;

    //        // Include only controllers marked with [ApiController]
    //        return cad.ControllerTypeInfo.GetCustomAttributes(typeof(ApiControllerAttribute), true).Any();
    //    });
    //});



            var app = builder.Build();

            SeedAdminUser(app);
            SeedDoctorUser(app);
            SeedPatientUser(app);
            SeedHospitalsWithAdmins(app);
            SeedAdditionalPatients(app);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                //app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
                    options.RoutePrefix = string.Empty;
                });
            }

            app.UseHttpsRedirection();

            // Enable static files serving
            app.UseStaticFiles();

            app.UseCors(policy =>
                policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .WithOrigins("http://localhost:4200", "https://localhost:4200")); // Replace with your Angular app's URL

            app.UseAuthentication();
            app.UseAuthorization();


        


            app.MapControllers();

            app.Run();
        }

        private static void SeedAdminUser(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            const string adminEmail = "superadmin@example.com";
            const string adminName = "SuperAdmin";
            const string adminPassword = "Admin@1234";

            if (dbContext.Users.Any(u => u.Email == adminEmail))
            {
                return;
            }

            using var hmac = new System.Security.Cryptography.HMACSHA512();
            var passwordBytes = System.Text.Encoding.UTF8.GetBytes(adminPassword);
            var hash = hmac.ComputeHash(passwordBytes);
            var salt = hmac.Key;

            var adminUser = new HospitalManagementSystem.Domain.Models.User
            {
                UserId = Guid.NewGuid(),
                Username = adminName,
                Email = adminEmail,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = "SuperAdmin",
                ImageUrl = "",
                IsEmailVerified = true // Pre-verified for seeded users
            };

            dbContext.Users.Add(adminUser);
            dbContext.SaveChanges();
        }

        private static void SeedDoctorUser(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            const string doctorEmail = "doctor@example.com";
            const string doctorName = "Dr. John Smith";
            const string doctorPassword = "Doctor@1234";

            // Check if doctor user already exists
            if (dbContext.Users.Any(u => u.Email == doctorEmail))
            {
                return;
            }

            // Create or get department
            var department = dbContext.Departments.FirstOrDefault(d => d.Name == "General Medicine");
            var hospitalId = Guid.NewGuid();
            if (!dbContext.Hospitals.Any(h => h.Name == "Default Hospital"))
            {
                dbContext.Hospitals.Add(new HospitalManagementSystem.Domain.Models.Hospital
                {
                    HospitalId = hospitalId,
                    Name = "Default Hospital",
                    Address = "123 Health Way",
                    City = "Healthyville",
                    State = "Wellness",
                    Country = "Careland",
                    PostalCode = "00000",
                    PhoneNumber = "+10000000000",
                    Email = "info@defaulthospital.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                dbContext.SaveChanges();
            }

            if (department == null)
            {
                department = new HospitalManagementSystem.Domain.Models.Doctors.Department
                {
                    DepartmentId = Guid.NewGuid(),
                    Name = "General Medicine",
                    HospitalId = hospitalId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                dbContext.Departments.Add(department);
                dbContext.SaveChanges();
            }

            // Create doctor user
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            var passwordBytes = System.Text.Encoding.UTF8.GetBytes(doctorPassword);
            var hash = hmac.ComputeHash(passwordBytes);
            var salt = hmac.Key;

            var doctorUser = new HospitalManagementSystem.Domain.Models.User
            {
                UserId = Guid.NewGuid(),
                Username = doctorName,
                Email = doctorEmail,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = "Doctor",
                ImageUrl = "",
                IsEmailVerified = true // Pre-verified for seeded users
            };

            dbContext.Users.Add(doctorUser);
            dbContext.SaveChanges();

            // Create doctor entity
            var doctor = new HospitalManagementSystem.Domain.Models.Doctors.Doctor
            {
                DoctorId = Guid.NewGuid(),
                Name = doctorName,
                Email = doctorEmail,
                Phone = "+1234567890",
                Qualification = "MBBS, MD",
                LicenseNumber = "DOC123456",
                Status = "Active",
                AppointmentDurationMinutes = 30,
                BreakTimeMinutes = 5,
                DepartmentId = department.DepartmentId
            };

            dbContext.Doctors.Add(doctor);
            dbContext.SaveChanges();
        }

        private static void SeedPatientUser(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            const string patientEmail = "patient@example.com";
            const string patientFirstName = "Jane";
            const string patientLastName = "Doe";
            const string patientPassword = "Patient@1234";

            // Check if patient user already exists
            if (dbContext.Users.Any(u => u.Email == patientEmail))
            {
                return;
            }

            // Create patient user
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            var passwordBytes = System.Text.Encoding.UTF8.GetBytes(patientPassword);
            var hash = hmac.ComputeHash(passwordBytes);
            var salt = hmac.Key;

            var patientUser = new HospitalManagementSystem.Domain.Models.User
            {
                UserId = Guid.NewGuid(),
                Username = $"{patientFirstName} {patientLastName}",
                Email = patientEmail,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = "Patient",
                ImageUrl = "",
                IsEmailVerified = true // Pre-verified for seeded users
            };

            dbContext.Users.Add(patientUser);
            dbContext.SaveChanges();

            // Create patient entity with all related entities
            var patientId = Guid.NewGuid();
            var patient = new HospitalManagementSystem.Domain.Models.Patient.Patient
            {
                PatientId = patientId,
                UserId = patientUser.UserId,
                FirstName = patientFirstName,
                LastName = patientLastName,
                DateOfBirth = new DateTime(1990, 5, 15),
                Gender = "Female",
                ImageUrl = "",
                ContactInfo = new HospitalManagementSystem.Domain.Models.Patient.Patient_Contact_Information
                {
                    PatientId = patientId,
                    PhoneNumber = "+1234567890",
                    EmailAddress = patientEmail,
                    AddressLine1 = "123 Main St",
                    AddressLine2 = "Apt 4B",
                    City = "New York",
                    State = "NY",
                    PostalCode = "10001",
                    Country = "USA",
                    Nationality = "American"
                },
                IdentificationDetails = new HospitalManagementSystem.Domain.Models.Patient.Patient_Identification_Details
                {
                    PatientId = patientId,
                    NIC = "123456789V",
                    PassportNumber = "P12345678",
                    DriversLicenseNumber = "DL12345678"
                },
                MedicalHistory = new HospitalManagementSystem.Domain.Models.Patient.Patient_Medical_History
                {
                    PatientId = patientId,
                    PastIllnesses = "None",
                    Surgeries = "None",
                    MedicalHistoryNotes = "No significant medical history"
                },
                MedicalRelatedInfo = new HospitalManagementSystem.Domain.Models.Patient.Patient_Medical_Related_Info
                {
                    PatientId = patientId,
                    BloodType = "O+",
                    Allergies = "None",
                    ChronicConditions = "None"
                },
                EmergencyContact = new HospitalManagementSystem.Domain.Models.Patient.Patient_Emergency_Contact
                {
                    Id = Guid.NewGuid(),
                    PatientId = patientId,
                    ContactName = "John Doe",
                    ContactEmail = "john.doe@example.com",
                    ContactPhone = "+1122334455",
                    RelationshipToPatient = "Spouse"
                }
            };

            dbContext.Patients.Add(patient);
            dbContext.SaveChanges();
        }

        private static void SeedHospitalsWithAdmins(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Hospital 1: City General Hospital
            var hospital1Email = "admin@citygeneralhospital.com";
            if (!dbContext.Users.Any(u => u.Email == hospital1Email))
            {
                var hospital1Id = Guid.NewGuid();
                var admin1UserId = Guid.NewGuid();

                // Create Hospital 1
                var hospital1 = new HospitalManagementSystem.Domain.Models.Hospital
                {
                    HospitalId = hospital1Id,
                    Name = "City General Hospital",
                    Address = "456 Medical Center Drive",
                    City = "Colombo",
                    State = "Western",
                    Country = "Sri Lanka",
                    PostalCode = "00100",
                    PhoneNumber = "+94112345678",
                    Email = "info@citygeneralhospital.com",
                    Website = "https://citygeneralhospital.lk",
                    Description = "A leading multi-specialty hospital providing comprehensive healthcare services.",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                dbContext.Hospitals.Add(hospital1);

                // Create Admin User for Hospital 1
                using var hmac1 = new System.Security.Cryptography.HMACSHA512();
                var admin1User = new HospitalManagementSystem.Domain.Models.User
                {
                    UserId = admin1UserId,
                    Username = "City General Admin",
                    Email = hospital1Email,
                    PasswordHash = hmac1.ComputeHash(System.Text.Encoding.UTF8.GetBytes("Admin@1234")),
                    PasswordSalt = hmac1.Key,
                    Role = "Admin",
                    ImageUrl = "",
                    IsEmailVerified = true
                };
                dbContext.Users.Add(admin1User);

                // Create HospitalAdmin relationship
                var hospitalAdmin1 = new HospitalManagementSystem.Domain.Models.HospitalAdmin
                {
                    HospitalAdminId = Guid.NewGuid(),
                    HospitalId = hospital1Id,
                    UserId = admin1UserId
                };
                dbContext.HospitalAdmins.Add(hospitalAdmin1);

                // Add departments to Hospital 1
                dbContext.Departments.Add(new HospitalManagementSystem.Domain.Models.Doctors.Department
                {
                    DepartmentId = Guid.NewGuid(),
                    Name = "Cardiology",
                    HospitalId = hospital1Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                dbContext.Departments.Add(new HospitalManagementSystem.Domain.Models.Doctors.Department
                {
                    DepartmentId = Guid.NewGuid(),
                    Name = "Neurology",
                    HospitalId = hospital1Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                dbContext.SaveChanges();
            }

            // Hospital 2: Wellness Medical Center
            var hospital2Email = "admin@wellnessmedical.com";
            if (!dbContext.Users.Any(u => u.Email == hospital2Email))
            {
                var hospital2Id = Guid.NewGuid();
                var admin2UserId = Guid.NewGuid();

                // Create Hospital 2
                var hospital2 = new HospitalManagementSystem.Domain.Models.Hospital
                {
                    HospitalId = hospital2Id,
                    Name = "Wellness Medical Center",
                    Address = "789 Healthcare Boulevard",
                    City = "Kandy",
                    State = "Central",
                    Country = "Sri Lanka",
                    PostalCode = "20000",
                    PhoneNumber = "+94812345678",
                    Email = "info@wellnessmedical.com",
                    Website = "https://wellnessmedical.lk",
                    Description = "Your trusted partner in health and wellness.",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                dbContext.Hospitals.Add(hospital2);

                // Create Admin User for Hospital 2
                using var hmac2 = new System.Security.Cryptography.HMACSHA512();
                var admin2User = new HospitalManagementSystem.Domain.Models.User
                {
                    UserId = admin2UserId,
                    Username = "Wellness Admin",
                    Email = hospital2Email,
                    PasswordHash = hmac2.ComputeHash(System.Text.Encoding.UTF8.GetBytes("Admin@1234")),
                    PasswordSalt = hmac2.Key,
                    Role = "Admin",
                    ImageUrl = "",
                    IsEmailVerified = true
                };
                dbContext.Users.Add(admin2User);

                // Create HospitalAdmin relationship
                var hospitalAdmin2 = new HospitalManagementSystem.Domain.Models.HospitalAdmin
                {
                    HospitalAdminId = Guid.NewGuid(),
                    HospitalId = hospital2Id,
                    UserId = admin2UserId
                };
                dbContext.HospitalAdmins.Add(hospitalAdmin2);

                // Add departments to Hospital 2
                dbContext.Departments.Add(new HospitalManagementSystem.Domain.Models.Doctors.Department
                {
                    DepartmentId = Guid.NewGuid(),
                    Name = "Orthopedics",
                    HospitalId = hospital2Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                dbContext.Departments.Add(new HospitalManagementSystem.Domain.Models.Doctors.Department
                {
                    DepartmentId = Guid.NewGuid(),
                    Name = "Pediatrics",
                    HospitalId = hospital2Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                dbContext.SaveChanges();
            }
        }

        private static void SeedAdditionalPatients(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var patients = new[]
            {
                new { Email = "patient2@example.com", FirstName = "Michael", LastName = "Johnson", Gender = "Male", DOB = new DateTime(1985, 3, 20) },
                new { Email = "patient3@example.com", FirstName = "Sarah", LastName = "Williams", Gender = "Female", DOB = new DateTime(1992, 8, 10) }
            };

            foreach (var p in patients)
            {
                if (dbContext.Users.Any(u => u.Email == p.Email))
                    continue;

                using var hmac = new System.Security.Cryptography.HMACSHA512();
                var userId = Guid.NewGuid();
                var patientId = Guid.NewGuid();

                // Create user
                var user = new HospitalManagementSystem.Domain.Models.User
                {
                    UserId = userId,
                    Username = $"{p.FirstName} {p.LastName}",
                    Email = p.Email,
                    PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("Patient@1234")),
                    PasswordSalt = hmac.Key,
                    Role = "Patient",
                    ImageUrl = "",
                    IsEmailVerified = true
                };
                dbContext.Users.Add(user);

                // Create patient
                var patient = new HospitalManagementSystem.Domain.Models.Patient.Patient
                {
                    PatientId = patientId,
                    UserId = userId,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    DateOfBirth = p.DOB,
                    Gender = p.Gender,
                    ImageUrl = "",
                    ContactInfo = new HospitalManagementSystem.Domain.Models.Patient.Patient_Contact_Information
                    {
                        PatientId = patientId,
                        PhoneNumber = "+94771234567",
                        EmailAddress = p.Email,
                        AddressLine1 = "123 Sample Street",
                        City = "Colombo",
                        State = "Western",
                        PostalCode = "00100",
                        Country = "Sri Lanka",
                        Nationality = "Sri Lankan"
                    },
                    IdentificationDetails = new HospitalManagementSystem.Domain.Models.Patient.Patient_Identification_Details
                    {
                        PatientId = patientId,
                        NIC = $"{p.DOB.Year % 100}0{patientId.ToString().Substring(0, 7)}V"
                    },
                    MedicalHistory = new HospitalManagementSystem.Domain.Models.Patient.Patient_Medical_History
                    {
                        PatientId = patientId,
                        PastIllnesses = "None",
                        Surgeries = "None",
                        MedicalHistoryNotes = "No significant medical history"
                    },
                    MedicalRelatedInfo = new HospitalManagementSystem.Domain.Models.Patient.Patient_Medical_Related_Info
                    {
                        PatientId = patientId,
                        BloodType = p.Gender == "Male" ? "A+" : "B+",
                        Allergies = "None",
                        ChronicConditions = "None"
                    },
                    EmergencyContact = new HospitalManagementSystem.Domain.Models.Patient.Patient_Emergency_Contact
                    {
                        Id = Guid.NewGuid(),
                        PatientId = patientId,
                        ContactName = "Emergency Contact",
                        ContactEmail = "emergency@example.com",
                        ContactPhone = "+94771234568",
                        RelationshipToPatient = "Family"
                    }
                };
                dbContext.Patients.Add(patient);
            }

            dbContext.SaveChanges();
        }
    }
}
