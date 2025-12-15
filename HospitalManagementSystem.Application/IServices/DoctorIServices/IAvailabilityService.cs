using HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto;
using System;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices.DoctorIServices
{
    public interface IAvailabilityService
    {
        Task<AvailabilityResponseDto> GetAvailabilityAsync(Guid doctorId, DateTime date, Guid? hospitalId = null);
        Task<AvailableDatesResponseDto> GetAvailableDatesAsync(Guid doctorId, DateTime startDate, DateTime endDate, Guid? hospitalId = null);
        Task<bool> IsSlotAvailableAsync(Guid doctorId, DateTime date, string time, Guid? hospitalId = null);
    }
}

