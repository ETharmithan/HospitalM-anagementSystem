using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.DTOs
{
    public class LoginDto
    {
        [EmailAddress]
        public required string Email { get; set; } = ""; // default value is empty string
        public required string Password { get; set; } = ""; // default value is empty string
    }
}
