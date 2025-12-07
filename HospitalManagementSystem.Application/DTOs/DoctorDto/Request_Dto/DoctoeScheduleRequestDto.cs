using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto
{
    public class DoctorScheduleRequestDto
    {
        // For specific date scheduling
        public DateTime? ScheduleDate { get; set; }
        
        // For recurring weekly schedules
        [RegularExpression("^(Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday)$",
            ErrorMessage = "Invalid day of week")]
        public string? DayOfWeek { get; set; }
        
        public bool IsRecurring { get; set; } = false;

        [Required(ErrorMessage = "Start time is required")]
        [RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$",
            ErrorMessage = "Invalid start time format (HH:mm)")]
        public string StartTime { get; set; } = null!;

        [Required(ErrorMessage = "End time is required")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$",
            ErrorMessage = "Invalid end time format (HH:mm)")]
        public string EndTime { get; set; } = null!;

        // DoctorId is optional - will be set by backend from JWT token for Doctor role
        public Guid DoctorId { get; set; }

        [Required(ErrorMessage = "HospitalId is required")]
        public Guid HospitalId { get; set; }
    }
}
