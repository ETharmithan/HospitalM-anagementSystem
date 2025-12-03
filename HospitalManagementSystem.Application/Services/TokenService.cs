using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.Extensions.Configuration;

namespace HospitalManagementSystem.Application.Services
{
    public class TokenService(IConfiguration configuration) : ITokenService
    {

        public string CreateToken(User user)

        {
            //if (patient == null) throw new ArgumentNullException(nameof(patient));

            // Support both string format (Development) and object format (Production)
            var tokenKey = configuration["TokenKey:Key"] ?? configuration["TokenKey"]
                ?? throw new Exception("Token key not found in configuration");
            
            var issuer = configuration["TokenKey:Issuer"] ?? "HospitalManagementSystem";
            var audience = configuration["TokenKey:Audience"] ?? "HospitalManagementSystemClient";

            if (tokenKey.Length < 64) throw new Exception("Token key must be at least 64 characters long");

            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
                    new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
                };

            var creds = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = issuer,
                Audience = audience,
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds
            };


            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
