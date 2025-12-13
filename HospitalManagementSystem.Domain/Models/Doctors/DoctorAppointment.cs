using HospitalManagementSystem.Domain.Models.Patient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.Models.Doctors
{
    public class DoctorAppointment
    {
        [Key]
        public required Guid AppointmentId { get; set; }
        public required DateTime AppointmentDate { get; set; }
        public required string AppointmentTime { get; set; }
        public required string AppointmentStatus { get; set; }
        public required DateTime CreatedDate { get; set; }
        
        // Duration in minutes for overlap checking
        public int DurationMinutes { get; set; } = 30;
        
        // End time calculated from AppointmentTime + DurationMinutes
        public string? AppointmentEndTime { get; set; }

        public required Guid PatientId { get; set; }
        public required Guid DoctorId { get; set; }
        
        // Hospital where the appointment takes place
        public Guid? HospitalId { get; set; }

        // Cancellation workflow fields
        public bool CancellationRequested { get; set; } = false;
        public DateTime? CancellationRequestedAt { get; set; }
        public string? CancellationReason { get; set; }
        public bool CancellationApproved { get; set; } = false;
        public DateTime? CancellationApprovedAt { get; set; }
        public Guid? CancellationApprovedBy { get; set; }
        public string? CancellationApprovalNote { get; set; }

        // Navigation properties
        public Doctor Doctor { get; set; } = null!;
        public Patient.Patient? Patient { get; set; }
        public Hospital? Hospital { get; set; }
    }
}
