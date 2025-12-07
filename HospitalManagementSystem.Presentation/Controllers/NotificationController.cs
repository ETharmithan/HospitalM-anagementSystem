using HospitalManagementSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalManagementSystem.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // Get all notifications for current user
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications([FromQuery] string userType = "Patient")
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                    return Unauthorized(new { message = "User not found" });

                var notifications = await _notificationService.GetByUserIdAsync(userId.Value, userType);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Get unread notifications for current user
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications([FromQuery] string userType = "Patient")
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                    return Unauthorized(new { message = "User not found" });

                var notifications = await _notificationService.GetUnreadByUserIdAsync(userId.Value, userType);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Get unread count for current user
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount([FromQuery] string userType = "Patient")
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                    return Unauthorized(new { message = "User not found" });

                var count = await _notificationService.GetUnreadCountAsync(userId.Value, userType);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Mark a notification as read
        [HttpPut("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            try
            {
                var success = await _notificationService.MarkAsReadAsync(notificationId);
                if (!success)
                    return NotFound(new { message = "Notification not found" });

                return Ok(new { message = "Notification marked as read" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Mark all notifications as read
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead([FromQuery] string userType = "Patient")
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                    return Unauthorized(new { message = "User not found" });

                await _notificationService.MarkAllAsReadAsync(userId.Value, userType);
                return Ok(new { message = "All notifications marked as read" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Delete a notification
        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> Delete(Guid notificationId)
        {
            try
            {
                var success = await _notificationService.DeleteAsync(notificationId);
                if (!success)
                    return NotFound(new { message = "Notification not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Delete all notifications for current user
        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAll([FromQuery] string userType = "Patient")
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                    return Unauthorized(new { message = "User not found" });

                await _notificationService.DeleteAllByUserIdAsync(userId.Value, userType);
                return Ok(new { message = "All notifications deleted" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private Guid? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return null;
            return userId;
        }
    }
}
