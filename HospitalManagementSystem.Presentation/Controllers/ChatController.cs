using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HospitalManagementSystem.Application.IServices;

namespace HospitalManagementSystem.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        // ==================== SESSIONS ====================

        /// <summary>
        /// Get all chat sessions for the current user
        /// </summary>
        [HttpGet("sessions")]
        public async Task<IActionResult> GetMySessions()
        {
            var userId = GetUserId();
            var userRole = GetUserRole();

            if (userId == null) return Unauthorized();

            var sessions = await _chatService.GetSessionsByUserIdAsync(Guid.Parse(userId), userRole);
            return Ok(sessions);
        }

        /// <summary>
        /// Get a specific chat session
        /// </summary>
        [HttpGet("sessions/{sessionId}")]
        public async Task<IActionResult> GetSession(Guid sessionId)
        {
            var session = await _chatService.GetSessionByIdAsync(sessionId);
            if (session == null) return NotFound();
            return Ok(session);
        }

        /// <summary>
        /// Create a direct chat session (without request)
        /// </summary>
        [HttpPost("sessions/direct")]
        public async Task<IActionResult> CreateDirectSession([FromBody] CreateDirectSessionDto dto)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null) return Unauthorized();

                var userRole = GetUserRole();
                Guid patientId;
                Guid doctorId;

                // Determine patient and doctor IDs based on user role
                if (userRole == "Patient")
                {
                    patientId = Guid.Parse(userId);
                    doctorId = dto.DoctorId;
                }
                else if (userRole == "Doctor")
                {
                    doctorId = Guid.Parse(userId);
                    patientId = dto.DoctorId; // In this case, DoctorId field contains PatientId
                }
                else
                {
                    return BadRequest("Only patients and doctors can create chat sessions");
                }

                // Check if session already exists
                var existingSessions = await _chatService.GetSessionsByUserIdAsync(patientId, "Patient");
                var existingSession = existingSessions.FirstOrDefault(s => s.DoctorId == doctorId);
                
                if (existingSession != null)
                {
                    return Ok(existingSession);
                }

                // Create new session
                var session = await _chatService.CreateSessionAsync(
                    patientId,
                    doctorId,
                    null,
                    "Text"
                );

                if (session == null) return BadRequest("Failed to create session");
                return Ok(session);
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error creating direct session: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message, details = ex.InnerException?.Message });
            }
        }

        /// <summary>
        /// End a chat session
        /// </summary>
        [HttpPost("sessions/{sessionId}/end")]
        public async Task<IActionResult> EndSession(Guid sessionId)
        {
            var result = await _chatService.EndSessionAsync(sessionId);
            if (!result) return NotFound();
            return Ok(new { message = "Session ended" });
        }

        // ==================== MESSAGES ====================

        /// <summary>
        /// Get messages for a session
        /// </summary>
        [HttpGet("sessions/{sessionId}/messages")]
        public async Task<IActionResult> GetMessages(Guid sessionId, [FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            var messages = await _chatService.GetSessionMessagesAsync(sessionId, skip, take);
            return Ok(messages);
        }

        /// <summary>
        /// Get unread message count
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var count = await _chatService.GetUnreadCountAsync(Guid.Parse(userId));
            return Ok(new { unreadCount = count });
        }

        // ==================== CHAT REQUESTS ====================

        /// <summary>
        /// Create a chat request (patient to doctor)
        /// </summary>
        [HttpPost("requests")]
        public async Task<IActionResult> CreateChatRequest([FromBody] CreateChatRequestDto dto)
        {
            var patientId = GetUserId();
            if (patientId == null) return Unauthorized();

            var request = await _chatService.CreateChatRequestAsync(
                Guid.Parse(patientId),
                dto.DoctorId,
                dto.RequestType,
                dto.Message
            );

            if (request == null) return BadRequest("Failed to create request");
            return Ok(request);
        }

        /// <summary>
        /// Get pending chat requests for the current doctor
        /// </summary>
        [HttpGet("requests/pending")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var doctorId = GetUserId();
            if (doctorId == null) return Unauthorized();

            var requests = await _chatService.GetPendingRequestsForDoctorAsync(Guid.Parse(doctorId));
            return Ok(requests);
        }

        /// <summary>
        /// Get my chat requests (for patients)
        /// </summary>
        [HttpGet("requests/my")]
        public async Task<IActionResult> GetMyRequests()
        {
            var patientId = GetUserId();
            if (patientId == null) return Unauthorized();

            var requests = await _chatService.GetRequestsByPatientAsync(Guid.Parse(patientId));
            return Ok(requests);
        }

        /// <summary>
        /// Accept a chat request (doctor only)
        /// </summary>
        [HttpPost("requests/{requestId}/accept")]
        public async Task<IActionResult> AcceptRequest(Guid requestId)
        {
            var doctorId = GetUserId();
            if (doctorId == null) return Unauthorized();

            var result = await _chatService.AcceptChatRequestAsync(requestId, Guid.Parse(doctorId));
            if (result == null) return BadRequest("Failed to accept request");
            return Ok(result);
        }

        /// <summary>
        /// Decline a chat request (doctor only)
        /// </summary>
        [HttpPost("requests/{requestId}/decline")]
        public async Task<IActionResult> DeclineRequest(Guid requestId, [FromBody] DeclineRequestDto? dto)
        {
            var result = await _chatService.DeclineChatRequestAsync(requestId, dto?.Reason);
            if (result == null) return BadRequest("Failed to decline request");
            return Ok(result);
        }

        /// <summary>
        /// Cancel a chat request (patient only)
        /// </summary>
        [HttpPost("requests/{requestId}/cancel")]
        public async Task<IActionResult> CancelRequest(Guid requestId)
        {
            var result = await _chatService.CancelChatRequestAsync(requestId);
            if (result == null) return BadRequest("Failed to cancel request");
            return Ok(result);
        }

        // ==================== DOCTOR AVAILABILITY ====================

        /// <summary>
        /// Get available doctors for chat
        /// </summary>
        [HttpGet("available-doctors")]
        public async Task<IActionResult> GetAvailableDoctors()
        {
            var doctors = await _chatService.GetAvailableDoctorsAsync();
            return Ok(doctors);
        }

        /// <summary>
        /// Get all doctors with their availability status
        /// </summary>
        [HttpGet("doctors")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _chatService.GetAllDoctorsWithAvailabilityAsync();
            return Ok(doctors);
        }

        /// <summary>
        /// Get a specific doctor's availability
        /// </summary>
        [HttpGet("availability/{doctorId}")]
        public async Task<IActionResult> GetDoctorAvailability(Guid doctorId)
        {
            var availability = await _chatService.GetDoctorAvailabilityAsync(doctorId);
            return Ok(availability);
        }

        /// <summary>
        /// Set my chat availability (doctor only)
        /// </summary>
        [HttpPost("availability")]
        public async Task<IActionResult> SetMyAvailability([FromBody] SetAvailabilityDto dto)
        {
            var doctorId = GetUserId();
            if (doctorId == null) return Unauthorized();

            var result = await _chatService.SetDoctorAvailabilityAsync(
                Guid.Parse(doctorId),
                dto.IsAvailableForChat,
                dto.IsAvailableForVideo,
                dto.StatusMessage,
                null // Connection ID is set by SignalR
            );

            return Ok(new { success = result });
        }

        // ==================== HELPERS ====================

        private string? GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;
        }

        private string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "Patient";
        }
    }

    // ==================== DTOs ====================

    public class CreateDirectSessionDto
    {
        public Guid DoctorId { get; set; }
    }

    public class CreateChatRequestDto
    {
        public Guid DoctorId { get; set; }
        public string RequestType { get; set; } = "Text";
        public string? Message { get; set; }
    }

    public class DeclineRequestDto
    {
        public string? Reason { get; set; }
    }

    public class SetAvailabilityDto
    {
        public bool IsAvailableForChat { get; set; }
        public bool IsAvailableForVideo { get; set; }
        public string? StatusMessage { get; set; }
    }
}
