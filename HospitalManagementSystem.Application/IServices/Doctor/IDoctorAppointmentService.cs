using HospitalManagementSystem.Application.DTOs.Doctor.Request_Dto;
using HospitalManagementSystem.Application.DTOs.Doctor.Response_Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices.Doctor
{
    internal interface IDoctorAppointmentService
    {
        Task<IEnumerable<DoctorAppointmentResponseDto>> GetAllAsync();
        Task<DoctorAppointmentResponseDto?> GetByIdAsync(Guid id);
        Task<DoctorAppointmentResponseDto> CreateAsync(DoctorAppointmentRequestDto doctorAppointmentRequestDto);
        Task<DoctorAppointmentResponseDto?> UpdateAsync(Guid id, DoctorAppointmentRequestDto doctorAppointmentRequestDto);
        Task<bool> DeleteAsync(Guid id);

    }
}
