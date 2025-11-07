using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.DTOs
{
    public class PatientUserDto
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public string? ImageUrl { get; set; }
        public required string Token { get; set; }
    }
}
