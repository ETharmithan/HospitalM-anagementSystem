using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Domain.Models;
using HospitalManagementSystem.Infrastructure.Data;
using HospitalManagementSystem.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace HospitalManagementSystem.Presentation.Controllers
{
     [Authorize]
    public class AccountController(AppDbContext dbContext, ITokenService tokenService) : BaseApiController
    {
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

            //if (await UserExists(registerUserDto.Email)) return BadRequest("Email is already taken");
            using var hmac = new HMACSHA512();
            var user = new User
            {
                Username = registerUserDto.DisplayName,
                Email = registerUserDto.Email,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerUserDto.Password)),
                PasswordSalt = hmac.Key,
                Role = registerUserDto.Role
            };

            dbContext.Users.Add(user);



            await dbContext.SaveChangesAsync();

            return user.ToDto(tokenService);
        }


        [HttpPost("login")] // http://localhost:5170/api/account/login
        public async Task<ActionResult<User_Dto>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Email == loginDto.Email.ToLower());
            if (user == null) return Unauthorized("Invalid email address");

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password)); // compute the hash of the password provided during login

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

            return user.ToDto(tokenService);
        }






        private async Task<bool> UserExists(string email)
        {
            return await dbContext.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
        }

    }
}
