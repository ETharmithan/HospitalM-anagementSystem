using HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto;
using HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices.DoctorIServices
{
    public interface IDoctorLeaveService
    {
        Task<IEnumerable<DoctorLeaveResponseDto>> GetAllAsync();
        Task<DoctorLeaveResponseDto?> GetByIdAsync(Guid id);
        Task<DoctorLeaveResponseDto> CreateAsync(DoctorLeaveRequestDto doctorLeaveRequestDto);
        Task<DoctorLeaveResponseDto?> UpdateAsync(Guid id, DoctorLeaveRequestDto doctorLeaveRequestDto);
        Task<bool> DeleteAsync(Guid id);

    }
}
