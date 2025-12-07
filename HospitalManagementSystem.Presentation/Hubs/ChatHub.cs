using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HospitalManagementSystem.Application.IServices;

namespace HospitalManagementSystem.Presentation.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private static readonly Dictionary<string, string> _userConnections = new();
        private static readonly Dictionary<string, string> _connectionUsers = new();

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            var userRole = GetUserRole();
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Store connection mapping
                _userConnections[userId] = Context.ConnectionId;
                _connectionUsers[Context.ConnectionId] = userId;

                // If doctor, update their connection ID in availability
                if (userRole == "Doctor")
                {
                    await _chatService.UpdateDoctorConnectionAsync(userId, Context.ConnectionId);
                }

                // Join user-specific group for targeted messages
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                // Notify others that user is online
                await Clients.Others.SendAsync("UserOnline", userId, userRole);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            var userRole = GetUserRole();

            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.Remove(userId);
                _connectionUsers.Remove(Context.ConnectionId);

                // If doctor, update their status to offline
                if (userRole == "Doctor")
                {
                    await _chatService.SetDoctorOfflineAsync(userId);
                }

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                await Clients.Others.SendAsync("UserOffline", userId, userRole);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ==================== CHAT METHODS ====================

        /// <summary>
        /// Send a text message in a chat session
        /// </summary>
        public async Task SendMessage(string sessionId, string content, string messageType = "Text")
        {
            var senderId = GetUserId();
            var senderRole = GetUserRole();
            var senderName = GetUserName();

            if (string.IsNullOrEmpty(senderId)) return;

            var message = await _chatService.SendMessageAsync(
                Guid.Parse(sessionId),
                Guid.Parse(senderId),
                senderRole,
                senderName,
                content,
                messageType
            );

            if (message != null)
            {
                // Send to all participants in the session
                await Clients.Group($"session_{sessionId}").SendAsync("ReceiveMessage", message);
            }
        }

        /// <summary>
        /// Join a chat session room
        /// </summary>
        public async Task JoinSession(string sessionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");
            await Clients.Group($"session_{sessionId}").SendAsync("UserJoinedSession", GetUserId(), GetUserName());
        }

        /// <summary>
        /// Leave a chat session room
        /// </summary>
        public async Task LeaveSession(string sessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session_{sessionId}");
            await Clients.Group($"session_{sessionId}").SendAsync("UserLeftSession", GetUserId(), GetUserName());
        }

        /// <summary>
        /// Mark messages as read
        /// </summary>
        public async Task MarkMessagesAsRead(string sessionId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return;

            await _chatService.MarkMessagesAsReadAsync(Guid.Parse(sessionId), Guid.Parse(userId));
            await Clients.Group($"session_{sessionId}").SendAsync("MessagesRead", sessionId, userId);
        }

        /// <summary>
        /// Typing indicator
        /// </summary>
        public async Task StartTyping(string sessionId)
        {
            await Clients.OthersInGroup($"session_{sessionId}").SendAsync("UserTyping", GetUserId(), GetUserName());
        }

        public async Task StopTyping(string sessionId)
        {
            await Clients.OthersInGroup($"session_{sessionId}").SendAsync("UserStoppedTyping", GetUserId());
        }

        // ==================== CHAT REQUEST METHODS ====================

        /// <summary>
        /// Patient requests a chat with a doctor
        /// </summary>
        public async Task RequestChat(string doctorId, string requestType, string? message)
        {
            var patientId = GetUserId();
            if (string.IsNullOrEmpty(patientId)) return;

            var request = await _chatService.CreateChatRequestAsync(
                Guid.Parse(patientId),
                Guid.Parse(doctorId),
                requestType,
                message
            );

            if (request != null)
            {
                // Notify the doctor
                if (_userConnections.TryGetValue(doctorId, out var doctorConnectionId))
                {
                    await Clients.Client(doctorConnectionId).SendAsync("ChatRequestReceived", request);
                }
                
                // Also send to doctor's user group (in case they have multiple connections)
                await Clients.Group($"user_{doctorId}").SendAsync("ChatRequestReceived", request);
                
                // Confirm to patient
                await Clients.Caller.SendAsync("ChatRequestSent", request);
            }
        }

        /// <summary>
        /// Doctor accepts a chat request
        /// </summary>
        public async Task AcceptChatRequest(string requestId)
        {
            var doctorId = GetUserId();
            if (string.IsNullOrEmpty(doctorId)) return;

            var result = await _chatService.AcceptChatRequestAsync(Guid.Parse(requestId), Guid.Parse(doctorId));

            if (result != null)
            {
                // Notify patient
                await Clients.Group($"user_{result.PatientId}").SendAsync("ChatRequestAccepted", result);
                
                // Confirm to doctor
                await Clients.Caller.SendAsync("ChatRequestAccepted", result);

                // Both join the session
                await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{result.SessionId}");
            }
        }

        /// <summary>
        /// Doctor declines a chat request
        /// </summary>
        public async Task DeclineChatRequest(string requestId, string? reason)
        {
            var doctorId = GetUserId();
            if (string.IsNullOrEmpty(doctorId)) return;

            var result = await _chatService.DeclineChatRequestAsync(Guid.Parse(requestId), reason);

            if (result != null)
            {
                // Notify patient
                await Clients.Group($"user_{result.PatientId}").SendAsync("ChatRequestDeclined", result);
                
                // Confirm to doctor
                await Clients.Caller.SendAsync("ChatRequestDeclined", result);
            }
        }

        /// <summary>
        /// Patient cancels their chat request
        /// </summary>
        public async Task CancelChatRequest(string requestId)
        {
            var patientId = GetUserId();
            if (string.IsNullOrEmpty(patientId)) return;

            var result = await _chatService.CancelChatRequestAsync(Guid.Parse(requestId));

            if (result != null && result.DoctorId.HasValue)
            {
                // Notify doctor
                await Clients.Group($"user_{result.DoctorId}").SendAsync("ChatRequestCancelled", result);
            }
        }

        // ==================== DOCTOR AVAILABILITY METHODS ====================

        /// <summary>
        /// Doctor sets their chat availability
        /// </summary>
        public async Task SetChatAvailability(bool isAvailableForChat, bool isAvailableForVideo, string? statusMessage)
        {
            var doctorId = GetUserId();
            var userRole = GetUserRole();
            
            if (string.IsNullOrEmpty(doctorId))
            {
                throw new HubException("User not authenticated");
            }
            
            if (userRole != "Doctor")
            {
                // Silently ignore for non-doctors instead of throwing error
                return;
            }

            await _chatService.SetDoctorAvailabilityAsync(
                Guid.Parse(doctorId),
                isAvailableForChat,
                isAvailableForVideo,
                statusMessage,
                Context.ConnectionId
            );

            // Broadcast to all clients
            await Clients.All.SendAsync("DoctorAvailabilityChanged", new
            {
                DoctorId = doctorId,
                IsAvailableForChat = isAvailableForChat,
                IsAvailableForVideo = isAvailableForVideo,
                StatusMessage = statusMessage
            });
        }

        // ==================== VIDEO CALL SIGNALING (WebRTC) ====================

        /// <summary>
        /// Initiate a video call
        /// </summary>
        public async Task InitiateVideoCall(string sessionId, string targetUserId)
        {
            var callerId = GetUserId();
            var callerName = GetUserName();

            await Clients.Group($"user_{targetUserId}").SendAsync("IncomingVideoCall", new
            {
                SessionId = sessionId,
                CallerId = callerId,
                CallerName = callerName
            });
        }

        /// <summary>
        /// Accept video call
        /// </summary>
        public async Task AcceptVideoCall(string sessionId, string callerId)
        {
            var userId = GetUserId();
            await Clients.Group($"user_{callerId}").SendAsync("VideoCallAccepted", new
            {
                SessionId = sessionId,
                AcceptedBy = userId
            });
        }

        /// <summary>
        /// Decline video call
        /// </summary>
        public async Task DeclineVideoCall(string sessionId, string callerId)
        {
            var userId = GetUserId();
            await Clients.Group($"user_{callerId}").SendAsync("VideoCallDeclined", new
            {
                SessionId = sessionId,
                DeclinedBy = userId
            });
        }

        /// <summary>
        /// End video call
        /// </summary>
        public async Task EndVideoCall(string sessionId)
        {
            await Clients.Group($"session_{sessionId}").SendAsync("VideoCallEnded", sessionId);
        }

        /// <summary>
        /// WebRTC signaling - send offer
        /// </summary>
        public async Task SendOffer(string sessionId, string targetUserId, object offer)
        {
            await Clients.Group($"user_{targetUserId}").SendAsync("ReceiveOffer", new
            {
                SessionId = sessionId,
                FromUserId = GetUserId(),
                Offer = offer
            });
        }

        /// <summary>
        /// WebRTC signaling - send answer
        /// </summary>
        public async Task SendAnswer(string sessionId, string targetUserId, object answer)
        {
            await Clients.Group($"user_{targetUserId}").SendAsync("ReceiveAnswer", new
            {
                SessionId = sessionId,
                FromUserId = GetUserId(),
                Answer = answer
            });
        }

        /// <summary>
        /// WebRTC signaling - send ICE candidate
        /// </summary>
        public async Task SendIceCandidate(string sessionId, string targetUserId, object candidate)
        {
            await Clients.Group($"user_{targetUserId}").SendAsync("ReceiveIceCandidate", new
            {
                SessionId = sessionId,
                FromUserId = GetUserId(),
                Candidate = candidate
            });
        }

        // ==================== HELPER METHODS ====================

        private string? GetUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? Context.User?.FindFirst("sub")?.Value;
        }

        private string GetUserRole()
        {
            return Context.User?.FindFirst(ClaimTypes.Role)?.Value ?? "Patient";
        }

        private string GetUserName()
        {
            return Context.User?.FindFirst(ClaimTypes.Name)?.Value 
                ?? Context.User?.FindFirst("name")?.Value 
                ?? "Unknown";
        }

        // Static method to get connection ID for a user
        public static string? GetConnectionId(string userId)
        {
            return _userConnections.TryGetValue(userId, out var connectionId) ? connectionId : null;
        }
    }
}
