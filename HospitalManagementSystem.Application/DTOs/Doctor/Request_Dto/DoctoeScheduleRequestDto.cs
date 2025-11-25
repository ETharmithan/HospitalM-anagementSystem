using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.DTOs.Doctor.Request_Dto
{
    public class DoctoeScheduleRequestDto
    {
        [Required(ErrorMessage = "Day of week is required")]
        [RegularExpression("^(Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday)$",
            ErrorMessage = "Invalid day of week")]
        public string DayOfWeek { get; set; } = null!;

        [Required(ErrorMessage = "Start time is required")]
        [RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$",
            ErrorMessage = "Invalid start time format (HH:mm)")]
        public string StartTime { get; set; } = null!;


        [Required(ErrorMessage = "End time is required")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$",
            ErrorMessage = "Invalid end time format (HH:mm)")]
        public string EndTime { get; set; } = null!;


        //ForeignKey
        [Required(ErrorMessage = "DoctorId is required")]
        public Guid DoctorId { get; set; }

    }
}
