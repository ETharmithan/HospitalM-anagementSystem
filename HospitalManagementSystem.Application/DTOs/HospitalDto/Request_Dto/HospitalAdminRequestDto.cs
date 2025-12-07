using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Application.DTOs.HospitalDto.Request_Dto
{
    public class HospitalAdminRequestDto
    {
        [Required]
        public Guid HospitalId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [MaxLength(500)]
        public string? ProfileImage { get; set; }
    }
}
