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
    public class AccountController(
        IUserRepository userRepository, 
        ITokenService tokenService,
        IEmailService emailService) : BaseApiController
    {
        [Authorize]
        [HttpGet]
        public ActionResult<string> Login_()  
        {
            return "Login Successful";
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<User_Dto>> Register(RegisterUserDto registerUserDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Check if user exists
            var existingUser = await userRepository.GetByEmailAsync(registerUserDto.Email);
            if (existingUser != null) 
            {
                return BadRequest("Email is already taken");
            }
            
            using var hmac = new HMACSHA512();
            
            // Generate OTP for email verification
            var otp = emailService.GenerateOtp();
            
            var newUser = new User
            {
                Username = registerUserDto.DisplayName,
                Email = registerUserDto.Email,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerUserDto.Password)),
                PasswordSalt = hmac.Key,
                Role = registerUserDto.Role,
                ImageUrl = registerUserDto.ImageUrl,
                IsEmailVerified = false,
                VerificationOtp = otp,
                OtpExpiryTime = DateTime.UtcNow.AddMinutes(10),
                OtpAttempts = 0
            };

            await userRepository.AddAsync(newUser);
            await userRepository.SaveChangesAsync();

            // Send OTP email
            var emailSent = await emailService.SendOtpEmailAsync(newUser.Email, otp, newUser.Username);
            
            var response = newUser.ToDto(tokenService);
            
            return Ok(new { 
                userId = newUser.UserId.ToString(),
                user = response, 
                emailSent = emailSent,
                requiresVerification = true,
                message = emailSent 
                    ? "Registration successful! Please check your email for the verification code." 
                    : "Registration successful but email could not be sent. Please request a new OTP."
            });
        }

        [AllowAnonymous]
        [HttpPost("verify-email")]
        public async Task<ActionResult> VerifyEmail(VerifyOtpDto verifyOtpDto)
        {
            var user = await userRepository.GetByEmailAsync(verifyOtpDto.Email);
            if (user == null)
                return NotFound("User not found");

            if (user.IsEmailVerified)
                return Ok(new { message = "Email is already verified" });

            // Check OTP attempts (max 5)
            if (user.OtpAttempts >= 5)
            {
                return BadRequest("Too many failed attempts. Please request a new OTP.");
            }

            // Check if OTP is expired
            if (user.OtpExpiryTime == null || user.OtpExpiryTime < DateTime.UtcNow)
            {
                return BadRequest("OTP has expired. Please request a new one.");
            }

            // Verify OTP
            if (user.VerificationOtp != verifyOtpDto.Otp)
            {
                user.OtpAttempts++;
                await userRepository.SaveChangesAsync();
                return BadRequest($"Invalid OTP. {5 - user.OtpAttempts} attempts remaining.");
            }

            // OTP is valid - verify email
            user.IsEmailVerified = true;
            user.VerificationOtp = null;
            user.OtpExpiryTime = null;
            user.OtpAttempts = 0;
            await userRepository.SaveChangesAsync();

            return Ok(new { 
                message = "Email verified successfully!", 
                user = user.ToDto(tokenService) 
            });
        }

        [AllowAnonymous]
        [HttpPost("resend-otp")]
        public async Task<ActionResult> ResendOtp(ResendOtpDto resendOtpDto)
        {
            var user = await userRepository.GetByEmailAsync(resendOtpDto.Email);
            if (user == null)
                return NotFound("User not found");

            if (user.IsEmailVerified)
                return Ok(new { message = "Email is already verified" });

            // Generate new OTP
            var otp = emailService.GenerateOtp();
            user.VerificationOtp = otp;
            user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(10);
            user.OtpAttempts = 0;
            await userRepository.SaveChangesAsync();

            // Send OTP email
            var emailSent = await emailService.SendOtpEmailAsync(user.Email, otp, user.Username);

            if (!emailSent)
                return StatusCode(500, "Failed to send verification email. Please try again.");

            return Ok(new { message = "A new verification code has been sent to your email." });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<User_Dto>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await userRepository.GetByEmailAsync(loginDto.Email);
            if (user == null) return Unauthorized("Invalid email address");

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

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

            // Check if email is verified
            if (!user.IsEmailVerified)
            {
                return Ok(new { 
                    requiresVerification = true,
                    email = user.Email,
                    message = "Please verify your email before logging in."
                });
            }

            return user.ToDto(tokenService);
        }

        private async Task<bool> UserExists(string email)
        {
            var user = await userRepository.GetByEmailAsync(email);
            return user != null;
        }
    }

    public class VerifyOtpDto
    {
        public required string Email { get; set; }
        public required string Otp { get; set; }
    }

    public class ResendOtpDto
    {
        public required string Email { get; set; }
    }
}
