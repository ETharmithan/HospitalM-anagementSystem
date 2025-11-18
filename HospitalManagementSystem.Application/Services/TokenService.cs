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

        public string CreateToken(UserLogin user)

        {
            //if (patient == null) throw new ArgumentNullException(nameof(patient));

            var tokenKey = configuration["TokenKey"] ?? throw new Exception("Token key not found in configuration");


            if (tokenKey.Length < 64) throw new Exception("Token key must be at least 64 characters long");

            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

            var claims = new List<Claim>
        {
            // new Claim(ClaimTypes.Email, user.Email),

            new(ClaimTypes.Email, user.Email),

            //new(ClaimTypes.NameIdentifier, patient.PatientId.ToString())

        };

            var creds = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds
            };


            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
