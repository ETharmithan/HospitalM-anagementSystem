using HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto;
using HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices.DoctorIServices
{
    public interface IDoctorAppointmentService
    {
        Task<IEnumerable<DoctorAppointmentResponseDto>> GetAllAsync();
        Task<DoctorAppointmentResponseDto?> GetByIdAsync(Guid id);
        Task<DoctorAppointmentResponseDto> CreateAsync(DoctorAppointmentRequestDto doctorAppointmentRequestDto);
        Task<DoctorAppointmentResponseDto?> UpdateAsync(Guid id, DoctorAppointmentRequestDto doctorAppointmentRequestDto);
        Task<bool> DeleteAsync(Guid id);
        
        // Get appointments by doctor
        Task<IEnumerable<DoctorAppointmentResponseDto>> GetByDoctorIdAsync(Guid doctorId);
        
        // Get appointments by patient
        Task<IEnumerable<DoctorAppointmentResponseDto>> GetByPatientIdAsync(Guid patientId);
        
        // Check for booking overlap - returns true if slot is available
        Task<bool> IsSlotAvailableAsync(Guid doctorId, DateTime date, string time, Guid? excludeAppointmentId = null);
        
        // Get available time slots for a doctor on a specific date
        Task<IEnumerable<string>> GetAvailableSlotsAsync(Guid doctorId, DateTime date, Guid? hospitalId = null);
        
        // Get fully booked dates for a doctor (for calendar disabling)
        Task<IEnumerable<DateTime>> GetFullyBookedDatesAsync(Guid doctorId, DateTime startDate, DateTime endDate, Guid? hospitalId = null);
        
        // Cancel appointment
        Task<bool> CancelAppointmentAsync(Guid appointmentId, string? cancellationReason = null);
        
        // Cancellation workflow methods
        Task<bool> RequestCancellationAsync(Guid appointmentId, string? cancellationReason = null);
        Task<bool> ApproveCancellationAsync(Guid appointmentId, Guid approvedBy, string? approvalNote = null);
        Task<bool> RejectCancellationAsync(Guid appointmentId, Guid rejectedBy, string? rejectionReason = null);
    }
}
