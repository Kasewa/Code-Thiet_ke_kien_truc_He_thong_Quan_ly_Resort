using System.ComponentModel.DataAnnotations;

namespace Resort.Model.Models
{
    public class Staff : BaseEntity, IAuditTracker
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string IdCard { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        public string Gender { get; set; } = "Nam";

        [Required]
        public string Position { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        [Phone]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public decimal Salary { get; set; }

        public string Shift { get; set; } = "Ca sáng (6:00-14:00)";

        public bool IsActive { get; set; } = true;

        // Link to ASP.NET Identity user
        public string? UserId { get; set; }

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }
    }
}
