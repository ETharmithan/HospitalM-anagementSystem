using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto
{
    public class DoctorResponseDto
    {
        public Guid DoctorId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Qualification { get; set; } = null!;
        public string LicenseNumber { get; set; } = null!;
        public string Status { get; set; } = null!;
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }

    }
}
