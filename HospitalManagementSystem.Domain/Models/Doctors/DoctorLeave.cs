using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.Models.Doctors
{
    public class DoctorLeave
    {
        [Key]
        public Guid LeaveId { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required string Reason { get; set; }
        public required string Status { get; set; }



        public Guid DoctorId { get; set; }

        public Doctor Doctor { get; set; } = null!;

    }
}
