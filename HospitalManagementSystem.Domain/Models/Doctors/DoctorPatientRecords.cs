using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.Models.Doctors
{
    public class DoctorPatientRecords
    {
        [Key]
        public Guid TreatmentId { get; set; }
        public required string Diagnosis { get; set; }
        public required string Prescription { get; set; }
        public required string Notes { get; set; }
        public required DateTime VisitDate { get; set; }



        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }

        public Doctor Doctor { get; set; } = null!;
        public Patient.Patient Patient { get; set; } = null!;

    }
}
