using HospitalManagementSystem.Application.DTOs.Doctor.Request_Dto;
using HospitalManagementSystem.Application.DTOs.Doctor.Response_Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices.Doctor_IServices
{
    internal interface IDoctorSalaryService
    {
        Task<IEnumerable<DoctorSalaryResponseDto>> GetAllAsync();
        Task<DoctorSalaryResponseDto?> GetByIdAsync(Guid id);
        Task<DoctorSalaryResponseDto> CreateAsync(DoctorSalaryRequestDto doctorSalaryRequestDto);
        Task<bool> DeleteAsync(Guid id);

    }
}
