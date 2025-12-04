
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
    }
}
