using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HospitalManagementSystem.Domain.Models.Doctors;

namespace HospitalManagementSystem.Domain.Models.Chat
{
    public class DoctorChatAvailability
    {
        [Key]
        public Guid AvailabilityId { get; set; } = Guid.NewGuid();
        
        public Guid DoctorId { get; set; }
        
        // Availability status
        public bool IsAvailableForChat { get; set; } = false;
        public bool IsAvailableForVideo { get; set; } = false;
        
        // Current status
        public string Status { get; set; } = "Offline"; // Online, Busy, Away, Offline
        public string? StatusMessage { get; set; } // Custom status message
        
        // Connection info (for SignalR)
        public string? ConnectionId { get; set; }
        
        // Timestamps
        public DateTime LastOnlineAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Settings
        public int MaxConcurrentChats { get; set; } = 3;
        public int CurrentActiveChatCount { get; set; } = 0;
        
        // Navigation
        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; } = null!;
    }
}
