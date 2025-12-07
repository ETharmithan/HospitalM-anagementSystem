using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Domain.Models.Doctors
{
    /// <summary>
    /// Date-specific availability overrides for doctors
    /// Allows doctors/admins to set custom availability for specific dates
    /// </summary>
    public class DoctorAvailability
    {
        [Key]
        public Guid AvailabilityId { get; set; }
        
        public required Guid DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;
        
        // Hospital where this availability applies (null = all hospitals)
        public Guid? HospitalId { get; set; }
        
        [Required]
        public DateTime Date { get; set; } // Specific date (time part ignored)
        
        [Required]
        public required string StartTime { get; set; } // Format: "HH:mm" (e.g., "09:00")
        
        [Required]
        public required string EndTime { get; set; } // Format: "HH:mm" (e.g., "17:00")
        
        // Duration per appointment in minutes
        public int SlotDurationMinutes { get; set; } = 30;
        
        // Maximum appointments for this time period
        public int MaxAppointments { get; set; } = 10;
        
        public bool IsAvailable { get; set; } = true; // If false, doctor is unavailable on this date
        
        public string? Reason { get; set; } // Optional reason (e.g., "Holiday", "Conference", "Special Schedule")
        
        // Who created this availability (Doctor or Admin)
        public Guid? CreatedByUserId { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
    }
}

