using HospitalManagementSystem.Domain.Models;
using HospitalManagementSystem.Domain.Models.Doctors;

namespace HospitalManagementSystem.Domain.IRepository
{
    public interface IHospitalRepository
    {
        Task<List<Hospital>> GetAllHospitalsAsync();
        Task<Hospital?> GetHospitalByIdAsync(Guid hospitalId);
        Task<Hospital> CreateHospitalAsync(Hospital hospital);
        Task<Hospital?> UpdateHospitalAsync(Guid hospitalId, Hospital hospital);
        Task<bool> DeleteHospitalAsync(Guid hospitalId);
        Task<List<Department>> GetHospitalDepartmentsAsync(Guid hospitalId);
        Task<Hospital> GetByIdAsync(Guid id);
        
        // Hospital Admin Management
        Task<bool> AssignHospitalAdminAsync(Guid hospitalId, Guid userId);
        Task<bool> RemoveHospitalAdminAsync(Guid hospitalId, Guid userId);
        Task<bool> IsUserHospitalAdminAsync(Guid hospitalId, Guid userId);
    }
}
