using HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto;
using HospitalManagementSystem.Application.IServices.DoctorIServices;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models.Doctors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Services.DoctorServices
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IDoctorScheduleRepository _doctorScheduleRepository;
        private readonly IDoctorAppointmentRepository _doctorAppointmentRepository;
        private readonly IDoctorLeaveRepository _doctorLeaveRepository;
        private readonly IDoctorAvailabilityRepository _doctorAvailabilityRepository;

        public AvailabilityService(
            IDoctorRepository doctorRepository,
            IDoctorScheduleRepository doctorScheduleRepository,
            IDoctorAppointmentRepository doctorAppointmentRepository,
            IDoctorLeaveRepository doctorLeaveRepository,
            IDoctorAvailabilityRepository doctorAvailabilityRepository)
        {
            _doctorRepository = doctorRepository;
            _doctorScheduleRepository = doctorScheduleRepository;
            _doctorAppointmentRepository = doctorAppointmentRepository;
            _doctorLeaveRepository = doctorLeaveRepository;
            _doctorAvailabilityRepository = doctorAvailabilityRepository;
        }

        public async Task<AvailabilityResponseDto> GetAvailabilityAsync(Guid doctorId, DateTime date)
        {
            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            var response = new AvailabilityResponseDto
            {
                Date = date.Date,
                DoctorId = doctorId,
                DoctorName = doctor.Name,
                AppointmentDurationMinutes = doctor.AppointmentDurationMinutes
            };

            // Check if doctor is on leave
            var leaves = await _doctorLeaveRepository.GetByDoctorIdAsync(doctorId);
            var isOnLeave = leaves.Any(l => date.Date >= l.StartDate.Date && date.Date <= l.EndDate.Date);
            
            if (isOnLeave)
            {
                response.IsOnLeave = true;
                response.UnavailableReason = "Doctor is on leave";
                return response;
            }

            // Check for date-specific availability override
            var dateAvailability = await _doctorAvailabilityRepository.GetByDoctorIdAndDateAsync(doctorId, date);
            
            string? startTime = null;
            string? endTime = null;
            bool hasSchedule = false;

            if (dateAvailability != null)
            {
                // Use date-specific override
                if (!dateAvailability.IsAvailable)
                {
                    response.UnavailableReason = dateAvailability.Reason ?? "Not available on this date";
                    return response;
                }
                startTime = dateAvailability.StartTime;
                endTime = dateAvailability.EndTime;
                hasSchedule = true;
            }
            else
            {
                // Check DoctorSchedule: First try specific date, then weekly recurring
                var dayOfWeek = date.DayOfWeek.ToString();
                var schedules = await _doctorScheduleRepository.GetByDoctorIdAsync(doctorId);
                
                // Priority 1: Specific date schedule (ScheduleDate matches exactly)
                var schedule = schedules.FirstOrDefault(s => 
                    s.ScheduleDate.HasValue && s.ScheduleDate.Value.Date == date.Date);
                
                // Priority 2: Weekly recurring schedule (DayOfWeek matches)
                if (schedule == null)
                {
                    schedule = schedules.FirstOrDefault(s => 
                        !string.IsNullOrEmpty(s.DayOfWeek) && 
                        s.DayOfWeek.Equals(dayOfWeek, StringComparison.OrdinalIgnoreCase));
                }
                
                if (schedule == null)
                {
                    response.UnavailableReason = "No schedule for this day";
                    return response;
                }
                
                startTime = schedule.StartTime;
                endTime = schedule.EndTime;
                hasSchedule = true;
            }

            response.HasSchedule = hasSchedule;

            // Generate time slots
            var slots = GenerateTimeSlots(startTime!, endTime!, doctor.AppointmentDurationMinutes, doctor.BreakTimeMinutes);
            
            // Get existing appointments for this date
            var appointments = await _doctorAppointmentRepository.GetByDoctorIdAsync(doctorId);
            var dateAppointments = appointments
                .Where(a => a.AppointmentDate.Date == date.Date && 
                           a.AppointmentStatus.ToLower() != "cancelled")
                .ToList();

            // Mark booked slots as unavailable
            var availableSlots = new List<TimeSlotDto>();
            var now = DateTime.Now;
            var isToday = date.Date == now.Date;
            var currentTime = now.TimeOfDay;

            foreach (var slot in slots)
            {
                // Be defensive: if slot time cannot be parsed, skip this slot instead of crashing
                if (!TimeSpan.TryParse(slot, out var slotTime))
                {
                    continue;
                }

                // Filter out past time slots for today
                if (isToday && slotTime <= currentTime)
                {
                    continue; // Skip past time slots
                }

                var isBooked = dateAppointments.Any(apt =>
                {
                    if (string.IsNullOrWhiteSpace(apt.AppointmentTime) ||
                        !TimeSpan.TryParse(apt.AppointmentTime, out var aptTime))
                    {
                        // If an appointment has an invalid time format, ignore it for overlap checks
                        return false;
                    }

                    var aptEndTime = aptTime.Add(TimeSpan.FromMinutes(doctor.AppointmentDurationMinutes));
                    var slotEndTime = slotTime.Add(TimeSpan.FromMinutes(doctor.AppointmentDurationMinutes));
                    
                    // Check if slots overlap
                    return (slotTime >= aptTime && slotTime < aptEndTime) ||
                           (aptTime >= slotTime && aptTime < slotEndTime);
                });

                availableSlots.Add(new TimeSlotDto
                {
                    Time = slot,
                    Available = !isBooked,
                    Reason = isBooked ? "Already booked" : null
                });
            }

            response.AvailableSlots = availableSlots;
            response.IsFullyBooked = availableSlots.All(s => !s.Available);

            return response;
        }

        public async Task<AvailableDatesResponseDto> GetAvailableDatesAsync(Guid doctorId, DateTime startDate, DateTime endDate)
        {
            var response = new AvailableDatesResponseDto
            {
                DoctorId = doctorId,
                StartDate = startDate.Date,
                EndDate = endDate.Date
            };

            // Get doctor info
            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            // Get leaves
            var leaves = await _doctorLeaveRepository.GetByDoctorIdAsync(doctorId);
            var leaveDates = new HashSet<DateTime>();
            foreach (var leave in leaves)
            {
                for (var date = leave.StartDate.Date; date <= leave.EndDate.Date; date = date.AddDays(1))
                {
                    leaveDates.Add(date);
                }
            }

            // Get date-specific availabilities
            var dateAvailabilities = await _doctorAvailabilityRepository.GetByDoctorIdAndDateRangeAsync(doctorId, startDate, endDate);
            var availabilityDict = dateAvailabilities.ToDictionary(a => a.Date.Date, a => a);

            // Get schedules (both specific dates and weekly recurring)
            var schedules = await _doctorScheduleRepository.GetByDoctorIdAsync(doctorId);
            var specificDateSchedules = schedules
                .Where(s => s.ScheduleDate.HasValue)
                .GroupBy(s => s.ScheduleDate!.Value.Date)
                .ToDictionary(g => g.Key, g => g.First()); // Take first schedule if duplicates exist
            var scheduledDays = schedules
                .Where(s => !string.IsNullOrEmpty(s.DayOfWeek))
                .Select(s => s.DayOfWeek!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Get all appointments
            var appointments = await _doctorAppointmentRepository.GetByDoctorIdAsync(doctorId);
            var appointmentsByDate = appointments
                .Where(a => a.AppointmentDate.Date >= startDate && a.AppointmentDate.Date <= endDate &&
                           a.AppointmentStatus.ToLower() != "cancelled")
                .GroupBy(a => a.AppointmentDate.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Check each date in range
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                // Skip past dates
                if (date < DateTime.Today)
                    continue;

                // Check if on leave
                if (leaveDates.Contains(date))
                {
                    response.UnavailableDates.Add(date);
                    continue;
                }

                // Check date-specific availability override
                if (availabilityDict.TryGetValue(date, out var dateAvailability))
                {
                    if (!dateAvailability.IsAvailable)
                    {
                        response.UnavailableDates.Add(date);
                        continue;
                    }
                    // Has date-specific schedule, check if fully booked
                    try
                    {
                        var availability = await GetAvailabilityAsync(doctorId, date);
                        if (availability.IsFullyBooked)
                        {
                            response.FullyBookedDates.Add(date);
                        }
                        else if (availability.HasSchedule)
                        {
                            response.AvailableDates.Add(date);
                        }
                    }
                    catch
                    {
                        // If error checking availability, mark as unavailable
                        response.UnavailableDates.Add(date);
                    }
                    continue;
                }

                // Check if there's a specific date schedule or weekly schedule
                var dayOfWeek = date.DayOfWeek.ToString();
                var hasSchedule = specificDateSchedules.ContainsKey(date) || scheduledDays.Contains(dayOfWeek);
                
                if (!hasSchedule)
                {
                    response.UnavailableDates.Add(date);
                    continue;
                }

                // Check if fully booked
                try
                {
                    var dayAvailability = await GetAvailabilityAsync(doctorId, date);
                    if (dayAvailability.IsFullyBooked)
                    {
                        response.FullyBookedDates.Add(date);
                    }
                    else
                    {
                        response.AvailableDates.Add(date);
                    }
                }
                catch
                {
                    // If error checking availability, mark as unavailable
                    response.UnavailableDates.Add(date);
                }
            }

            return response;
        }

        public async Task<bool> IsSlotAvailableAsync(Guid doctorId, DateTime date, string time)
        {
            var availability = await GetAvailabilityAsync(doctorId, date);
            if (!availability.HasSchedule || availability.IsOnLeave)
                return false;

            var slot = availability.AvailableSlots.FirstOrDefault(s => s.Time == time);
            return slot?.Available ?? false;
        }

        private List<string> GenerateTimeSlots(string startTime, string endTime, int durationMinutes, int breakMinutes)
        {
            var slots = new List<string>();

            // Be defensive when parsing configured times
            if (!TimeSpan.TryParse(startTime, out var start) ||
                !TimeSpan.TryParse(endTime, out var end))
            {
                // If configuration is bad, return empty slot list instead of throwing
                return slots;
            }

            var duration = TimeSpan.FromMinutes(durationMinutes);
            var breakTime = TimeSpan.FromMinutes(breakMinutes);

            var current = start;
            while (current + duration <= end)
            {
                slots.Add(current.ToString(@"hh\:mm")); // 24-hour format
                current = current.Add(duration).Add(breakTime);
            }

            return slots;
        }
    }
}

