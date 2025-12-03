using System;
using System.Collections.Generic;

namespace HospitalManagementSystem.Application.DTOs.DoctorDto.Response_Dto
{
    public class TimeSlotDto
    {
        public string Time { get; set; } = null!; // Format: "HH:mm"
        public bool Available { get; set; }
        public string? Reason { get; set; } // If not available, reason (e.g., "Already booked")
    }

    public class AvailabilityResponseDto
    {
        public DateTime Date { get; set; }
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public int AppointmentDurationMinutes { get; set; }
        public List<TimeSlotDto> AvailableSlots { get; set; } = new();
        public bool IsFullyBooked { get; set; }
        public bool HasSchedule { get; set; }
        public bool IsOnLeave { get; set; }
        public string? UnavailableReason { get; set; }
    }

    public class AvailableDatesResponseDto
    {
        public Guid DoctorId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<DateTime> AvailableDates { get; set; } = new();
        public List<DateTime> FullyBookedDates { get; set; } = new();
        public List<DateTime> UnavailableDates { get; set; } = new(); // On leave or no schedule
    }
}

