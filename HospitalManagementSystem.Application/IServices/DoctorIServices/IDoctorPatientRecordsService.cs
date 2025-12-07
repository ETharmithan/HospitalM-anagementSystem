using HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto;
using HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices.DoctorIServices
{
    public interface IDoctorPatientRecordsService
    {
        Task<IEnumerable<DoctorPatientRecordsResponseDto>> GetAllAsync();
        Task<DoctorPatientRecordsResponseDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<DoctorPatientRecordsResponseDto>> GetByPatientIdAsync(Guid patientId);
        Task<IEnumerable<DoctorPatientRecordsResponseDto>> GetByDoctorIdAsync(Guid doctorId);
        Task<DoctorPatientRecordsResponseDto> CreateAsync(DoctorPatientRecordsRequestDto doctorPatientRecordsRequestDto);
        Task<DoctorPatientRecordsResponseDto?> UpdateAsync(Guid id, DoctorPatientRecordsRequestDto doctorPatientRecordsRequestDto);
        Task<bool> DeleteAsync(Guid id);

    }
}
