using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models.Chat;
using HospitalManagementSystem.Infrastructure.Data;

namespace HospitalManagementSystem.Infrastructure.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDbContext _context;

        public ChatRepository(AppDbContext context)
        {
            _context = context;
        }

        // ==================== CHAT SESSIONS ====================

        public async Task<ChatSession> CreateSessionAsync(ChatSession session)
        {
            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<ChatSession?> GetSessionByIdAsync(Guid sessionId)
        {
            return await _context.ChatSessions
                .Include(s => s.Patient)
                .Include(s => s.Doctor)
                .Include(s => s.Admin)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);
        }

        public async Task<List<ChatSession>> GetSessionsByPatientIdAsync(Guid patientId)
        {
            return await _context.ChatSessions
                .Include(s => s.Doctor)
                .Include(s => s.Admin)
                .Where(s => s.PatientId == patientId && s.Status != "Archived")
                .OrderByDescending(s => s.LastMessageAt)
                .ToListAsync();
        }

        public async Task<List<ChatSession>> GetSessionsByDoctorIdAsync(Guid doctorId)
        {
            return await _context.ChatSessions
                .Include(s => s.Patient)
                .Where(s => s.DoctorId == doctorId && s.Status != "Archived")
                .OrderByDescending(s => s.LastMessageAt)
                .ToListAsync();
        }

        public async Task<List<ChatSession>> GetSessionsByAdminIdAsync(Guid adminId)
        {
            return await _context.ChatSessions
                .Include(s => s.Patient)
                .Where(s => s.AdminId == adminId && s.Status != "Archived")
                .OrderByDescending(s => s.LastMessageAt)
                .ToListAsync();
        }

        public async Task<ChatSession?> UpdateSessionAsync(ChatSession session)
        {
            _context.ChatSessions.Update(session);
            await _context.SaveChangesAsync();
            return session;
        }

        // ==================== CHAT MESSAGES ====================

        public async Task<ChatMessage> CreateMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            
            // Update session's last message time
            var session = await _context.ChatSessions.FindAsync(message.SessionId);
            if (session != null)
            {
                session.LastMessageAt = message.SentAt;
            }
            
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<ChatMessage>> GetMessagesBySessionIdAsync(Guid sessionId, int skip = 0, int take = 50)
        {
            return await _context.ChatMessages
                .Where(m => m.SessionId == sessionId && !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .Skip(skip)
                .Take(take)
                .OrderBy(m => m.SentAt) // Return in chronological order
                .ToListAsync();
        }

        public async Task<bool> MarkMessagesAsReadAsync(Guid sessionId, Guid userId)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.SessionId == sessionId && m.SenderId != userId && !m.IsRead)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountForUserAsync(Guid userId, string userRole)
        {
            // Get all sessions for this user
            IQueryable<ChatSession> sessionsQuery;
            
            if (userRole == "Doctor")
            {
                sessionsQuery = _context.ChatSessions.Where(s => s.DoctorId == userId);
            }
            else if (userRole == "Admin")
            {
                sessionsQuery = _context.ChatSessions.Where(s => s.AdminId == userId);
            }
            else
            {
                sessionsQuery = _context.ChatSessions.Where(s => s.PatientId == userId);
            }

            var sessionIds = await sessionsQuery.Select(s => s.SessionId).ToListAsync();

            return await _context.ChatMessages
                .Where(m => sessionIds.Contains(m.SessionId) && m.SenderId != userId && !m.IsRead)
                .CountAsync();
        }

        public async Task<ChatMessage?> GetLastMessageAsync(Guid sessionId)
        {
            return await _context.ChatMessages
                .Where(m => m.SessionId == sessionId && !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();
        }

        // ==================== CHAT REQUESTS ====================

        public async Task<ChatRequest> CreateRequestAsync(ChatRequest request)
        {
            _context.ChatRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<ChatRequest?> GetRequestByIdAsync(Guid requestId)
        {
            return await _context.ChatRequests
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);
        }

        public async Task<List<ChatRequest>> GetPendingRequestsForDoctorAsync(Guid doctorId)
        {
            return await _context.ChatRequests
                .Include(r => r.Patient)
                .Where(r => r.DoctorId == doctorId && r.Status == "Pending" && r.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

        public async Task<List<ChatRequest>> GetRequestsByPatientIdAsync(Guid patientId)
        {
            return await _context.ChatRequests
                .Include(r => r.Doctor)
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.RequestedAt)
                .Take(20)
                .ToListAsync();
        }

        public async Task<ChatRequest?> UpdateRequestAsync(ChatRequest request)
        {
            _context.ChatRequests.Update(request);
            await _context.SaveChangesAsync();
            return request;
        }

        // ==================== DOCTOR CHAT AVAILABILITY ====================

        public async Task<DoctorChatAvailability?> GetDoctorAvailabilityAsync(Guid doctorId)
        {
            return await _context.DoctorChatAvailabilities
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.DoctorId == doctorId);
        }

        public async Task<DoctorChatAvailability> CreateOrUpdateAvailabilityAsync(DoctorChatAvailability availability)
        {
            var existing = await _context.DoctorChatAvailabilities
                .FirstOrDefaultAsync(a => a.DoctorId == availability.DoctorId);

            if (existing != null)
            {
                existing.IsAvailableForChat = availability.IsAvailableForChat;
                existing.IsAvailableForVideo = availability.IsAvailableForVideo;
                existing.Status = availability.Status;
                existing.StatusMessage = availability.StatusMessage;
                existing.ConnectionId = availability.ConnectionId;
                existing.UpdatedAt = DateTime.UtcNow;
                
                if (availability.IsAvailableForChat || availability.IsAvailableForVideo)
                {
                    existing.LastOnlineAt = DateTime.UtcNow;
                }
            }
            else
            {
                _context.DoctorChatAvailabilities.Add(availability);
            }

            await _context.SaveChangesAsync();
            return existing ?? availability;
        }

        public async Task<List<DoctorChatAvailability>> GetAvailableDoctorsAsync()
        {
            return await _context.DoctorChatAvailabilities
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .Where(a => (a.IsAvailableForChat || a.IsAvailableForVideo) && a.Status == "Online")
                .ToListAsync();
        }

        public async Task<List<DoctorChatAvailability>> GetAllDoctorsWithAvailabilityAsync()
        {
            // Get all doctors
            var allDoctors = await _context.Doctors
                .Include(d => d.Department)
                .ToListAsync();

            // Get existing availabilities
            var availabilities = await _context.DoctorChatAvailabilities
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .ToListAsync();

            var availabilityDict = availabilities.ToDictionary(a => a.DoctorId);

            // Create availability records for doctors that don't have one
            var result = new List<DoctorChatAvailability>();
            foreach (var doctor in allDoctors)
            {
                if (availabilityDict.TryGetValue(doctor.DoctorId, out var availability))
                {
                    result.Add(availability);
                }
                else
                {
                    // Create a default offline availability for doctors without a record
                    result.Add(new DoctorChatAvailability
                    {
                        DoctorId = doctor.DoctorId,
                        Doctor = doctor,
                        IsAvailableForChat = false,
                        IsAvailableForVideo = false,
                        Status = "Offline",
                        LastOnlineAt = DateTime.UtcNow,
                        MaxConcurrentChats = 5
                    });
                }
            }

            return result;
        }

        public async Task<bool> UpdateDoctorConnectionIdAsync(Guid doctorId, string connectionId)
        {
            var availability = await _context.DoctorChatAvailabilities
                .FirstOrDefaultAsync(a => a.DoctorId == doctorId);

            if (availability != null)
            {
                availability.ConnectionId = connectionId;
                availability.LastOnlineAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> SetDoctorOfflineAsync(Guid doctorId)
        {
            var availability = await _context.DoctorChatAvailabilities
                .FirstOrDefaultAsync(a => a.DoctorId == doctorId);

            if (availability != null)
            {
                availability.Status = "Offline";
                availability.IsAvailableForChat = false;
                availability.IsAvailableForVideo = false;
                availability.ConnectionId = null;
                availability.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
