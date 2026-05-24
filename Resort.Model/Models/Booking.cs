using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Resort.Model.Models
{
    public class Booking : BaseEntity, IAuditTracker
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string GuestId { get; set; } = string.Empty;

        [Required]
        public string RoomId { get; set; } = string.Empty;

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        public DateTime? ActualCheckIn { get; set; }

        public DateTime? ActualCheckOut { get; set; }

        public int NumberOfGuests { get; set; } = 1;

        [Required]
        public string Status { get; set; } = BookingStatus.Pending;
        // Pending | Confirmed | CheckedIn | CheckedOut | Cancelled

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Deposit { get; set; }

        public string Notes { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }

        // Navigation
        public virtual Guest Guest { get; set; } = null!;
        public virtual Room Room { get; set; } = null!;
        public virtual ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
    }

    public static class BookingStatus
    {
        public const string Pending = "Pending";
        public const string Confirmed = "Confirmed";
        public const string CheckedIn = "CheckedIn";
        public const string CheckedOut = "CheckedOut";
        public const string Cancelled = "Cancelled";
    }
}
