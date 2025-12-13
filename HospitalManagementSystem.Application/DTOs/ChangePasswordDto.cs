using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Application.DTOs
{
    public class ChangePasswordDto
    {
        [Required]
        public required string CurrentPassword { get; set; }
        
        [Required]
        [MinLength(6)]
        public required string NewPassword { get; set; }
        
        [Required]
        [Compare(nameof(NewPassword))]
        public required string ConfirmNewPassword { get; set; }
    }
}
