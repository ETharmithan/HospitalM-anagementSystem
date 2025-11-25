using HospitalManagementSystem.Application.DTOs.Doctor.Request_Dto;
using HospitalManagementSystem.Application.DTOs.Doctor.Response_Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices.Doctor_IServices
{
    internal interface IDoctorLeaveService
    {
        Task<IEnumerable<DoctorLeaveResponseDto>> GetAllAsync();
        Task<DoctorLeaveResponseDto?> GetByIdAsync(Guid id);
        Task<DoctorLeaveResponseDto> CreateAsync(DoctorLeaveRequestDto doctorLeaveRequestDto);
        Task<DoctorLeaveResponseDto?> UpdateAsync(Guid id, DoctorLeaveRequestDto doctorLeaveRequestDto);
        Task<bool> DeleteAsync(Guid id);

    }
}
