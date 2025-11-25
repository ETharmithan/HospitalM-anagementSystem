using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.DTOs
{
    public class User_Dto
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string DisplayName { get; set; }
        public string? ImageUrl { get; set; }
        public required string Token { get; set; }
        public required string Role { get; set; }
    }
}
