using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.DTOs.Doctor.Request_Dto
{
    public class DoctorSalaryRequestDto
    {
        [Required(ErrorMessage = "Monthly salary is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive number")]
        public decimal MonthlySalary { get; set; }

        [Required(ErrorMessage = "Payment date is required")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; }

        //ForeignKey
        [Required(ErrorMessage = "DoctorId is required")]
        public Guid DoctorId { get; set; }

    }
}
