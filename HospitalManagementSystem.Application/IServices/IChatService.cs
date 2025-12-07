using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices
{
    public interface IChatService
    {
        // Chat Sessions
        Task<ChatSessionDto?> CreateSessionAsync(Guid patientId, Guid? doctorId, Guid? adminId, string sessionType);
        Task<ChatSessionDto?> GetSessionByIdAsync(Guid sessionId);
        Task<List<ChatSessionDto>> GetSessionsByUserIdAsync(Guid userId, string userRole);
        Task<bool> EndSessionAsync(Guid sessionId);

        // Messages
        Task<ChatMessageDto?> SendMessageAsync(Guid sessionId, Guid senderId, string senderType, string senderName, string content, string messageType);
        Task<List<ChatMessageDto>> GetSessionMessagesAsync(Guid sessionId, int skip = 0, int take = 50);
        Task<bool> MarkMessagesAsReadAsync(Guid sessionId, Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId);

        // Chat Requests
        Task<ChatRequestDto?> CreateChatRequestAsync(Guid patientId, Guid doctorId, string requestType, string? message);
        Task<ChatRequestResponseDto?> AcceptChatRequestAsync(Guid requestId, Guid doctorId);
        Task<ChatRequestDto?> DeclineChatRequestAsync(Guid requestId, string? reason);
        Task<ChatRequestDto?> CancelChatRequestAsync(Guid requestId);
        Task<List<ChatRequestDto>> GetPendingRequestsForDoctorAsync(Guid doctorId);
        Task<List<ChatRequestDto>> GetRequestsByPatientAsync(Guid patientId);

        // Doctor Availability
        Task<bool> SetDoctorAvailabilityAsync(Guid doctorId, bool isAvailableForChat, bool isAvailableForVideo, string? statusMessage, string? connectionId);
        Task<bool> UpdateDoctorConnectionAsync(string doctorId, string connectionId);
        Task<bool> SetDoctorOfflineAsync(string doctorId);
        Task<DoctorChatAvailabilityDto?> GetDoctorAvailabilityAsync(Guid doctorId);
        Task<List<DoctorChatAvailabilityDto>> GetAvailableDoctorsAsync();
    }

    // DTOs
    public class ChatSessionDto
    {
        public Guid SessionId { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public Guid? AdminId { get; set; }
        public string? AdminName { get; set; }
        public string SessionType { get; set; } = "Text";
        public string Status { get; set; } = "Active";
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
        public ChatMessageDto? LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }

    public class ChatMessageDto
    {
        public Guid MessageId { get; set; }
        public Guid SessionId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderType { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "Text";
        public string? AttachmentUrl { get; set; }
        public string? AttachmentName { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime SentAt { get; set; }
    }

    public class ChatRequestDto
    {
        public Guid RequestId { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public string RequestType { get; set; } = "Text";
        public string Status { get; set; } = "Pending";
        public string? Message { get; set; }
        public string? DeclineReason { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public Guid? SessionId { get; set; }
    }

    public class ChatRequestResponseDto
    {
        public Guid RequestId { get; set; }
        public Guid PatientId { get; set; }
        public Guid SessionId { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class DoctorChatAvailabilityDto
    {
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public string? ProfileImage { get; set; }
        public bool IsAvailableForChat { get; set; }
        public bool IsAvailableForVideo { get; set; }
        public string Status { get; set; } = "Offline";
        public string? StatusMessage { get; set; }
        public DateTime LastOnlineAt { get; set; }
        public int CurrentActiveChatCount { get; set; }
        public int MaxConcurrentChats { get; set; }
    }
}
