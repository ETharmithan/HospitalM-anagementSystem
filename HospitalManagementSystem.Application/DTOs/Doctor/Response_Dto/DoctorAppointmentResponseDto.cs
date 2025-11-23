using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.DTOs.Doctor.Response_Dto
{
    public class DoctorAppointmentResponseDto
    {
        public Guid AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; } = null!;
        public string AppointmentStatus { get; set; } = null!;
        public DateTime CreatedDate { get; set; }


        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }

    }
}
