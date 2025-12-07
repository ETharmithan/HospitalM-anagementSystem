using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HospitalManagementSystem.Domain.Models.Doctors;
using HospitalManagementSystem.Domain.Models.Patient;

namespace HospitalManagementSystem.Domain.Models.Chat
{
    public class ChatRequest
    {
        [Key]
        public Guid RequestId { get; set; } = Guid.NewGuid();
        
        // Requester (Patient)
        public Guid PatientId { get; set; }
        
        // Target (Doctor or Admin)
        public Guid? DoctorId { get; set; }
        public Guid? AdminId { get; set; }
        
        // Request details
        public string RequestType { get; set; } = "Text"; // Text, Video
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Declined, Expired, Cancelled
        public string? Message { get; set; } // Optional message with request
        public string? DeclineReason { get; set; }
        
        // Timestamps
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(5); // Request expires in 5 mins
        
        // If accepted, link to session
        public Guid? SessionId { get; set; }
        
        // Navigation properties
        [ForeignKey("PatientId")]
        public virtual Patient.Patient Patient { get; set; } = null!;
        
        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }
        
        [ForeignKey("AdminId")]
        public virtual User? Admin { get; set; }
        
        [ForeignKey("SessionId")]
        public virtual ChatSession? Session { get; set; }
    }
}
