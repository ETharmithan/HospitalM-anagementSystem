using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.Models.DoctorModel
{
    public class DoctorSchedule
    {
        [Key]
        public Guid ScheduleId { get; set; }
        public required string DayOfWeek { get; set; }
        public required string StartTime { get; set; }
        public required string EndTime { get; set; }



        public Guid DoctorId { get; set; }

        public Doctor Doctor { get; set; } = null!;

    }
}
