using HospitalManagementSystem.Domain.Models.Doctors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.IRepository
{
    public interface IDoctorAppointmentRepository
    {
        Task<IEnumerable<DoctorAppointment>> GetAllAsync();
        Task<DoctorAppointment?> GetByIdAsync(Guid id);
        Task<IEnumerable<DoctorAppointment>> GetByDoctorIdAsync(Guid doctorId);
        Task<DoctorAppointment> CreateAsync(DoctorAppointment doctorAppointment);
        Task<DoctorAppointment> UpdateAsync(DoctorAppointment doctorAppointment);
        Task<bool> DeleteAsync(Guid id);

    }
}
