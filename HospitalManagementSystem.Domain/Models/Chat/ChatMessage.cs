using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Models.Chat
{
    public class ChatMessage
    {
        [Key]
        public Guid MessageId { get; set; } = Guid.NewGuid();
        
        public Guid SessionId { get; set; }
        
        // Sender info
        public Guid SenderId { get; set; }
        public string SenderType { get; set; } = "Patient"; // Patient, Doctor, Admin
        public string SenderName { get; set; } = string.Empty;
        
        // Message content
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "Text"; // Text, Image, File, System
        public string? AttachmentUrl { get; set; }
        public string? AttachmentName { get; set; }
        
        // Status
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        
        // Timestamps
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        
        // Navigation
        [ForeignKey("SessionId")]
        public virtual ChatSession Session { get; set; } = null!;
    }
}
