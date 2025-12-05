
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
                ImageUrl = ""
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
                ImageUrl = ""
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
    }
}
