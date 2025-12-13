using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Application.DTOs.AppointmentDto
{
    public class CancellationRequestDto
    {
        [Required]
        public required Guid AppointmentId { get; set; }
        
        [Required]
        [MinLength(10, ErrorMessage = "Please provide a reason (minimum 10 characters)")]
        public required string CancellationReason { get; set; }
    }

    public class CancellationApprovalDto
    {
        [Required]
        public required Guid AppointmentId { get; set; }
        
        [Required]
        public required bool Approved { get; set; }
        
        public string? ApprovalNote { get; set; }
    }

    public class AppointmentSearchDto
    {
        public Guid? DoctorId { get; set; }
        public Guid? HospitalId { get; set; }
        public DateTime? PreferredDate { get; set; }
        public string? PreferredTime { get; set; }
        public string? DoctorName { get; set; }
        public string? Specialization { get; set; }
    }
}
