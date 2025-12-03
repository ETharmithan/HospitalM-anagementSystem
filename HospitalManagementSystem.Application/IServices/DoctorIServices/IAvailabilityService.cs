using HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto;
using System;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices.DoctorIServices
{
    public interface IAvailabilityService
    {
        Task<AvailabilityResponseDto> GetAvailabilityAsync(Guid doctorId, DateTime date);
        Task<AvailableDatesResponseDto> GetAvailableDatesAsync(Guid doctorId, DateTime startDate, DateTime endDate);
        Task<bool> IsSlotAvailableAsync(Guid doctorId, DateTime date, string time);
    }
}

