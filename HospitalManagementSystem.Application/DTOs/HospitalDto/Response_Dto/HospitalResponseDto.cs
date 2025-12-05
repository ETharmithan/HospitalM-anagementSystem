using DepartmentResponseDto = HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto.DepartmentResponseDto;

namespace HospitalManagementSystem.Application.DTOs.HospitalDto.Response_Dto
{
    public class HospitalResponseDto
    {
        public Guid HospitalId { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string Country { get; set; }
        public required string PostalCode { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public List<DepartmentResponseDto> Departments { get; set; } = new();
        public List<HospitalAdminResponseDto> HospitalAdmins { get; set; } = new();
    }
}
