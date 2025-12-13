using HospitalManagementSystem.Application.DTOs;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices
{
    public interface IAdminDashboardService
    {
        Task<AdminOverviewDto> GetOverviewAsync(Guid? hospitalId = null);
    }
}
