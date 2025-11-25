using HospitalManagementSystem.Application.DTOs.Doctor.Request_Dto;
using HospitalManagementSystem.Application.DTOs.Doctor.Response_Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices.Doctor_IServices
{
    internal interface IDoctorPatientRecordsService
    {
        Task<IEnumerable<DoctorPatientRecordsResponseDto>> GetAllAsync();
        Task<DoctorPatientRecordsResponseDto?> GetByIdAsync(Guid id);
        Task<DoctorPatientRecordsResponseDto> CreateAsync(DoctorPatientRecordsRequestDto doctorPatientRecordsRequestDto);
        Task<DoctorPatientRecordsResponseDto?> UpdateAsync(Guid id, DoctorPatientRecordsRequestDto doctorPatientRecordsRequestDto);
        Task<bool> DeleteAsync(Guid id);

    }
}
