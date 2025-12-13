using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Domain.Models.Doctors
{
    public class EPrescription
    {
        [Key]
        public Guid EPrescriptionId { get; set; }

        public required string Diagnosis { get; set; }
        public required string Prescription { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
    }
}
