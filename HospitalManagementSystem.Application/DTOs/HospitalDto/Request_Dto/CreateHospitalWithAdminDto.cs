using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Application.DTOs.HospitalDto.Request_Dto
{
    public class CreateHospitalWithAdminDto
    {
        // Hospital fields
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string Address { get; set; }
        [Required]
        public required string City { get; set; }
        [Required]
        public required string State { get; set; }
        [Required]
        public required string Country { get; set; }
        [Required]
        public required string PostalCode { get; set; }
        [Required]
        public required string PhoneNumber { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        // Admin fields
        [Required]
        [EmailAddress]
        public required string AdminEmail { get; set; }
        [Required]
        [MinLength(6)]
        public required string AdminPassword { get; set; }
        [Required]
        public required string AdminDisplayName { get; set; }
    }
}
