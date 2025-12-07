using HospitalManagementSystem.Domain.IRepository;
using HospitalManagementSystem.Domain.Models;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Infrastructure.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _dbContext;

        public NotificationRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            notification.NotificationId = Guid.NewGuid();
            notification.CreatedAt = DateTime.UtcNow;
            await _dbContext.Notifications.AddAsync(notification);
            await _dbContext.SaveChangesAsync();
            return notification;
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Notifications.FindAsync(id);
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, string userType)
        {
            return await _dbContext.Notifications
                .Where(n => n.UserId == userId && n.UserType == userType)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50) // Limit to last 50 notifications
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId, string userType)
        {
            return await _dbContext.Notifications
                .Where(n => n.UserId == userId && n.UserType == userType && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId, string userType)
        {
            return await _dbContext.Notifications
                .CountAsync(n => n.UserId == userId && n.UserType == userType && !n.IsRead);
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _dbContext.Notifications.FindAsync(notificationId);
            if (notification == null) return false;

            notification.IsRead = true;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(Guid userId, string userType)
        {
            var notifications = await _dbContext.Notifications
                .Where(n => n.UserId == userId && n.UserType == userType && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var notification = await _dbContext.Notifications.FindAsync(id);
            if (notification == null) return false;

            _dbContext.Notifications.Remove(notification);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAllByUserIdAsync(Guid userId, string userType)
        {
            var notifications = await _dbContext.Notifications
                .Where(n => n.UserId == userId && n.UserType == userType)
                .ToListAsync();

            _dbContext.Notifications.RemoveRange(notifications);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
