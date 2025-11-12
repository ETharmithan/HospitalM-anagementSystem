using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.Models.Patient
{
    public class Patient_Identification_Details
    {
        [Key]
        [ForeignKey(nameof(Patient))]
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        public string NIC { get; set; } = null!;
        public string PassportNumber { get; set; } = string.Empty;
        public string DriversLicenseNumber { get; set; } = string.Empty;
    }
}
