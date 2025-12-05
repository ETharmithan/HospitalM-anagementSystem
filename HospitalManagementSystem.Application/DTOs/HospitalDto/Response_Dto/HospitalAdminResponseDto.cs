namespace HospitalManagementSystem.Application.DTOs.HospitalDto.Response_Dto
{
    public class HospitalAdminResponseDto
    {
        public Guid HospitalAdminId { get; set; }
        public Guid HospitalId { get; set; }
        public string? HospitalName { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
