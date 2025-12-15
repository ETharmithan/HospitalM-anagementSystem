namespace HospitalManagementSystem.Application.DTOs.HospitalDto.Response_Dto
{
    public class HospitalDetailsDto
    {
        public Guid HospitalId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Statistics
        public int TotalDepartments { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalBookings { get; set; }
        public int UpcomingBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        
        // Related data
        public List<DepartmentSummaryDto> Departments { get; set; } = new();
        public List<AdminSummaryDto> Admins { get; set; } = new();
    }

    public class DepartmentSummaryDto
    {
        public Guid DepartmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DoctorCount { get; set; }
    }

    public class AdminSummaryDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
