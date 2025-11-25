using HospitalManagementSystem.Domain.Models.DoctorModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.IRepository
{
    public interface IDoctorLeaveRepository
    {
        Task<IEnumerable<DoctorLeave>> GetAllAsync();
        Task<DoctorLeave?> GetByIdAsync(Guid id);
        Task<DoctorLeave> CreateAsync(DoctorLeave doctorLeave);
        Task<DoctorLeave?> UpdateAsync(DoctorLeave doctorLeave);
        Task<bool> DeleteAsync(Guid id);

    }
}
