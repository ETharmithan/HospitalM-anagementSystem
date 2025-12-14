
using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Application.IServices.DoctorIServices;
using HospitalManagementSystem.Application.Services;
using HospitalManagementSystem.Application.Services.DoctorServices;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Infrastructure.Data;
using HospitalManagementSystem.Infrastructure.Repositories;
using HospitalManagementSystem.Infrastructure.Repository;
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
            builder.Services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();
            builder.Services.AddScoped<IDoctorPatientRecordsRepository, DoctorPatientRecordsRepository>();
            builder.Services.AddScoped<IDoctorPatientRecordsService, DoctorPatientRecordsService>();
            builder.Services.AddScoped<IDoctorLeaveRepository, DoctorLeaveRepository>();
            builder.Services.AddScoped<IDoctorAvailabilityRepository, DoctorAvailabilityRepository>();
            builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
            builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            
            // Hospital Management Services
            builder.Services.AddScoped<IHospitalRepository, HospitalRepository>();
            builder.Services.AddScoped<IHospitalService, HospitalService>();

            // Chat Services
            builder.Services.AddScoped<IChatRepository, ChatRepository>();
            builder.Services.AddScoped<IChatService, ChatService>();

            // SignalR
            builder.Services.AddSignalR();








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

                    // Configure SignalR authentication
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
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
            SeedDoctorUsers(app);
            SeedPatientUsers(app);
            SeedHospitalsWithAdmins(app);
            SeedDoctorSchedules(app);

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
                  .AllowCredentials() // Required for SignalR
                  .WithOrigins("http://localhost:4200", "https://localhost:4200")); // Replace with your Angular app's URL

            app.UseAuthentication();
            app.UseAuthorization();


        


            app.MapControllers();

            // Map SignalR Hub
            app.MapHub<HospitalManagementSystem.Presentation.Hubs.ChatHub>("/hubs/chat");

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
                ImageUrl = "https://randomuser.me/api/portraits/men/6.jpg",
                IsEmailVerified = true // Pre-verified for seeded users
            };

            dbContext.Users.Add(adminUser);
            dbContext.SaveChanges();
        }

        private static void SeedDoctorUsers(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var doctors = new[]
            {
                new { Email = "doctor@example.com", Name = "Dr. John Smith", Password = "Doctor@1234", 
                      Phone = "+1234567890", Qualification = "MBBS, MD", LicenseNumber = "DOC123456",
                      Department = "General Medicine", ImageUrl = "https://randomuser.me/api/portraits/men/1.jpg" },
                new { Email = "dr.sarah.jones@example.com", Name = "Dr. Sarah Jones", Password = "Doctor@1234",
                      Phone = "+1234567891", Qualification = "MBBS, MD (Cardiology)", LicenseNumber = "DOC234567",
                      Department = "Cardiology", ImageUrl = "https://randomuser.me/api/portraits/women/1.jpg" },
                new { Email = "dr.michael.chen@example.com", Name = "Dr. Michael Chen", Password = "Doctor@1234",
                      Phone = "+1234567892", Qualification = "MBBS, MD (Neurology)", LicenseNumber = "DOC345678",
                      Department = "Neurology", ImageUrl = "https://randomuser.me/api/portraits/men/2.jpg" },
                new { Email = "dr.emily.wilson@example.com", Name = "Dr. Emily Wilson", Password = "Doctor@1234",
                      Phone = "+1234567893", Qualification = "MBBS, MD (Pediatrics)", LicenseNumber = "DOC456789",
                      Department = "Pediatrics", ImageUrl = "https://randomuser.me/api/portraits/women/2.jpg" }
            };

            // Create default hospital if needed
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

            foreach (var doctorInfo in doctors)
            {
                // Check if doctor user already exists
                if (dbContext.Users.Any(u => u.Email == doctorInfo.Email))
                {
                    continue;
                }

                // Create or get department
                var department = dbContext.Departments.FirstOrDefault(d => d.Name == doctorInfo.Department);
                if (department == null)
                {
                    department = new HospitalManagementSystem.Domain.Models.Doctors.Department
                    {
                        DepartmentId = Guid.NewGuid(),
                        Name = doctorInfo.Department,
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
                var passwordBytes = System.Text.Encoding.UTF8.GetBytes(doctorInfo.Password);
                var hash = hmac.ComputeHash(passwordBytes);
                var salt = hmac.Key;

                var doctorUser = new HospitalManagementSystem.Domain.Models.User
                {
                    UserId = Guid.NewGuid(),
                    Username = doctorInfo.Name,
                    Email = doctorInfo.Email,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Role = "Doctor",
                    ImageUrl = doctorInfo.ImageUrl,
                    IsEmailVerified = true
                };

                dbContext.Users.Add(doctorUser);
                dbContext.SaveChanges();

                // Create doctor entity
                var doctor = new HospitalManagementSystem.Domain.Models.Doctors.Doctor
                {
                    DoctorId = Guid.NewGuid(),
                    Name = doctorInfo.Name,
                    Email = doctorInfo.Email,
                    Phone = doctorInfo.Phone,
                    Qualification = doctorInfo.Qualification,
                    LicenseNumber = doctorInfo.LicenseNumber,
                    Status = "Active",
                    ProfileImage = doctorInfo.ImageUrl,
                    AppointmentDurationMinutes = 30,
                    BreakTimeMinutes = 5,
                    DepartmentId = department.DepartmentId
                };

                dbContext.Doctors.Add(doctor);
                dbContext.SaveChanges();
            }
        }

        private static void SeedPatientUsers(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var patients = new[]
            {
                new { Email = "patient@example.com", FirstName = "Jane", LastName = "Doe", 
                      Password = "Patient@1234", DOB = new DateTime(1990, 5, 15), Gender = "Female",
                      Phone = "+1234567890", ImageUrl = "https://randomuser.me/api/portraits/women/3.jpg",
                      BloodType = "O+", Address = "123 Main St, Apt 4B, New York, NY 10001" },
                new { Email = "robert.johnson@example.com", FirstName = "Robert", LastName = "Johnson",
                      Password = "Patient@1234", DOB = new DateTime(1985, 8, 22), Gender = "Male",
                      Phone = "+1234567891", ImageUrl = "https://randomuser.me/api/portraits/men/3.jpg",
                      BloodType = "A+", Address = "456 Oak Avenue, Los Angeles, CA 90210" },
                new { Email = "maria.garcia@example.com", FirstName = "Maria", LastName = "Garcia",
                      Password = "Patient@1234", DOB = new DateTime(1992, 3, 10), Gender = "Female",
                      Phone = "+1234567892", ImageUrl = "https://randomuser.me/api/portraits/women/4.jpg",
                      BloodType = "B+", Address = "789 Pine Street, Chicago, IL 60601" },
                new { Email = "james.wilson@example.com", FirstName = "James", LastName = "Wilson",
                      Password = "Patient@1234", DOB = new DateTime(1988, 11, 30), Gender = "Male",
                      Phone = "+1234567893", ImageUrl = "https://randomuser.me/api/portraits/men/4.jpg",
                      BloodType = "AB+", Address = "321 Elm Road, Houston, TX 77001" }
            };

            foreach (var patientInfo in patients)
            {
                // Check if patient user already exists
                if (dbContext.Users.Any(u => u.Email == patientInfo.Email))
                {
                    continue;
                }

                // Create patient user
                using var hmac = new System.Security.Cryptography.HMACSHA512();
                var passwordBytes = System.Text.Encoding.UTF8.GetBytes(patientInfo.Password);
                var hash = hmac.ComputeHash(passwordBytes);
                var salt = hmac.Key;

                var patientUser = new HospitalManagementSystem.Domain.Models.User
                {
                    UserId = Guid.NewGuid(),
                    Username = $"{patientInfo.FirstName} {patientInfo.LastName}",
                    Email = patientInfo.Email,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Role = "Patient",
                    ImageUrl = patientInfo.ImageUrl,
                    IsEmailVerified = true
                };

                dbContext.Users.Add(patientUser);
                dbContext.SaveChanges();

                // Create patient entity with all related entities
                var patientId = Guid.NewGuid();
                var addressParts = patientInfo.Address.Split(", ");
                var patient = new HospitalManagementSystem.Domain.Models.Patient.Patient
                {
                    PatientId = patientId,
                    UserId = patientUser.UserId,
                    FirstName = patientInfo.FirstName,
                    LastName = patientInfo.LastName,
                    DateOfBirth = patientInfo.DOB,
                    Gender = patientInfo.Gender,
                    ImageUrl = patientInfo.ImageUrl,
                    ContactInfo = new HospitalManagementSystem.Domain.Models.Patient.Patient_Contact_Information
                    {
                        PatientId = patientId,
                        PhoneNumber = patientInfo.Phone,
                        EmailAddress = patientInfo.Email,
                        AddressLine1 = addressParts.Length > 0 ? addressParts[0] : "",
                        AddressLine2 = addressParts.Length > 1 ? addressParts[1] : "",
                        City = addressParts.Length > 2 ? addressParts[2] : "",
                        State = addressParts.Length > 3 ? addressParts[3] : "",
                        PostalCode = addressParts.Length > 4 ? addressParts[4] : "",
                        Country = "USA",
                        Nationality = "American"
                    },
                    IdentificationDetails = new HospitalManagementSystem.Domain.Models.Patient.Patient_Identification_Details
                    {
                        PatientId = patientId,
                        NIC = $"{patientInfo.DOB.Year % 100}0{patientId.ToString().Substring(0, 7)}V",
                        PassportNumber = $"P{patientId.ToString().Substring(0, 8)}",
                        DriversLicenseNumber = $"DL{patientId.ToString().Substring(0, 8)}"
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
                        BloodType = patientInfo.BloodType,
                        Allergies = "None",
                        ChronicConditions = "None"
                    },
                    EmergencyContact = new HospitalManagementSystem.Domain.Models.Patient.Patient_Emergency_Contact
                    {
                        Id = Guid.NewGuid(),
                        PatientId = patientId,
                        ContactName = "Emergency Contact",
                        ContactEmail = "emergency@example.com",
                        ContactPhone = "+1122334455",
                        RelationshipToPatient = "Family"
                    }
                };

                dbContext.Patients.Add(patient);
                dbContext.SaveChanges();
            }
        }

        private static void SeedHospitalsWithAdmins(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var hospitals = new[]
            {
                new { 
                    Email = "admin@citygeneralhospital.com", Name = "City General Hospital", 
                    AdminName = "City General Admin", Address = "456 Medical Center Drive", 
                    City = "Colombo", State = "Western", PostalCode = "00100",
                    Phone = "+94112345678", Website = "https://citygeneralhospital.lk",
                    Description = "A leading multi-specialty hospital providing comprehensive healthcare services.",
                    AdminImageUrl = "https://randomuser.me/api/portraits/women/5.jpg",
                    Departments = new[] { "Cardiology", "Neurology", "General Medicine" }
                },
                new { 
                    Email = "admin@wellnessmedical.com", Name = "Wellness Medical Center", 
                    AdminName = "Wellness Admin", Address = "789 Healthcare Boulevard", 
                    City = "Kandy", State = "Central", PostalCode = "20000",
                    Phone = "+94812345678", Website = "https://wellnessmedical.lk",
                    Description = "Your trusted partner in health and wellness.",
                    AdminImageUrl = "https://randomuser.me/api/portraits/men/5.jpg",
                    Departments = new[] { "Orthopedics", "Pediatrics", "General Medicine" }
                },
                new { 
                    Email = "admin@nationalmedical.com", Name = "National Medical Center", 
                    AdminName = "National Admin", Address = "321 Health Park Avenue", 
                    City = "Galle", State = "Southern", PostalCode = "80000",
                    Phone = "+94912345678", Website = "https://nationalmedical.lk",
                    Description = "Advanced medical care with cutting-edge technology and expert physicians.",
                    AdminImageUrl = "https://randomuser.me/api/portraits/women/6.jpg",
                    Departments = new[] { "Emergency", "Surgery", "Internal Medicine" }
                }
            };

            foreach (var hospitalInfo in hospitals)
            {
                // Check if hospital admin already exists
                if (dbContext.Users.Any(u => u.Email == hospitalInfo.Email))
                {
                    continue;
                }

                var hospitalId = Guid.NewGuid();
                var adminUserId = Guid.NewGuid();

                // Create Hospital
                var hospital = new HospitalManagementSystem.Domain.Models.Hospital
                {
                    HospitalId = hospitalId,
                    Name = hospitalInfo.Name,
                    Address = hospitalInfo.Address,
                    City = hospitalInfo.City,
                    State = hospitalInfo.State,
                    Country = "Sri Lanka",
                    PostalCode = hospitalInfo.PostalCode,
                    PhoneNumber = hospitalInfo.Phone,
                    Email = $"info@{hospitalInfo.Name.ToLower().Replace(" ", "")}.com",
                    Website = hospitalInfo.Website,
                    Description = hospitalInfo.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                dbContext.Hospitals.Add(hospital);

                // Create Admin User for hospital
                using var hmac = new System.Security.Cryptography.HMACSHA512();
                var adminUser = new HospitalManagementSystem.Domain.Models.User
                {
                    UserId = adminUserId,
                    Username = hospitalInfo.AdminName,
                    Email = hospitalInfo.Email,
                    PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("Admin@1234")),
                    PasswordSalt = hmac.Key,
                    Role = "Admin",
                    ImageUrl = hospitalInfo.AdminImageUrl,
                    IsEmailVerified = true
                };
                dbContext.Users.Add(adminUser);

                // Create HospitalAdmin relationship
                var hospitalAdmin = new HospitalManagementSystem.Domain.Models.HospitalAdmin
                {
                    HospitalAdminId = Guid.NewGuid(),
                    HospitalId = hospitalId,
                    UserId = adminUserId,
                    ProfileImage = hospitalInfo.AdminImageUrl
                };
                dbContext.HospitalAdmins.Add(hospitalAdmin);

                // Add departments to Hospital
                foreach (var deptName in hospitalInfo.Departments)
                {
                    dbContext.Departments.Add(new HospitalManagementSystem.Domain.Models.Doctors.Department
                    {
                        DepartmentId = Guid.NewGuid(),
                        Name = deptName,
                        HospitalId = hospitalId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                dbContext.SaveChanges();
            }
        }

        
        private static void SeedDoctorSchedules(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Check if schedules already exist
            if (dbContext.DoctorSchedules.Any())
            {
                return;
            }

            // Get all doctors and hospitals
            var doctors = dbContext.Doctors.ToList();
            var hospitals = dbContext.Hospitals.ToList();

            if (!doctors.Any() || !hospitals.Any())
            {
                return;
            }

            var schedules = new List<HospitalManagementSystem.Domain.Models.Doctors.DoctorSchedule>();
            var random = new Random();

            // Create schedules for each doctor - assign to ONE hospital per day (no conflicts)
            foreach (var doctor in doctors)
            {
                // Add availability for the next 14 days (weekdays only)
                for (int i = 1; i <= 14; i++)
                {
                    var scheduleDate = DateTime.Today.AddDays(i);
                    
                    // Skip weekends
                    if (scheduleDate.DayOfWeek == DayOfWeek.Saturday || 
                        scheduleDate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        continue;
                    }

                    // Assign to a random hospital for this day (doctor can only be at one hospital per day)
                    var hospital = hospitals[random.Next(hospitals.Count)];

                    schedules.Add(new HospitalManagementSystem.Domain.Models.Doctors.DoctorSchedule
                    {
                        ScheduleId = Guid.NewGuid(),
                        ScheduleDate = scheduleDate,
                        DayOfWeek = scheduleDate.DayOfWeek.ToString(),
                        IsRecurring = false,
                        StartTime = "09:00",
                        EndTime = "17:00",
                        DoctorId = doctor.DoctorId,
                        HospitalId = hospital.HospitalId
                    });
                }
            }

            dbContext.DoctorSchedules.AddRange(schedules);
            dbContext.SaveChanges();
        }
    }
}
