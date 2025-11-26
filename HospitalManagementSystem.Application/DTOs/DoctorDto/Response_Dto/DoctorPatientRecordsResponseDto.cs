using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto
{
    public class DoctorPatientRecordsResponseDto
    {
        public Guid RecordId { get; set; }
        public string Diagnosis { get; set; } = null!;
        public string Prescription { get; set; } = null!;
        public string Notes { get; set; } = null!;
        public DateTime VisitDate { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }

    }
}
