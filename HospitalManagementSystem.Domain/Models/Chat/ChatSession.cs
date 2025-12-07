using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HospitalManagementSystem.Domain.Models.Doctors;
using HospitalManagementSystem.Domain.Models.Patient;

namespace HospitalManagementSystem.Domain.Models.Chat
{
    public class ChatSession
    {
        [Key]
        public Guid SessionId { get; set; } = Guid.NewGuid();
        
        // Participants
        public Guid PatientId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? AdminId { get; set; } // For hospital admin chats
        
        // Session info
        public string SessionType { get; set; } = "Text"; // Text, Video, Both
        public string Status { get; set; } = "Active"; // Active, Ended, Archived
        
        // Timestamps
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
        
        // Optional: Link to appointment
        public Guid? AppointmentId { get; set; }
        
        // Navigation properties
        [ForeignKey("PatientId")]
        public virtual Patient.Patient Patient { get; set; } = null!;
        
        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }
        
        [ForeignKey("AdminId")]
        public virtual User? Admin { get; set; }
        
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
