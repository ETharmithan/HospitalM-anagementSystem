using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto
{
    public class DoctorSalaryResponseDto
    {
        public Guid SalaryId { get; set; }
        public decimal MonthlySalary { get; set; }
        public DateTime PaymentDate { get; set; }
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;

    }
}
