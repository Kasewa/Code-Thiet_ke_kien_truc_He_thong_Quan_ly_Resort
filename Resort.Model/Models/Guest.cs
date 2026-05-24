using System.ComponentModel.DataAnnotations;

namespace Resort.Model.Models
{
    public class Guest : BaseEntity, IAuditTracker
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string IdCard { get; set; } = string.Empty;

        public string Nationality { get; set; } = "Việt Nam";

        public string Gender { get; set; } = "Nam";

        public DateTime? DateOfBirth { get; set; }

        [Phone]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }

        // Navigation
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
