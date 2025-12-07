using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HospitalManagementSystem.Domain.Models.Chat;

namespace HospitalManagementSystem.Domain.IRepository
{
    public interface IChatRepository
    {
        // Chat Sessions
        Task<ChatSession> CreateSessionAsync(ChatSession session);
        Task<ChatSession?> GetSessionByIdAsync(Guid sessionId);
        Task<List<ChatSession>> GetSessionsByPatientIdAsync(Guid patientId);
        Task<List<ChatSession>> GetSessionsByDoctorIdAsync(Guid doctorId);
        Task<List<ChatSession>> GetSessionsByAdminIdAsync(Guid adminId);
        Task<ChatSession?> UpdateSessionAsync(ChatSession session);

        // Chat Messages
        Task<ChatMessage> CreateMessageAsync(ChatMessage message);
        Task<List<ChatMessage>> GetMessagesBySessionIdAsync(Guid sessionId, int skip = 0, int take = 50);
        Task<bool> MarkMessagesAsReadAsync(Guid sessionId, Guid userId);
        Task<int> GetUnreadCountForUserAsync(Guid userId, string userRole);
        Task<ChatMessage?> GetLastMessageAsync(Guid sessionId);

        // Chat Requests
        Task<ChatRequest> CreateRequestAsync(ChatRequest request);
        Task<ChatRequest?> GetRequestByIdAsync(Guid requestId);
        Task<List<ChatRequest>> GetPendingRequestsForDoctorAsync(Guid doctorId);
        Task<List<ChatRequest>> GetRequestsByPatientIdAsync(Guid patientId);
        Task<ChatRequest?> UpdateRequestAsync(ChatRequest request);

        // Doctor Chat Availability
        Task<DoctorChatAvailability?> GetDoctorAvailabilityAsync(Guid doctorId);
        Task<DoctorChatAvailability> CreateOrUpdateAvailabilityAsync(DoctorChatAvailability availability);
        Task<List<DoctorChatAvailability>> GetAvailableDoctorsAsync();
        Task<bool> UpdateDoctorConnectionIdAsync(Guid doctorId, string connectionId);
        Task<bool> SetDoctorOfflineAsync(Guid doctorId);
    }
}
