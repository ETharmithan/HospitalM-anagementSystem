using HospitalManagementSystem.Domain.Models.Patient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices
{
    public interface IPatientRepository : IGenericRepository<Patient>
    {
        Task<Patient> GetByUserIdAsync(Guid userId);
        Task<Patient> GetPatientWithDetailsAsync(Guid patientId);
        Task<IEnumerable<Patient>> GetAllPatientsAsync();
        Task<IEnumerable<Patient>> GetPatientsByGenderAsync(string gender);
    }
}
