using HospitalManagementSystem.Application.DTOs.Doctor.Request_Dto;
using HospitalManagementSystem.Application.DTOs.Doctor.Response_Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices.Doctor_IServices
{
    internal interface IDoctorService
    {
        Task<IEnumerable<DoctorResponseDto>> GetAllAsync();
        Task<DoctorResponseDto?> GetByIdAsync(Guid id);
        Task<DoctorResponseDto> CreateAsync(DoctorRequestDto doctorRequestDto);
        Task<DoctorResponseDto?> UpdateAsync(Guid id, DoctorRequestDto doctorRequestDto);
        Task<bool> DeleteAsync(Guid id);

    }
}
