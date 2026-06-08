using System;
using System.ComponentModel.DataAnnotations;

namespace EduConnect.Models
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Message { get; set; } = "";
        public NotificationType NotificationType { get; set; }
        public Guid UserId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // EF Core navigation property
        public Person? User { get; set; }
    }
}