using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Domain.Models
{
    public class Notification
    {
        [Key]
        public Guid NotificationId { get; set; }
        
        public Guid UserId { get; set; }  // Can be PatientId or DoctorId
        
        public string UserType { get; set; } = "Patient";  // "Patient" or "Doctor"
        
        public string Title { get; set; } = string.Empty;
        
        public string Message { get; set; } = string.Empty;
        
        public string Type { get; set; } = "Info";  // "Info", "Success", "Warning", "Error", "Booking", "Prescription", "Payment"
        
        public bool IsRead { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string? RelatedEntityId { get; set; }  // AppointmentId, PrescriptionId, etc.
        
        public string? RelatedEntityType { get; set; }  // "Appointment", "Prescription", "Payment"
        
        public string? ActionUrl { get; set; }  // URL to navigate to when clicked
    }
}
