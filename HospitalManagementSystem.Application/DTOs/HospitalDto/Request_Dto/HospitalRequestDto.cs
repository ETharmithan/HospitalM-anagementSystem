using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Application.DTOs.HospitalDto.Request_Dto
{
    public class HospitalRequestDto
    {
        [Required]
        [StringLength(200)]
        public required string Name { get; set; }

        [Required]
        [StringLength(500)]
        public required string Address { get; set; }

        [Required]
        [StringLength(100)]
        public required string City { get; set; }

        [Required]
        [StringLength(100)]
        public required string State { get; set; }

        [Required]
        [StringLength(100)]
        public required string Country { get; set; }

        [Required]
        [StringLength(20)]
        public required string PostalCode { get; set; }

        [Required]
        [StringLength(20)]
        public required string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public required string Email { get; set; }

        [StringLength(500)]
        public string? Website { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }
    }
}
