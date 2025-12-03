using HospitalManagementSystem.Domain.Models.Doctors;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.IRepository
{
    public interface IDoctorAvailabilityRepository
    {
        Task<IEnumerable<DoctorAvailability>> GetAllAsync();
        Task<DoctorAvailability?> GetByIdAsync(Guid id);
        Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAsync(Guid doctorId);
        Task<DoctorAvailability?> GetByDoctorIdAndDateAsync(Guid doctorId, DateTime date);
        Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAndDateRangeAsync(Guid doctorId, DateTime startDate, DateTime endDate);
        Task<DoctorAvailability> CreateAsync(DoctorAvailability doctorAvailability);
        Task<DoctorAvailability> UpdateAsync(DoctorAvailability doctorAvailability);
        Task<bool> DeleteAsync(Guid id);
    }
}

