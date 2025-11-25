using HospitalManagementSystem.Domain.Models.DoctorModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.IRepository
{
    public interface IDoctorScheduleRepository
    {
        Task<IEnumerable<DoctorSchedule>> GetAllAsync();
        Task<DoctorSchedule?> GetByIdAsync(Guid id);
        Task<DoctorSchedule> CreateAsync(DoctorSchedule doctorSchedule);
        Task<bool> DeleteAsync(Guid id);

    }
}
