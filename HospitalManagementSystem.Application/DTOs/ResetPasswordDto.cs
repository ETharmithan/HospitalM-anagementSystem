using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Application.DTOs
{
    public class ResetPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }

    public class ResetPasswordDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        
        [Required]
        public required string ResetToken { get; set; }
        
        [Required]
        [MinLength(6)]
        public required string NewPassword { get; set; }
        
        [Required]
        [Compare(nameof(NewPassword))]
        public required string ConfirmNewPassword { get; set; }
    }
}
