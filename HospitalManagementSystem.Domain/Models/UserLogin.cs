using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.Models
{
    public class UserLogin
    {
        [Key]   
        public Guid UserId { get; set; }
        public required string Email { get; set; }
        public string? ImageUrl { get; set; }
        public required string Username { get; set; }
        public required byte[] PasswordHash { get; set; }
        public required byte[] PasswordSalt { get; set; }
        public required string Role { get; set; }

        // public Patient? Patient { get; set; } //navigation property
    }
}
