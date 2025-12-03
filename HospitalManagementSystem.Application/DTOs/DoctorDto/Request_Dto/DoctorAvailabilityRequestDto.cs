using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto
{
    public class DoctorAvailabilityRequestDto
    {
        [Required(ErrorMessage = "DoctorId is required")]
        public Guid DoctorId { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        [RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$",
            ErrorMessage = "Invalid start time format (HH:mm)")]
        public string StartTime { get; set; } = null!;

        [Required(ErrorMessage = "End time is required")]
        [RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$",
            ErrorMessage = "Invalid end time format (HH:mm)")]
        public string EndTime { get; set; } = null!;

        public bool IsAvailable { get; set; } = true;

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}

