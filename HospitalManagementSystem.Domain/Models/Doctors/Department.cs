using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.Models.Doctors
{
    public class Department
    {
        [Key]
        public Guid DepartmentId { get; set; }
        public required string Name { get; set; }
        
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid HospitalId { get; set; }
        public Hospital Hospital { get; set; } = null!;

        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
