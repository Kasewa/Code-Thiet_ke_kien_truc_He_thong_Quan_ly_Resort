using System.ComponentModel.DataAnnotations;

namespace Resort.Model.Models
{
    public class Notification : BaseEntity, IAuditTracker
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string SenderId { get; set; } = string.Empty;

        public string RecipientId { get; set; } = string.Empty;   // empty = broadcast to all

        public bool IsRead { get; set; } = false;

        public string NotificationType { get; set; } = "General";
        // General | Booking | Repair | System

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }
    }
}
