using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            return await _notificationRepository.CreateAsync(notification);
        }

        public async Task CreateBookingNotificationAsync(Guid patientId, string doctorName, DateTime appointmentDate, string appointmentTime, string hospitalName, string appointmentId)
        {
            var notification = new Notification
            {
                UserId = patientId,
                UserType = "Patient",
                Title = "Appointment Confirmed! ✅",
                Message = $"Your appointment with Dr. {doctorName} on {appointmentDate:MMM dd, yyyy} at {appointmentTime} at {hospitalName} has been confirmed.",
                Type = "Booking",
                RelatedEntityId = appointmentId,
                RelatedEntityType = "Appointment",
                ActionUrl = "/my-appointments"
            };

            await _notificationRepository.CreateAsync(notification);
        }

        public async Task CreateBookingCancellationNotificationAsync(Guid patientId, string doctorName, DateTime appointmentDate, string appointmentTime, string appointmentId)
        {
            var notification = new Notification
            {
                UserId = patientId,
                UserType = "Patient",
                Title = "Appointment Cancelled ❌",
                Message = $"Your appointment with Dr. {doctorName} on {appointmentDate:MMM dd, yyyy} at {appointmentTime} has been cancelled.",
                Type = "Warning",
                RelatedEntityId = appointmentId,
                RelatedEntityType = "Appointment",
                ActionUrl = "/my-appointments"
            };

            await _notificationRepository.CreateAsync(notification);
        }

        public async Task CreatePrescriptionNotificationAsync(Guid patientId, string doctorName, string diagnosis, string recordId)
        {
            var notification = new Notification
            {
                UserId = patientId,
                UserType = "Patient",
                Title = "New Prescription Available 💊",
                Message = $"Dr. {doctorName} has issued a prescription for you. Diagnosis: {diagnosis}. Check your email for details.",
                Type = "Prescription",
                RelatedEntityId = recordId,
                RelatedEntityType = "Prescription",
                ActionUrl = "/my-prescriptions"
            };

            await _notificationRepository.CreateAsync(notification);
        }

        public async Task CreatePaymentNotificationAsync(Guid patientId, decimal amount, string transactionId, string appointmentId)
        {
            var notification = new Notification
            {
                UserId = patientId,
                UserType = "Patient",
                Title = "Payment Successful 💳",
                Message = $"Your payment of LKR {amount:N2} has been processed successfully. Transaction ID: {transactionId}",
                Type = "Payment",
                RelatedEntityId = appointmentId,
                RelatedEntityType = "Payment",
                ActionUrl = "/my-appointments"
            };

            await _notificationRepository.CreateAsync(notification);
        }

        public async Task<IEnumerable<NotificationDto>> GetByUserIdAsync(Guid userId, string userType)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId, userType);
            return notifications.Select(MapToDto);
        }

        public async Task<IEnumerable<NotificationDto>> GetUnreadByUserIdAsync(Guid userId, string userType)
        {
            var notifications = await _notificationRepository.GetUnreadByUserIdAsync(userId, userType);
            return notifications.Select(MapToDto);
        }

        public async Task<int> GetUnreadCountAsync(Guid userId, string userType)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId, userType);
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId)
        {
            return await _notificationRepository.MarkAsReadAsync(notificationId);
        }

        public async Task<bool> MarkAllAsReadAsync(Guid userId, string userType)
        {
            return await _notificationRepository.MarkAllAsReadAsync(userId, userType);
        }

        public async Task<bool> DeleteAsync(Guid notificationId)
        {
            return await _notificationRepository.DeleteAsync(notificationId);
        }

        public async Task<bool> DeleteAllByUserIdAsync(Guid userId, string userType)
        {
            return await _notificationRepository.DeleteAllByUserIdAsync(userId, userType);
        }

        private static NotificationDto MapToDto(Notification n)
        {
            return new NotificationDto
            {
                NotificationId = n.NotificationId,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                RelatedEntityId = n.RelatedEntityId,
                RelatedEntityType = n.RelatedEntityType,
                ActionUrl = n.ActionUrl,
                TimeAgo = GetTimeAgo(n.CreatedAt)
            };
        }

        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} min ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours > 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays > 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} week{((int)(timeSpan.TotalDays / 7) > 1 ? "s" : "")} ago";
            
            return dateTime.ToString("MMM dd, yyyy");
        }
    }
}
