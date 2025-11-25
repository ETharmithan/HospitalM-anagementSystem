using HospitalManagementSystem.Application.DTOs.Doctor.Request_Dto;
using HospitalManagementSystem.Application.DTOs.Doctor.Response_Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices.Doctor
{
    internal interface IDoctorScheduleService
    {
        Task<IEnumerable<DoctorScheduleResponseDto>> GetAllAsync();
        Task<DoctorScheduleResponseDto?> GetByIdAsync(Guid id);
        Task<DoctorScheduleResponseDto> CreateAsync(DoctoeScheduleRequestDto doctorScheduleRequestDto);
        Task<bool> DeleteAsync(Guid id);

    }
}
