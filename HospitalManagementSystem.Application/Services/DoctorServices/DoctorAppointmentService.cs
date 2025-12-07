using HospitalManagementSystem.Application.DTOs.DoctorDto.Request_Dto;
using HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto;
using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Application.IServices.DoctorIServices;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models.Doctors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Services.DoctorServices
{
    public class DoctorAppointmentService : IDoctorAppointmentService
    {
        private readonly IDoctorAppointmentRepository _doctorAppointmentRepository;
        private readonly IDoctorAvailabilityRepository _availabilityRepository;
        private readonly IEmailService _emailService;
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IHospitalRepository _hospitalRepository;
        private readonly INotificationService _notificationService;

        public DoctorAppointmentService(
            IDoctorAppointmentRepository doctorAppointmentRepository,
            IDoctorAvailabilityRepository availabilityRepository,
            IEmailService emailService,
            IPatientRepository patientRepository,
            IDoctorRepository doctorRepository,
            IHospitalRepository hospitalRepository,
            INotificationService notificationService)
        {
            _doctorAppointmentRepository = doctorAppointmentRepository;
            _availabilityRepository = availabilityRepository;
            _emailService = emailService;
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
            _hospitalRepository = hospitalRepository;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<DoctorAppointmentResponseDto>> GetAllAsync()
        {
            var data = await _doctorAppointmentRepository.GetAllAsync();
            return data.Select(MapToResponseDto);
        }

        public async Task<DoctorAppointmentResponseDto?> GetByIdAsync(Guid id)
        {
            var a = await _doctorAppointmentRepository.GetByIdAsync(id);
            if (a == null) return null;
            return MapToResponseDto(a);
        }

        public async Task<IEnumerable<DoctorAppointmentResponseDto>> GetByDoctorIdAsync(Guid doctorId)
        {
            var data = await _doctorAppointmentRepository.GetByDoctorIdAsync(doctorId);
            return data.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<DoctorAppointmentResponseDto>> GetByPatientIdAsync(Guid patientId)
        {
            var data = await _doctorAppointmentRepository.GetByPatientIdAsync(patientId);
            return data.Select(MapToResponseDto);
        }

        public async Task<DoctorAppointmentResponseDto> CreateAsync(DoctorAppointmentRequestDto dto)
        {
            // Check for booking overlap
            var isAvailable = await IsSlotAvailableAsync(dto.DoctorId, dto.AppointmentDate, dto.AppointmentTime);
            if (!isAvailable)
            {
                throw new InvalidOperationException("This time slot is already booked. Please select another time.");
            }

            // Calculate end time
            var endTime = CalculateEndTime(dto.AppointmentTime, dto.DurationMinutes);

            var entity = new DoctorAppointment
            {
                AppointmentId = Guid.NewGuid(),
                AppointmentDate = dto.AppointmentDate,
                AppointmentTime = dto.AppointmentTime,
                AppointmentEndTime = endTime,
                AppointmentStatus = dto.AppointmentStatus,
                CreatedDate = DateTime.UtcNow,
                DurationMinutes = dto.DurationMinutes,
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                HospitalId = dto.HospitalId
            };

            await _doctorAppointmentRepository.CreateAsync(entity);

            // Send booking confirmation email (fire and forget)
            _ = SendBookingConfirmationEmailAsync(entity);

            // Create in-app notification (fire and forget)
            _ = CreateBookingNotificationAsync(entity);

            return MapToResponseDto(entity);
        }

        public async Task<DoctorAppointmentResponseDto?> UpdateAsync(Guid id, DoctorAppointmentRequestDto dto)
        {
            var entity = await _doctorAppointmentRepository.GetByIdAsync(id);
            if (entity == null) return null;

            // Check for overlap only if date/time changed
            if (entity.AppointmentDate != dto.AppointmentDate || entity.AppointmentTime != dto.AppointmentTime)
            {
                var isAvailable = await IsSlotAvailableAsync(dto.DoctorId, dto.AppointmentDate, dto.AppointmentTime, id);
                if (!isAvailable)
                {
                    throw new InvalidOperationException("This time slot is already booked. Please select another time.");
                }
            }

            entity.AppointmentDate = dto.AppointmentDate;
            entity.AppointmentTime = dto.AppointmentTime;
            entity.AppointmentEndTime = CalculateEndTime(dto.AppointmentTime, dto.DurationMinutes);
            entity.AppointmentStatus = dto.AppointmentStatus;
            entity.DurationMinutes = dto.DurationMinutes;
            entity.PatientId = dto.PatientId;
            entity.DoctorId = dto.DoctorId;
            entity.HospitalId = dto.HospitalId;

            await _doctorAppointmentRepository.UpdateAsync(entity);
            return MapToResponseDto(entity);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _doctorAppointmentRepository.DeleteAsync(id);
        }

        public async Task<bool> CancelAppointmentAsync(Guid appointmentId, string? cancellationReason = null)
        {
            var entity = await _doctorAppointmentRepository.GetByIdAsync(appointmentId);
            if (entity == null) return false;

            entity.AppointmentStatus = "Cancelled";
            await _doctorAppointmentRepository.UpdateAsync(entity);

            // Send cancellation email (fire and forget)
            _ = SendBookingCancellationEmailAsync(entity, cancellationReason);

            // Create cancellation notification (fire and forget)
            _ = CreateCancellationNotificationAsync(entity);

            return true;
        }

        public async Task<bool> IsSlotAvailableAsync(Guid doctorId, DateTime date, string time, Guid? excludeAppointmentId = null)
        {
            var appointments = await _doctorAppointmentRepository.GetByDoctorIdAsync(doctorId);
            
            // Filter to same date and active appointments
            var sameDateAppointments = appointments.Where(a => 
                a.AppointmentDate.Date == date.Date && 
                a.AppointmentStatus != "Cancelled" &&
                (excludeAppointmentId == null || a.AppointmentId != excludeAppointmentId));

            // Check for exact time match or overlap
            foreach (var apt in sameDateAppointments)
            {
                if (TimesOverlap(apt.AppointmentTime, apt.DurationMinutes, time, 30))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<IEnumerable<string>> GetAvailableSlotsAsync(Guid doctorId, DateTime date, Guid? hospitalId = null)
        {
            // Get doctor's availability for this date
            var availability = await _availabilityRepository.GetByDoctorAndDateAsync(doctorId, date, hospitalId);
            
            // Default hours if no specific availability set
            string startTime = "09:00";
            string endTime = "17:00";
            int slotDuration = 30;

            if (availability != null && availability.IsAvailable)
            {
                startTime = availability.StartTime;
                endTime = availability.EndTime;
                slotDuration = availability.SlotDurationMinutes;
            }
            else if (availability != null && !availability.IsAvailable)
            {
                // Doctor not available on this date
                return Enumerable.Empty<string>();
            }

            // Generate all possible slots
            var allSlots = GenerateTimeSlots(startTime, endTime, slotDuration);

            // Get booked appointments for this date
            var appointments = await _doctorAppointmentRepository.GetByDoctorIdAsync(doctorId);
            var bookedSlots = appointments
                .Where(a => a.AppointmentDate.Date == date.Date && 
                           a.AppointmentStatus != "Cancelled" &&
                           (hospitalId == null || a.HospitalId == hospitalId))
                .Select(a => a.AppointmentTime)
                .ToHashSet();

            // Return only available slots
            return allSlots.Where(slot => !bookedSlots.Contains(slot));
        }

        public async Task<IEnumerable<DateTime>> GetFullyBookedDatesAsync(Guid doctorId, DateTime startDate, DateTime endDate, Guid? hospitalId = null)
        {
            var fullyBookedDates = new List<DateTime>();
            
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                // Skip past dates
                if (date < DateTime.Today) continue;

                var availableSlots = await GetAvailableSlotsAsync(doctorId, date, hospitalId);
                if (!availableSlots.Any())
                {
                    fullyBookedDates.Add(date);
                }
            }

            return fullyBookedDates;
        }

        // Helper methods
        private static DoctorAppointmentResponseDto MapToResponseDto(DoctorAppointment a)
        {
            return new DoctorAppointmentResponseDto
            {
                AppointmentId = a.AppointmentId,
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                AppointmentEndTime = a.AppointmentEndTime,
                AppointmentStatus = a.AppointmentStatus,
                CreatedDate = a.CreatedDate,
                DurationMinutes = a.DurationMinutes,
                PatientId = a.PatientId,
                DoctorId = a.DoctorId,
                HospitalId = a.HospitalId,
                DoctorName = a.Doctor?.Name,
                PatientName = a.Patient != null ? $"{a.Patient.FirstName} {a.Patient.LastName}" : null,
                HospitalName = a.Hospital?.Name
            };
        }

        private static string CalculateEndTime(string startTime, int durationMinutes)
        {
            if (TimeSpan.TryParse(startTime, out var start))
            {
                var end = start.Add(TimeSpan.FromMinutes(durationMinutes));
                return end.ToString(@"hh\:mm");
            }
            return startTime;
        }

        private static bool TimesOverlap(string existingStart, int existingDuration, string newStart, int newDuration)
        {
            if (!TimeSpan.TryParse(existingStart, out var existStart) || 
                !TimeSpan.TryParse(newStart, out var newStartTime))
            {
                return existingStart == newStart; // Fallback to exact match
            }

            var existEnd = existStart.Add(TimeSpan.FromMinutes(existingDuration));
            var newEnd = newStartTime.Add(TimeSpan.FromMinutes(newDuration));

            // Check if ranges overlap
            return newStartTime < existEnd && newEnd > existStart;
        }

        private static List<string> GenerateTimeSlots(string startTime, string endTime, int slotDurationMinutes)
        {
            var slots = new List<string>();
            
            if (!TimeSpan.TryParse(startTime, out var start) || !TimeSpan.TryParse(endTime, out var end))
            {
                return slots;
            }

            var current = start;
            while (current.Add(TimeSpan.FromMinutes(slotDurationMinutes)) <= end)
            {
                slots.Add(current.ToString(@"hh\:mm"));
                current = current.Add(TimeSpan.FromMinutes(slotDurationMinutes));
            }

            return slots;
        }

        // Email notification methods
        private async Task SendBookingConfirmationEmailAsync(DoctorAppointment appointment)
        {
            try
            {
                var bookingEmail = await BuildBookingEmailDto(appointment);
                if (bookingEmail != null)
                {
                    await _emailService.SendBookingConfirmationAsync(bookingEmail);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the booking
                Console.WriteLine($"Failed to send booking confirmation email: {ex.Message}");
            }
        }

        private async Task SendBookingCancellationEmailAsync(DoctorAppointment appointment, string? reason)
        {
            try
            {
                var bookingEmail = await BuildBookingEmailDto(appointment);
                if (bookingEmail != null)
                {
                    bookingEmail.CancellationReason = reason;
                    await _emailService.SendBookingCancellationAsync(bookingEmail);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send cancellation email: {ex.Message}");
            }
        }

        private async Task<BookingEmailDto?> BuildBookingEmailDto(DoctorAppointment appointment)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(appointment.PatientId);
                var doctor = await _doctorRepository.GetByIdAsync(appointment.DoctorId);
                
                if (patient == null || doctor == null) return null;

                string hospitalName = "Hospital";
                string hospitalAddress = "";
                
                if (appointment.HospitalId.HasValue)
                {
                    var hospital = await _hospitalRepository.GetByIdAsync(appointment.HospitalId.Value);
                    if (hospital != null)
                    {
                        hospitalName = hospital.Name;
                        hospitalAddress = hospital.Address ?? "";
                    }
                }

                return new BookingEmailDto
                {
                    PatientEmail = patient.ContactInfo.EmailAddress,
                    PatientName = $"{patient.FirstName} {patient.LastName}",
                    DoctorName = doctor.Name,
                    DoctorSpecialization = doctor.Qualification ?? "",
                    HospitalName = hospitalName,
                    HospitalAddress = hospitalAddress,
                    AppointmentDate = appointment.AppointmentDate,
                    AppointmentTime = appointment.AppointmentTime,
                    AppointmentId = appointment.AppointmentId.ToString(),
                    BookingReference = $"APT-{appointment.AppointmentId.ToString().Substring(0, 8).ToUpper()}"
                };
            }
            catch
            {
                return null;
            }
        }

        private async Task CreateBookingNotificationAsync(DoctorAppointment appointment)
        {
            try
            {
                var doctor = await _doctorRepository.GetByIdAsync(appointment.DoctorId);
                string hospitalName = "Hospital";
                
                if (appointment.HospitalId.HasValue)
                {
                    var hospital = await _hospitalRepository.GetByIdAsync(appointment.HospitalId.Value);
                    hospitalName = hospital?.Name ?? "Hospital";
                }

                await _notificationService.CreateBookingNotificationAsync(
                    appointment.PatientId,
                    doctor?.Name ?? "Doctor",
                    appointment.AppointmentDate,
                    appointment.AppointmentTime,
                    hospitalName,
                    appointment.AppointmentId.ToString()
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create booking notification: {ex.Message}");
            }
        }

        private async Task CreateCancellationNotificationAsync(DoctorAppointment appointment)
        {
            try
            {
                var doctor = await _doctorRepository.GetByIdAsync(appointment.DoctorId);

                await _notificationService.CreateBookingCancellationNotificationAsync(
                    appointment.PatientId,
                    doctor?.Name ?? "Doctor",
                    appointment.AppointmentDate,
                    appointment.AppointmentTime,
                    appointment.AppointmentId.ToString()
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create cancellation notification: {ex.Message}");
            }
        }
    }
}
