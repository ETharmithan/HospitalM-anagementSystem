using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Models.Patient
{
    public class Patient
    {
        [Key]
        public Guid PatientId { get; set; }

        [ForeignKey(nameof(UserLogin))]
        public Guid UserId { get; set; }
        public User UserLogin { get; set; } = null!;

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }



        public Patient_Contact_Information ContactInfo { get; set; }
        public Patient_Identification_Details IdentificationDetails { get; set; }
        public Patient_Medical_History MedicalHistory { get; set; }
        public Patient_Medical_Related_Info MedicalRelatedInfo { get; set; }
        public Patient_Emergency_Contact EmergencyContact { get; set; }
        public Patient_Login_Info? LoginInfo { get; set; } = null!;
    }
}