using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Resort.Model.Models
{
    public class Service : BaseEntity, IAuditTracker
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string Unit { get; set; } = "/lần";

        public string ServiceType { get; set; } = "Tiện ích";
        // Ăn uống | Chăm sóc | Giải trí | Vận chuyển | Tiện ích

        public bool IsActive { get; set; } = true;

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }

        // Navigation
        public virtual ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
    }

    public class BookingService : BaseEntity, IAuditTracker
    {
        [Required]
        public string BookingId { get; set; } = string.Empty;

        [Required]
        public string ServiceId { get; set; } = string.Empty;

        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public DateTime ServiceDate { get; set; }

        public string Notes { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }

        // Navigation
        public virtual Booking Booking { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
    }
}
