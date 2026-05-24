using System.ComponentModel.DataAnnotations;

namespace Resort.Model.Models
{
    public class RepairRequest : BaseEntity, IAuditTracker
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string RoomId { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string Priority { get; set; } = RepairPriority.Normal;
        // Urgent | Normal | Low

        public string TechnicianId { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = RepairStatus.Pending;
        // Pending | InProgress | Completed | Cancelled

        public DateTime? CompletedDate { get; set; }

        public string Notes { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }

        // Navigation
        public virtual Room Room { get; set; } = null!;
    }

    public static class RepairPriority
    {
        public const string Urgent = "Urgent";
        public const string Normal = "Normal";
        public const string Low = "Low";
    }

    public static class RepairStatus
    {
        public const string Pending = "Pending";
        public const string InProgress = "InProgress";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }
}
