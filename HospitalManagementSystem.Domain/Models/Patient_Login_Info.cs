using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.Models
{
    public class Patient_Login_Info
    {
        [Key]
        [ForeignKey(nameof(Patient))]
        public Guid PatientId { get; set; } 

        public required string Email { get; set; }
        public string? ImageUrl { get; set; }
        public required string Username { get; set; }
        public required byte[] PasswordHash { get; set; }
        public required byte[] PasswordSalt { get; set; }


        public Patient Patient { get; set; } = null!;

    }
}
