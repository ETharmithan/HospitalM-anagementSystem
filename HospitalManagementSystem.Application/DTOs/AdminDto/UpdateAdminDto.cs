using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Application.DTOs.AdminDto
{
    public class UpdateAdminDto
    {
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        public string? Username { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string? Password { get; set; }
    }
}
