using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Domain.Models
{
    public class HospitalAdmin
    {
        [Key]
        public Guid HospitalAdminId { get; set; }
        
        public Guid HospitalId { get; set; }
        public Hospital Hospital { get; set; } = null!;
        
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        public string? ProfileImage { get; set; } // Profile image URL or path
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
