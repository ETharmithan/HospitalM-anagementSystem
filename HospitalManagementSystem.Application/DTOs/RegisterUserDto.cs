using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.DTOs
{
    public class RegisterUserDto
    {
        [Required]
        public string DisplayName { get; set; } = "";
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
        [Required]
        public string Password { get; set; } = "";
        public required string Role { get; set; }
        public string? ImageUrl { get; set; }
    }
}
