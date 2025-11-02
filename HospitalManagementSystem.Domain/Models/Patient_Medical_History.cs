using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.Models
{
    public class Patient_Medical_History
    {
        [Key]
        [ForeignKey(nameof(Patient))]
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        public string PastIllnesses { get; set; } = string.Empty;
        public string Surgeries { get; set; } = string.Empty;
        public string? MedicalHistoryNotes { get; set; } = string.Empty;
    }
}
