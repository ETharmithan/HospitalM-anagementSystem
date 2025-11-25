using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto
{
    public class DepartmentResponseDto
    {
        public Guid DepartmentId { get; set; }
        public required string Name { get; set; }

    }
}
