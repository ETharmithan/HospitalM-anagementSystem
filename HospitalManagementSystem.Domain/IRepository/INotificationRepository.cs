using HospitalManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Domain.IRepository
{
    public interface INotificationRepository
    {
        Task<Notification> CreateAsync(Notification notification);
        Task<Notification?> GetByIdAsync(Guid id);
        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, string userType);
        Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId, string userType);
        Task<int> GetUnreadCountAsync(Guid userId, string userType);
        Task<bool> MarkAsReadAsync(Guid notificationId);
        Task<bool> MarkAllAsReadAsync(Guid userId, string userType);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeleteAllByUserIdAsync(Guid userId, string userType);
    }
}
