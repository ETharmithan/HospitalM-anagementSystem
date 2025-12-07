using HospitalManagementSystem.Application.DTOs.HospitalDto.Request_Dto;
using HospitalManagementSystem.Application.DTOs.HospitalDto.Response_Dto;
using DepartmentResponseDto = HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto.DepartmentResponseDto;

namespace HospitalManagementSystem.Application.IServices
{
    public interface IHospitalService
    {
        Task<List<HospitalResponseDto>> GetAllHospitalsAsync();
        Task<HospitalResponseDto?> GetHospitalByIdAsync(Guid hospitalId);
        Task<HospitalResponseDto> CreateHospitalAsync(HospitalRequestDto hospitalDto);
        Task<HospitalResponseDto?> UpdateHospitalAsync(Guid hospitalId, HospitalRequestDto hospitalDto);
        Task<bool> DeleteHospitalAsync(Guid hospitalId);
        Task<List<DepartmentResponseDto>> GetHospitalDepartmentsAsync(Guid hospitalId);
        
        // Hospital Admin Management
        Task<bool> AssignHospitalAdminAsync(Guid hospitalId, Guid userId);
        Task<bool> RemoveHospitalAdminAsync(Guid hospitalId, Guid userId);
    }
}
