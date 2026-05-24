using System.ComponentModel.DataAnnotations;

namespace Resort.Model.Models
{
    public class Room : BaseEntity, IAuditTracker
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string RoomType { get; set; } = string.Empty;   // Standard | Deluxe | Suite | Villa

        [Required]
        [Range(0, double.MaxValue)]
        public decimal PricePerNight { get; set; }

        public int Capacity { get; set; } = 2;

        public string Floor { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Amenities { get; set; } = string.Empty;  // comma-separated

        [Required]
        public string Status { get; set; } = RoomStatus.Available;  // Available | Occupied | Maintenance | Cleaning

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }

        // Navigation
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<RepairRequest> RepairRequests { get; set; } = new List<RepairRequest>();
    }

    public static class RoomStatus
    {
        public const string Available = "Available";
        public const string Occupied = "Occupied";
        public const string Maintenance = "Maintenance";
        public const string Cleaning = "Cleaning";
    }
}
