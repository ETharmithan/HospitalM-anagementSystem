using HospitalManagementSystem.Domain.Models.Doctor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.IRepository
{
    public interface IDoctorRepository
    {
        Task<IEnumerable<Doctor>> GetAllAsync();
        Task<Doctor?> GetByIdAsync(Guid id);
        Task<Doctor> CreateAsync(Doctor doctor);
        Task<Doctor?> UpdateAsync(Doctor doctor);
        Task<bool> DeleteAsync(Guid id);

    }
}
