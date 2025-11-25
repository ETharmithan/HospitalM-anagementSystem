using HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto;
using HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices.DoctorIServices
{
    public interface IDoctorScheduleService
    {
        Task<IEnumerable<DoctorScheduleResponseDto>> GetAllAsync();
        Task<DoctorScheduleResponseDto?> GetByIdAsync(Guid id);
        Task<DoctorScheduleResponseDto> CreateAsync(DoctorScheduleRequestDto doctorScheduleRequestDto);
        Task<bool> DeleteAsync(Guid id);

    }
}
