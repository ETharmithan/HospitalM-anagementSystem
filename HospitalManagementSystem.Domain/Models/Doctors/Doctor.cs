using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.Models.Doctors
{
    public class Doctor
    {
        [Key]
        public Guid DoctorId { get; set; }

        public required string Name { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        [Phone]
        public required string Phone { get; set; }
        public required string Qualification { get; set; }
        public required string LicenseNumber { get; set; }
        public required string Status { get; set; }
        public string? ProfileImage { get; set; } // Profile image URL or path
        public int AppointmentDurationMinutes { get; set; } = 30; // Default 30 minutes per appointment
        public int BreakTimeMinutes { get; set; } = 0; // Break time between appointments (optional)


        public Guid? DepartmentId { get; set; }


        public Department? Department { get; set; } = null!;
        public ICollection<DoctorAppointment> DoctorAppointments { get; set; } = new List<DoctorAppointment>();
        public ICollection<DoctorLeave> DoctorLeaves { get; set; } = new List<DoctorLeave>();
        public ICollection<DoctorPatientRecords> DoctorPatientRecords { get; set; } = new List<DoctorPatientRecords>();
        public ICollection<DoctorSalary> DoctorSalaries { get; set; } = new List<DoctorSalary>();
        public ICollection<DoctorSchedule> DoctorSchedules { get; set; } = new List<DoctorSchedule>();
        public ICollection<DoctorAvailability> DoctorAvailabilities { get; set; } = new List<DoctorAvailability>();

    }
}
