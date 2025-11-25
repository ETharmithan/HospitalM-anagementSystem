using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.Models.DoctorModel
{
    public class DoctorAppointment
    {
        [Key]
        public required Guid AppointmentId { get; set; }
        public required DateTime AppointmentDate { get; set; }
        public required string AppointmentTime { get; set; }
        public required string AppointmentStatus { get; set; }
        public required DateTime CreatedDate { get; set; }


        public required Guid PatientId { get; set; }
        public required Guid DoctorId { get; set; }

        public Doctor Doctor { get; set; } = null!;

    }
}
