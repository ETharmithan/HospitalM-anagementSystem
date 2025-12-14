using HospitalManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices
{
    public interface INotificationService
    {
        // Create notifications
        Task<Notification> CreateAsync(Notification notification);
        Task CreateBookingNotificationAsync(Guid patientId, string doctorName, DateTime appointmentDate, string appointmentTime, string hospitalName, string appointmentId);
        Task CreateBookingCancellationNotificationAsync(Guid patientId, string doctorName, DateTime appointmentDate, string appointmentTime, string appointmentId);
        Task CreateCancellationRequestNotificationAsync(Guid doctorId, string doctorName, DateTime appointmentDate, string appointmentTime, string patientName, string? reason, string appointmentId);
        Task CreateCancellationApprovalNotificationAsync(Guid patientId, DateTime appointmentDate, string appointmentTime, string appointmentId);
        Task CreateCancellationRejectionNotificationAsync(Guid patientId, DateTime appointmentDate, string appointmentTime, string? reason, string appointmentId);
        Task CreatePrescriptionNotificationAsync(Guid patientId, string doctorName, string diagnosis, string recordId);
        Task CreatePaymentNotificationAsync(Guid patientId, decimal amount, string transactionId, string appointmentId);
        
        // Get notifications
        Task<IEnumerable<NotificationDto>> GetByUserIdAsync(Guid userId, string userType);
        Task<IEnumerable<NotificationDto>> GetUnreadByUserIdAsync(Guid userId, string userType);
        Task<int> GetUnreadCountAsync(Guid userId, string userType);
        
        // Update notifications
        Task<bool> MarkAsReadAsync(Guid notificationId);
        Task<bool> MarkAllAsReadAsync(Guid userId, string userType);
        
        // Delete notifications
        Task<bool> DeleteAsync(Guid notificationId);
        Task<bool> DeleteAllByUserIdAsync(Guid userId, string userType);
    }

    public class NotificationDto
    {
        public Guid NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "Info";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public string? ActionUrl { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }
}
