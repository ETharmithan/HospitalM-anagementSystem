using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.Models
{
    public class Doctors
    {
        public Guid Id { get; set; }
        public string DoctorName { get; set; } = string.Empty;
    }
}
