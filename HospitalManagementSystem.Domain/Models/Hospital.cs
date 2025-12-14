using System.ComponentModel.DataAnnotations;
using HospitalManagementSystem.Domain.Models.Doctors;

namespace HospitalManagementSystem.Domain.Models
{
    public class Hospital
    {
        [Key]
        public Guid HospitalId { get; set; }
        
        public required string Name { get; set; }
        
        public required string Address { get; set; }
        
        public required string City { get; set; }
        
        public required string State { get; set; }
        
        public required string Country { get; set; }
        
        public required string PostalCode { get; set; }
        
        public required string PhoneNumber { get; set; }
        
        public required string Email { get; set; }
        
        public string? Website { get; set; }
        
        public string? Description { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Department> Departments { get; set; } = new List<Department>();
        public ICollection<HospitalAdmin> HospitalAdmins { get; set; } = new List<HospitalAdmin>(); 
    }
}
