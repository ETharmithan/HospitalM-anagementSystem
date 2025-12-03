using System;

namespace HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto
{
    public class DoctorAvailabilityResponseDto
    {
        public Guid AvailabilityId { get; set; }
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;
        public bool IsAvailable { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}

