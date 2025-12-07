using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HospitalManagementSystem.Domain.Models;

namespace HospitalManagementSystem.Domain.Models.Doctors
{
    public class DoctorSchedule
    {
        [Key]
        public Guid ScheduleId { get; set; }
        
        // Can be either a specific date OR a day of week for recurring schedules
        public DateTime? ScheduleDate { get; set; }  // For specific date availability
        public string? DayOfWeek { get; set; }       // For recurring weekly schedules (Monday, Tuesday, etc.)
        
        public required string StartTime { get; set; }
        public required string EndTime { get; set; }
        
        public bool IsRecurring { get; set; } = false;  // True = weekly recurring, False = specific date

        public Guid DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        // Hospital where this schedule applies
        public Guid? HospitalId { get; set; }
        public Hospital? Hospital { get; set; }
    }
}
