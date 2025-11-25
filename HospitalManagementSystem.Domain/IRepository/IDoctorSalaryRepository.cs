using HospitalManagementSystem.Domain.Models.Doctors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.IRepository
{
    public interface IDoctorSalaryRepository
    {
        Task<IEnumerable<DoctorSalary>> GetAllAsync();
        Task<DoctorSalary?> GetByIdAsync(Guid id);
        Task<DoctorSalary> CreateAsync(DoctorSalary doctorSalary);
        Task<bool> DeleteAsync(Guid id);

    }
}
