using HospitalManagementSystem.Domain.Models.Doctor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices
{
    public interface IDoctorRepository : IGenericRepository<Doctors>
    {
        Task<Doctors> GetByNameAsync(string doctorName);
        Task<IEnumerable<Doctors>> GetAllDoctorsAsync();
    }
}
