using HospitalManagementSystem.Domain.Models.Doctors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.IRepository
{
    public interface IDoctorPatientRecordsRepository
    {
        Task<IEnumerable<DoctorPatientRecords>> GetAllAsync();
        Task<DoctorPatientRecords?> GetByIdAsync(Guid id);
        Task<IEnumerable<DoctorPatientRecords>> GetByPatientIdAsync(Guid patientId);
        Task<IEnumerable<DoctorPatientRecords>> GetByDoctorIdAsync(Guid doctorId);
        Task<DoctorPatientRecords> CreateAsync(DoctorPatientRecords doctorPatientRecords);
        Task<DoctorPatientRecords?> UpdateAsync(DoctorPatientRecords doctorPatientRecords);
        Task<bool> DeleteAsync(Guid id);

    }
}
