using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.Models.Patient
{
    public class Patient_Medical_Related_Info
    {
        [Key]
        [ForeignKey(nameof(Patient))]
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        public string BloodType { get; set; } = string.Empty;
        public string Allergies { get; set; } = string.Empty;
        public string ChronicConditions { get; set; } = string.Empty;

    }
}
