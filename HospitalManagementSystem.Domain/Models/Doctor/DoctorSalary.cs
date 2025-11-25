using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.Models.Doctor
{
    public class DoctorSalary
    {
        [Key]
        public Guid SalaryId { get; set; }
        public required decimal MonthlySalary { get; set; }
        public required DateTime PaymentDate { get; set; }



        public Guid DoctorId { get; set; }

        public Doctor Doctor { get; set; } = null!;

    }
}
