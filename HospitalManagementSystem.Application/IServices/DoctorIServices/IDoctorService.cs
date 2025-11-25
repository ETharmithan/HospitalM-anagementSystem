using HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto;
using HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices.DoctorIServices
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorResponseDto>> GetAllAsync();
        Task<DoctorResponseDto?> GetByIdAsync(Guid id);
        Task<DoctorResponseDto> CreateAsync(DoctorRequestDto doctorRequestDto);
        Task<DoctorResponseDto?> UpdateAsync(Guid id, DoctorRequestDto doctorRequestDto);
        Task<bool> DeleteAsync(Guid id);

    }
}
