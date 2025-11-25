using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Application.DTOs
{
    public class AdditionalPatientDetailsDTO
    {
        [Required]
        public Guid PatientId { get; set; }

        // Emergency Contact
        public EmergencyContactDTO EmergencyContact { get; set; } = new EmergencyContactDTO();

        // Medical History
        public MedicalHistoryDTO MedicalHistory { get; set; } = new MedicalHistoryDTO();

        // Medical Information
        public MedicalInfoDTO MedicalInfo { get; set; } = new MedicalInfoDTO();
    }

    public class EmergencyContactDTO
    {
        [Required]
        public string ContactName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string ContactEmail { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits")]
        public string ContactPhone { get; set; } = string.Empty;

        [Required]
        public string RelationshipToPatient { get; set; } = string.Empty;
    }

    public class MedicalHistoryDTO
    {
        public string PastIllnesses { get; set; } = string.Empty;
        public string Surgeries { get; set; } = string.Empty;
        public string MedicalHistoryNotes { get; set; } = string.Empty;
    }

    public class MedicalInfoDTO
    {
        [Required]
        public string BloodType { get; set; } = string.Empty;
        public string Allergies { get; set; } = string.Empty;
        public string ChronicConditions { get; set; } = string.Empty;
    }
}
