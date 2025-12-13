using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Application.DTOs.EPrescriptionDto.Request_Dto
{
    public class UpdateEPrescriptionRequestDto
    {
        [Required(ErrorMessage = "Diagnosis is required")]
        public string Diagnosis { get; set; } = null!;

        [Required(ErrorMessage = "Prescription is required")]
        public string Prescription { get; set; } = null!;

        [MaxLength(2000)]
        public string Notes { get; set; } = string.Empty;

        [Required(ErrorMessage = "Visit date is required")]
        [DataType(DataType.Date)]
        public DateTime VisitDate { get; set; }
    }
}
