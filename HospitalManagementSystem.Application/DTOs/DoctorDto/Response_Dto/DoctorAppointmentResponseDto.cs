using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto
{
    public class DoctorAppointmentResponseDto
    {
        public Guid AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; } = null!;
        public string? AppointmentEndTime { get; set; }
        public string AppointmentStatus { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public int DurationMinutes { get; set; }

        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid? HospitalId { get; set; }
        
        // Additional info for display
        public string? DoctorName { get; set; }
        public string? PatientName { get; set; }
        public string? HospitalName { get; set; }
    }
}
