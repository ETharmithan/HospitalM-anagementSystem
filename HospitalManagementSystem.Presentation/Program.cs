
using System.Text;
using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Application.Services;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HospitalManagementSystem.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // change
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("HospitalDB")));

            builder.Services.AddCors();

            builder.Services.AddScoped<ITokenService, TokenService>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => // Configure the JWT bearer authentication options. This will specify how the JWT token should be validated and used for authentication.
                {
                var TokenKey = builder.Configuration["TokenKey"] // Get the secret key used to sign the JWT tokens from the configuration file. This key is used to validate the token's signature and ensure that it has not been tampered with.
                    ?? throw new InvalidOperationException("TokenKey is not configured."); // If the TokenKey is not found in the configuration, throw an exception to indicate that the application cannot start without it. This is a safeguard to ensure that the application does not start with an invalid or missing key.
                options.TokenValidationParameters = new TokenValidationParameters // Create a new instance of the TokenValidationParameters class to specify the validation parameters for the JWT token.
                {
                    ValidateIssuerSigningKey = true, // Enable validation of the token's signing key to ensure that it is valid and has not been tampered with. This is a critical security measure to prevent unauthorized access to the API.
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenKey)), // Set the signing key to the secret key obtained from the configuration file. This key is used to validate the token's signature.
                    ValidateIssuer = false, // Disable validation of the token's issuer. This means that the token can be issued by any trusted authority and does not need to match a specific issuer. This is useful for applications that use multiple issuers or want to allow tokens issued by different authorities.
                    ValidateAudience = false // Disable validation of the token's audience. This means that the token can be used by any audience and does not need to match a specific audience. This is useful for applications that want to allow tokens to be used by multiple audiences or do not require strict audience validation.
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

            app.UseCors(policy =>
                policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .WithOrigins("http://localhost:4200", "https://localhost:4200")); // Replace with your Angular app's URL

            app.UseAuthentication();
            app.UseAuthorization();


        


            app.MapControllers();

            app.Run();
        }
    }
}
