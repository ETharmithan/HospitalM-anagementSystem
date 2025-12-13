using System;

namespace HospitalManagementSystem.Application.DTOs.EPrescriptionDto.Response_Dto
{
    public class EPrescriptionResponseDto
    {
        public Guid EPrescriptionId { get; set; }
        public string Diagnosis { get; set; } = null!;
        public string Prescription { get; set; } = null!;
        public string Notes { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
    }
}
