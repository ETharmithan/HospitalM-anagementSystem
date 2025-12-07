using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models.Chat;

namespace HospitalManagementSystem.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;

        public ChatService(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        // ==================== CHAT SESSIONS ====================

        public async Task<ChatSessionDto?> CreateSessionAsync(Guid patientId, Guid? doctorId, Guid? adminId, string sessionType)
        {
            var session = new ChatSession
            {
                PatientId = patientId,
                DoctorId = doctorId,
                AdminId = adminId,
                SessionType = sessionType,
                Status = "Active",
                StartedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };

            var created = await _chatRepository.CreateSessionAsync(session);
            return await GetSessionByIdAsync(created.SessionId);
        }

        public async Task<ChatSessionDto?> GetSessionByIdAsync(Guid sessionId)
        {
            var session = await _chatRepository.GetSessionByIdAsync(sessionId);
            if (session == null) return null;

            var lastMessage = await _chatRepository.GetLastMessageAsync(sessionId);

            return new ChatSessionDto
            {
                SessionId = session.SessionId,
                PatientId = session.PatientId,
                PatientName = session.Patient != null ? $"{session.Patient.FirstName} {session.Patient.LastName}" : "Unknown",
                DoctorId = session.DoctorId,
                DoctorName = session.Doctor?.Name,
                AdminId = session.AdminId,
                AdminName = session.Admin?.Username,
                SessionType = session.SessionType,
                Status = session.Status,
                StartedAt = session.StartedAt,
                EndedAt = session.EndedAt,
                LastMessageAt = session.LastMessageAt,
                LastMessage = lastMessage != null ? MapMessageToDto(lastMessage) : null
            };
        }

        public async Task<List<ChatSessionDto>> GetSessionsByUserIdAsync(Guid userId, string userRole)
        {
            List<ChatSession> sessions;

            switch (userRole.ToLower())
            {
                case "doctor":
                    sessions = await _chatRepository.GetSessionsByDoctorIdAsync(userId);
                    break;
                case "admin":
                    sessions = await _chatRepository.GetSessionsByAdminIdAsync(userId);
                    break;
                default:
                    sessions = await _chatRepository.GetSessionsByPatientIdAsync(userId);
                    break;
            }

            var result = new List<ChatSessionDto>();
            foreach (var session in sessions)
            {
                var lastMessage = await _chatRepository.GetLastMessageAsync(session.SessionId);
                result.Add(new ChatSessionDto
                {
                    SessionId = session.SessionId,
                    PatientId = session.PatientId,
                    PatientName = session.Patient != null ? $"{session.Patient.FirstName} {session.Patient.LastName}" : "Unknown",
                    DoctorId = session.DoctorId,
                    DoctorName = session.Doctor?.Name,
                    AdminId = session.AdminId,
                    AdminName = session.Admin?.Username,
                    SessionType = session.SessionType,
                    Status = session.Status,
                    StartedAt = session.StartedAt,
                    EndedAt = session.EndedAt,
                    LastMessageAt = session.LastMessageAt,
                    LastMessage = lastMessage != null ? MapMessageToDto(lastMessage) : null
                });
            }

            return result;
        }

        public async Task<bool> EndSessionAsync(Guid sessionId)
        {
            var session = await _chatRepository.GetSessionByIdAsync(sessionId);
            if (session == null) return false;

            session.Status = "Ended";
            session.EndedAt = DateTime.UtcNow;
            await _chatRepository.UpdateSessionAsync(session);
            return true;
        }

        // ==================== MESSAGES ====================

        public async Task<ChatMessageDto?> SendMessageAsync(Guid sessionId, Guid senderId, string senderType, string senderName, string content, string messageType)
        {
            var message = new ChatMessage
            {
                SessionId = sessionId,
                SenderId = senderId,
                SenderType = senderType,
                SenderName = senderName,
                Content = content,
                MessageType = messageType,
                SentAt = DateTime.UtcNow
            };

            var created = await _chatRepository.CreateMessageAsync(message);
            return MapMessageToDto(created);
        }

        public async Task<List<ChatMessageDto>> GetSessionMessagesAsync(Guid sessionId, int skip = 0, int take = 50)
        {
            var messages = await _chatRepository.GetMessagesBySessionIdAsync(sessionId, skip, take);
            return messages.Select(MapMessageToDto).ToList();
        }

        public async Task<bool> MarkMessagesAsReadAsync(Guid sessionId, Guid userId)
        {
            return await _chatRepository.MarkMessagesAsReadAsync(sessionId, userId);
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            // Default to patient role for unread count
            return await _chatRepository.GetUnreadCountForUserAsync(userId, "Patient");
        }

        // ==================== CHAT REQUESTS ====================

        public async Task<ChatRequestDto?> CreateChatRequestAsync(Guid patientId, Guid doctorId, string requestType, string? message)
        {
            var request = new ChatRequest
            {
                PatientId = patientId,
                DoctorId = doctorId,
                RequestType = requestType,
                Message = message,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };

            var created = await _chatRepository.CreateRequestAsync(request);
            var full = await _chatRepository.GetRequestByIdAsync(created.RequestId);
            return MapRequestToDto(full!);
        }

        public async Task<ChatRequestResponseDto?> AcceptChatRequestAsync(Guid requestId, Guid doctorId)
        {
            var request = await _chatRepository.GetRequestByIdAsync(requestId);
            if (request == null || request.DoctorId != doctorId || request.Status != "Pending")
                return null;

            // Create a chat session
            var session = new ChatSession
            {
                PatientId = request.PatientId,
                DoctorId = doctorId,
                SessionType = request.RequestType,
                Status = "Active",
                StartedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };

            var createdSession = await _chatRepository.CreateSessionAsync(session);

            // Update request
            request.Status = "Accepted";
            request.RespondedAt = DateTime.UtcNow;
            request.SessionId = createdSession.SessionId;
            await _chatRepository.UpdateRequestAsync(request);

            // Update doctor's active chat count
            var availability = await _chatRepository.GetDoctorAvailabilityAsync(doctorId);
            if (availability != null)
            {
                availability.CurrentActiveChatCount++;
                await _chatRepository.CreateOrUpdateAvailabilityAsync(availability);
            }

            return new ChatRequestResponseDto
            {
                RequestId = requestId,
                PatientId = request.PatientId,
                SessionId = createdSession.SessionId,
                Status = "Accepted"
            };
        }

        public async Task<ChatRequestDto?> DeclineChatRequestAsync(Guid requestId, string? reason)
        {
            var request = await _chatRepository.GetRequestByIdAsync(requestId);
            if (request == null || request.Status != "Pending")
                return null;

            request.Status = "Declined";
            request.DeclineReason = reason;
            request.RespondedAt = DateTime.UtcNow;
            await _chatRepository.UpdateRequestAsync(request);

            return MapRequestToDto(request);
        }

        public async Task<ChatRequestDto?> CancelChatRequestAsync(Guid requestId)
        {
            var request = await _chatRepository.GetRequestByIdAsync(requestId);
            if (request == null || request.Status != "Pending")
                return null;

            request.Status = "Cancelled";
            request.RespondedAt = DateTime.UtcNow;
            await _chatRepository.UpdateRequestAsync(request);

            return MapRequestToDto(request);
        }

        public async Task<List<ChatRequestDto>> GetPendingRequestsForDoctorAsync(Guid doctorId)
        {
            var requests = await _chatRepository.GetPendingRequestsForDoctorAsync(doctorId);
            return requests.Select(MapRequestToDto).ToList();
        }

        public async Task<List<ChatRequestDto>> GetRequestsByPatientAsync(Guid patientId)
        {
            var requests = await _chatRepository.GetRequestsByPatientIdAsync(patientId);
            return requests.Select(MapRequestToDto).ToList();
        }

        // ==================== DOCTOR AVAILABILITY ====================

        public async Task<bool> SetDoctorAvailabilityAsync(Guid doctorId, bool isAvailableForChat, bool isAvailableForVideo, string? statusMessage, string? connectionId)
        {
            var availability = new DoctorChatAvailability
            {
                DoctorId = doctorId,
                IsAvailableForChat = isAvailableForChat,
                IsAvailableForVideo = isAvailableForVideo,
                Status = (isAvailableForChat || isAvailableForVideo) ? "Online" : "Offline",
                StatusMessage = statusMessage,
                ConnectionId = connectionId,
                LastOnlineAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _chatRepository.CreateOrUpdateAvailabilityAsync(availability);
            return true;
        }

        public async Task<bool> UpdateDoctorConnectionAsync(string doctorId, string connectionId)
        {
            if (!Guid.TryParse(doctorId, out var docGuid))
                return false;

            return await _chatRepository.UpdateDoctorConnectionIdAsync(docGuid, connectionId);
        }

        public async Task<bool> SetDoctorOfflineAsync(string doctorId)
        {
            if (!Guid.TryParse(doctorId, out var docGuid))
                return false;

            return await _chatRepository.SetDoctorOfflineAsync(docGuid);
        }

        public async Task<DoctorChatAvailabilityDto?> GetDoctorAvailabilityAsync(Guid doctorId)
        {
            var availability = await _chatRepository.GetDoctorAvailabilityAsync(doctorId);
            if (availability == null) return null;

            return MapAvailabilityToDto(availability);
        }

        public async Task<List<DoctorChatAvailabilityDto>> GetAvailableDoctorsAsync()
        {
            var availabilities = await _chatRepository.GetAvailableDoctorsAsync();
            return availabilities.Select(MapAvailabilityToDto).ToList();
        }

        // ==================== MAPPING HELPERS ====================

        private ChatMessageDto MapMessageToDto(ChatMessage message)
        {
            return new ChatMessageDto
            {
                MessageId = message.MessageId,
                SessionId = message.SessionId,
                SenderId = message.SenderId,
                SenderType = message.SenderType,
                SenderName = message.SenderName,
                Content = message.Content,
                MessageType = message.MessageType,
                AttachmentUrl = message.AttachmentUrl,
                AttachmentName = message.AttachmentName,
                IsRead = message.IsRead,
                ReadAt = message.ReadAt,
                SentAt = message.SentAt
            };
        }

        private ChatRequestDto MapRequestToDto(ChatRequest request)
        {
            return new ChatRequestDto
            {
                RequestId = request.RequestId,
                PatientId = request.PatientId,
                PatientName = request.Patient != null ? $"{request.Patient.FirstName} {request.Patient.LastName}" : "Unknown",
                DoctorId = request.DoctorId,
                DoctorName = request.Doctor?.Name,
                RequestType = request.RequestType,
                Status = request.Status,
                Message = request.Message,
                DeclineReason = request.DeclineReason,
                RequestedAt = request.RequestedAt,
                RespondedAt = request.RespondedAt,
                ExpiresAt = request.ExpiresAt,
                SessionId = request.SessionId
            };
        }

        private DoctorChatAvailabilityDto MapAvailabilityToDto(DoctorChatAvailability availability)
        {
            return new DoctorChatAvailabilityDto
            {
                DoctorId = availability.DoctorId,
                DoctorName = availability.Doctor?.Name ?? "Unknown",
                Specialization = availability.Doctor?.Department?.Name,
                ProfileImage = null, // Doctor model doesn't have ProfileImage
                IsAvailableForChat = availability.IsAvailableForChat,
                IsAvailableForVideo = availability.IsAvailableForVideo,
                Status = availability.Status,
                StatusMessage = availability.StatusMessage,
                LastOnlineAt = availability.LastOnlineAt,
                CurrentActiveChatCount = availability.CurrentActiveChatCount,
                MaxConcurrentChats = availability.MaxConcurrentChats
            };
        }
    }
}
