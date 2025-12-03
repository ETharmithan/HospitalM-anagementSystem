using HospitalManagementSystem.Domain.Models.Doctors;
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
        Task<IEnumerable<DoctorSchedule>> GetByDoctorIdAsync(Guid doctorId);
        Task<DoctorSchedule> CreateAsync(DoctorSchedule doctorSchedule);
        Task<bool> DeleteAsync(Guid id);

    }
}
