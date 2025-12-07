using System;
using System.Collections.Generic;

namespace HospitalManagementSystem.Application.DTOs.PatientDto
{
    // Complete medical profile for doctor to view
    public class PatientMedicalProfileDto
    {
        // Basic Info
        public Guid PatientId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public DateTime DateOfBirth { get; set; }
        public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
        public string Gender { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

        // Contact Info
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        // Medical Info
        public string? BloodType { get; set; }
        public string? Allergies { get; set; }
        public string? ChronicConditions { get; set; }

        // Medical History
        public string? PastIllnesses { get; set; }
        public string? Surgeries { get; set; }
        public string? MedicalHistoryNotes { get; set; }

        // Emergency Contact
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelationship { get; set; }

        // Past Prescriptions
        public List<PrescriptionSummaryDto> Prescriptions { get; set; } = new();

        // Past Appointments
        public List<AppointmentSummaryDto> PastAppointments { get; set; } = new();
    }

    public class PrescriptionSummaryDto
    {
        public Guid RecordId { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string Prescription { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public string? DoctorName { get; set; }
        public Guid DoctorId { get; set; }
    }

    public class AppointmentSummaryDto
    {
        public Guid AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? DoctorName { get; set; }
        public string? HospitalName { get; set; }
    }
}
