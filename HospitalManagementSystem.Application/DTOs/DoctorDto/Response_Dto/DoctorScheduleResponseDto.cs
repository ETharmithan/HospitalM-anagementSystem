using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto
{
    public class DoctorScheduleResponseDto
    {
        public Guid ScheduleId { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public string? DayOfWeek { get; set; }
        public bool IsRecurring { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public Guid? HospitalId { get; set; }
        public string? HospitalName { get; set; }
    }
}
