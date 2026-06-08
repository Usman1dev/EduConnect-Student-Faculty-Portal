using System;
using System.Collections.Generic;
using System.Linq;
using EduConnect.Data;
using EduConnect.Models;

namespace EduConnect.Services
{
    /// <summary>
    /// SRP: Owns notification storage, read status, and notification events for reactive UI updates.
    /// DIP: Injects EduConnectDbContext through DI.
    /// </summary>
    public class NotificationService
    {
        public event Action<Notification>? OnNewNotification;
        private readonly EduConnectDbContext _context;

        public NotificationService(EduConnectDbContext context)
        {
            _context = context;
        }

        public void AddNotification(Notification n)
        {
            if (n.Id == Guid.Empty) n.Id = Guid.NewGuid();
            if (n.Timestamp == default) n.Timestamp = DateTime.Now;
            _context.Notifications.Add(n);
            _context.SaveChanges();
            OnNewNotification?.Invoke(n);
        }

        public void AddNotification(string message, NotificationType type, Guid userId) => AddNotification(new Notification { Message = message, NotificationType = type, UserId = userId, Timestamp = DateTime.Now });
        public List<Notification> GetForUser(Guid userId) => _context.Notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.Timestamp).ToList();
        public int GetUnreadCount(Guid userId) => _context.Notifications.Count(n => n.UserId == userId && !n.IsRead);

        public void MarkAsRead(Guid notificationId)
        {
            var n = _context.Notifications.FirstOrDefault(x => x.Id == notificationId);
            if (n != null) { n.IsRead = true; _context.SaveChanges(); }
        }

        public void MarkAllAsRead(Guid userId)
        {
            foreach (var n in _context.Notifications.Where(x => x.UserId == userId)) n.IsRead = true;
            _context.SaveChanges();
        }
    }
}
