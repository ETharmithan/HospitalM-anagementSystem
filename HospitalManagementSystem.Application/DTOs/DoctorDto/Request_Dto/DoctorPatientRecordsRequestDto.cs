using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto
{
    public class DoctorPatientRecordsRequestDto
    {
        [Required(ErrorMessage = "Diagnosis is required")]
        public string Diagnosis { get; set; } = null!;

        [Required(ErrorMessage = "Prescription is required")]
        public string Prescription { get; set; } = null!;

        [MaxLength(2000)]
        public string Notes { get; set; } = null!;

        [Required(ErrorMessage = "Visit date is required")]
        [DataType(DataType.Date)]
        public DateTime VisitDate { get; set; }


        //ForeignKey
        [Required(ErrorMessage = "DoctorId is required")]
        public Guid DoctorId { get; set; }

        [Required(ErrorMessage = "PatientId is required")]
        public Guid PatientId { get; set; }

    }
}
