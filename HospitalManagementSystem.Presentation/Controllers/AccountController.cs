using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Domain.Models;
using HospitalManagementSystem.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace HospitalManagementSystem.Presentation.Controllers
{
    public class AccountController(IUserRepository userRepository, ITokenService tokenService) : BaseApiController
    {
        [Authorize]
        [HttpGet]
        public ActionResult<string> Login_()  
        {
            return "Login Successful";
        }

        [AllowAnonymous]
        [HttpPost("register")] // http://localhost:5170/api/account/register
        public async Task<ActionResult<User_Dto>> Register(RegisterUserDto registerUserDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // For development: Clear existing users with this email to prevent duplicates
            var existingUser = await userRepository.GetByEmailAsync(registerUserDto.Email);
            if (existingUser != null) 
            {
                Console.WriteLine($"Found existing user with email {registerUserDto.Email}, deleting to prevent duplicates...");
                await userRepository.DeleteAsync(existingUser.UserId);
                await userRepository.SaveChangesAsync();
                Console.WriteLine("Existing user deleted, proceeding with new registration");
            }
            using var hmac = new HMACSHA512();
            var user = new User
            {
                Username = registerUserDto.DisplayName,
                Email = registerUserDto.Email,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerUserDto.Password)),
                PasswordSalt = hmac.Key,
                Role = registerUserDto.Role
            };

            await userRepository.AddAsync(user);
            await userRepository.SaveChangesAsync();

            return user.ToDto(tokenService);
        }


        [AllowAnonymous]
        [HttpPost("login")] // http://localhost:5170/api/account/login
        public async Task<ActionResult<User_Dto>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await userRepository.GetByEmailAsync(loginDto.Email);
            if (user == null) return Unauthorized("Invalid email address");

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password)); // compute the hash of the password provided during login

            // Check if hash lengths match
            if (computedHash.Length != user.PasswordHash.Length) 
            {
                return Unauthorized("Invalid password");
            }

            // Compare password hashes
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

            return user.ToDto(tokenService);
        }






        private async Task<bool> UserExists(string email)
        {
            var user = await userRepository.GetByEmailAsync(email);
            return user != null;
        }

    }
}
